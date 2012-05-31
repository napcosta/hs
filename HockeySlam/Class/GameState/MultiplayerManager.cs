using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using HockeySlam.Interface;
using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.Networking;

namespace HockeySlam.Class.GameState
{
	public class MultiplayerManager : IGameEntity
	{

		//Dictionary<NetworkGamer, Player> _playersInGame;
		Camera _camera;
		GameManager _gameManager;
		Game _game;
		const int _maxLocalGamers = 4;
		const int _maxGamers = 8;
		NetworkSession _networkSession;
		PacketReader _packetReader;
		PacketWriter _packetWriter;
		Vector2 _lastPositionInput; // A temporary vector to avoid WASD conflicts (like pressing A & D at the same time)
		Vector4 _rotationInput;
		Disk _disk;
		int _priority;

		SpriteBatch _spriteBatch;
		SpriteFont _font;

		KeyboardState _currentKeyboardState;
		KeyboardState _previousKeyboardState;

		enum NetworkQuality
		{
			Typical,	// 100 ms latency, 10% packet loss
			Poor,		// 200 ms latency, 20% packet loss
			Perfect,	// 0 latency, 0% packet loss
		}

		NetworkQuality _networkQuality;

		int _framesBetweenPackets = 6;

		int _framesSinceLastSend;

		bool _enablePrediction = true;
		bool _enableSmoothing = true;

		public MultiplayerManager(Game game, Camera camera, GameManager gameManager, NetworkSession networkSession)
		{
			_game = game;
			_camera = camera;
			_gameManager = gameManager;
			_networkSession = networkSession;
		}

		public void LoadContent()
		{
			_spriteBatch = new SpriteBatch(_game.GraphicsDevice);
			_font = _game.Content.Load<SpriteFont>("Fonts/GameFont");
		}

		public void Initialize()
		{
			//_playersInGame = new Dictionary<NetworkGamer, Player>();
			_packetReader = new PacketReader();
			_packetWriter = new PacketWriter();
			_lastPositionInput = Vector2.Zero;
			_rotationInput = Vector4.Zero;
			_priority = 0;
			_disk = new Disk(_gameManager, _game, _camera, false);
			_disk.Initialize();
		}

		public void Draw(GameTime gameTime)
		{
			foreach (NetworkGamer gamer in _networkSession.AllGamers) {

				Player player = gamer.Tag as Player;

				player.Draw(gameTime);
			}
			_disk.Draw(gameTime);
		}

		private void HandleInput()
		{
			_previousKeyboardState = _currentKeyboardState;

			_currentKeyboardState = Keyboard.GetState();
		}

		bool IsPressed(Keys key)
		{
			return ((_currentKeyboardState.IsKeyDown(key) &&
			_previousKeyboardState.IsKeyUp(key)));
		}

		private void UpdateOptions()
		{
			if (_networkSession.IsHost) {
				if (IsPressed(Keys.U)) {
					_networkQuality++;

					if (_networkQuality > NetworkQuality.Perfect)
						_networkQuality = 0;
				}

				if (IsPressed(Keys.I)) {
					if (_framesBetweenPackets == 6)
						_framesBetweenPackets = 3;
					else if (_framesBetweenPackets == 3)
						_framesBetweenPackets = 1;
					else
						_framesBetweenPackets = 6;
				}

				if (IsPressed(Keys.O))
					_enablePrediction = !_enablePrediction;

				if (IsPressed(Keys.P))
					_enableSmoothing = !_enableSmoothing;

				_networkSession.SessionProperties[0] = (int)_networkQuality;
				_networkSession.SessionProperties[1] = _framesBetweenPackets;
				_networkSession.SessionProperties[2] = _enablePrediction ? 1 : 0;
				_networkSession.SessionProperties[3] = _enableSmoothing ? 1 : 0;
			} else {
				if (_networkSession.SessionProperties[0] != null)
					_networkQuality = (NetworkQuality)_networkSession.SessionProperties[0];
				if (_networkSession.SessionProperties[1] != null)
					_framesBetweenPackets = _networkSession.SessionProperties[1].Value;
				if (_networkSession.SessionProperties[2] != null)
					_enablePrediction = _networkSession.SessionProperties[2] != 0;
				if (_networkSession.SessionProperties[3] != null)
					_enableSmoothing = _networkSession.SessionProperties[3] != 0;
			}

			switch (_networkQuality) {
			case NetworkQuality.Typical:
				_networkSession.SimulatedLatency = TimeSpan.FromMilliseconds(100);
				_networkSession.SimulatedPacketLoss = 0.1f;
				break;
			case NetworkQuality.Poor:
				_networkSession.SimulatedLatency = TimeSpan.FromMilliseconds(200);
				_networkSession.SimulatedPacketLoss = 0.2f;
				break;

			case NetworkQuality.Perfect:
				_networkSession.SimulatedLatency = TimeSpan.Zero;
				_networkSession.SimulatedPacketLoss = 0;
				break;

			}
		}

		public void Update(GameTime gameTime)
		{
			HandleInput();
			UpdateNetworkSession(gameTime);
			UpdateOptions();
		}

		void UpdateLocalGamer(LocalNetworkGamer gamer, GameTime gameTime)
		{
			/* Look up what player is associated with this local player,
			 * and read the latest user inputs for it. The server will later
			 * use these values to control the player movement. */
			Player localPlayer = gamer.Tag as Player;

			Vector2 positionInput;
			Vector4 rotationInput;

			ReadPlayerInput(gamer.SignedInGamer.PlayerIndex, out positionInput, out rotationInput);

			/* Only send if we are not the server. There is no point sending packets
			 * to ourselves, because we already know what they will contain */
			localPlayer.UpdateLocal(positionInput, rotationInput, gameTime);

			if (!_networkSession.IsHost) {
				foreach (Gamer remoteGamer in _networkSession.RemoteGamers) {
					Player player = remoteGamer.Tag as Player;
					player.UpdateRemote(_framesBetweenPackets, _enablePrediction, gameTime);
				}
				
				localPlayer.ClientWriteNetworkPacket(_packetWriter);
				gamer.SendData(_packetWriter, SendDataOptions.InOrder, _networkSession.Host);
			}
		}

		void ReadPlayerInput(PlayerIndex playerIndex, out Vector2 positionInput, out Vector4 rotationInput)
		{
			positionInput = Vector2.Zero;
			rotationInput = Vector4.Zero;

			KeyboardState keyboard = Keyboard.GetState(playerIndex);

			if (keyboard.IsKeyDown(Keys.A) && keyboard.IsKeyDown(Keys.D))
				if (_lastPositionInput.X == 1)
					positionInput.X = 2;
				else
					positionInput.X = 1;
			else {
				if (keyboard.IsKeyDown(Keys.A))
					_lastPositionInput.X = 1;
				else if (keyboard.IsKeyDown(Keys.D))
					_lastPositionInput.X = 2;
				else
					_lastPositionInput.X = 0;

				positionInput.X = _lastPositionInput.X;
			}

			if (keyboard.IsKeyDown(Keys.W) && keyboard.IsKeyDown(Keys.S))
				if (_lastPositionInput.Y == 1)
					positionInput.Y = 2;
				else
					positionInput.Y = 1;
			else {
				if (keyboard.IsKeyDown(Keys.W))
					_lastPositionInput.Y = 1;
				else if (keyboard.IsKeyDown(Keys.S))
					_lastPositionInput.Y = 2;
				else
					_lastPositionInput.Y = 0;

				positionInput.Y = _lastPositionInput.Y;
			}

			if (keyboard.IsKeyDown(Keys.Up) && _rotationInput.X == 0)
				_rotationInput.X = _priority++;
			if (keyboard.IsKeyDown(Keys.Down) && _rotationInput.Y == 0)
				_rotationInput.Y = _priority++;
			if (keyboard.IsKeyDown(Keys.Left) && _rotationInput.Z == 0)
				_rotationInput.Z = _priority++;
			if (keyboard.IsKeyDown(Keys.Right) && _rotationInput.W == 0)
				_rotationInput.W = _priority++;

			float keyPriority = 4;
			if (keyboard.IsKeyUp(Keys.Up) && _rotationInput.X != 0) {
				keyPriority = _rotationInput.X - 1;
				_rotationInput.X = 0;
				_priority--;
			} else if (keyboard.IsKeyUp(Keys.Down) && _rotationInput.Y != 0) {
				keyPriority = _rotationInput.Y - 1;
				_rotationInput.Y = 0;
				_priority--;
			} else if (keyboard.IsKeyUp(Keys.Left) && _rotationInput.Z != 0) {
				keyPriority = _rotationInput.Z - 1;
				_rotationInput.Z = 0;
				_priority--;
			} else if (keyboard.IsKeyUp(Keys.Right) && _rotationInput.W != 0) {
				keyPriority = _rotationInput.W - 1;
				_rotationInput.W = 0;
				_priority--;
			}

			if (_rotationInput.X > keyPriority)
				_rotationInput.X--;
			if (_rotationInput.Y > keyPriority)
				_rotationInput.Y--;
			if (_rotationInput.Z > keyPriority)
				_rotationInput.Z--;
			if (_rotationInput.W > keyPriority)
				_rotationInput.W--;
		}

		/// <summary>
		/// Updates the server and sends the packets to the clients
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="sendPacketThisFrame"></param>
		void UpdateServer(GameTime gameTime, bool sendPacketThisFrame)
		{
			
			foreach (Gamer gamer in _networkSession.RemoteGamers) {
				Player player = gamer.Tag as Player;
				player.UpdateRemoteOnServer(gameTime);
			}
			if (sendPacketThisFrame) {
				_packetWriter.Write((float)gameTime.TotalGameTime.TotalSeconds);
				//Vector3 diskPosition = _disk.getPosition();
				//_packetWriter.Write(diskPosition);
				_disk.ServerWriteNetworkPacket(_packetWriter);
				foreach (NetworkGamer gamer in _networkSession.AllGamers) {

					Player player = gamer.Tag as Player;

					_packetWriter.Write(gamer.Id);
					player.ServerWriteNetworkPacket(_packetWriter);
				}
				//Send the combined data for all players to everyone in the session.
				LocalNetworkGamer server = (LocalNetworkGamer)_networkSession.Host;
				server.SendData(_packetWriter, SendDataOptions.InOrder);
			}
		}

		public List<Player> GetPlayers()
		{
			List<Player> players = new List<Player>();
			foreach (Gamer gamer in _networkSession.AllGamers) {
				Player player = gamer.Tag as Player;
				players.Add(player);
			}

			return players;
		}

		void ServerReadInputFromClients(LocalNetworkGamer gamer, GameTime gameTime)
		{
			//keep reading as long as incoming packets are available
			while (gamer.IsDataAvailable) {
				NetworkGamer sender;

				gamer.ReceiveData(_packetReader, out sender);

				if (!sender.IsLocal) {
					//Look up the player associated with whoever sent this packet.
					Player remotePlayer = sender.Tag as Player;

					remotePlayer.ReadInputFromClient(_packetReader, gameTime);
				}
			}
		}

		void ClientReadGameStateFromServer(LocalNetworkGamer gamer, GameTime gameTime)
		{
			while (gamer.IsDataAvailable) {
				NetworkGamer sender;
				gamer.ReceiveData(_packetReader, out sender);
				TimeSpan latency = _networkSession.SimulatedLatency +
								   TimeSpan.FromTicks(sender.RoundtripTime.Ticks / 2);
				float packetSendTime = _packetReader.ReadSingle();
				//Vector3 diskPosition = _packetReader.ReadVector3();
				//_disk.setPosition(diskPosition);
				_disk.ReadNetworkPacket(_packetReader, gameTime, latency, _enablePrediction, _enableSmoothing, packetSendTime);
				_disk.UpdateRemote(_framesBetweenPackets, _enablePrediction, gameTime);
				
				while (_packetReader.Position < _packetReader.Length) {
					//Read the state of one Player from the network packet

					byte gamerId = _packetReader.ReadByte();

					NetworkGamer remoteGamer = _networkSession.FindGamerById(gamerId);

					if (remoteGamer != null) {
						Player player = remoteGamer.Tag as Player;

						

						player.ReadNetworkPacket(_packetReader, gameTime, latency, _enablePrediction, _enableSmoothing, packetSendTime);
						player.UpdateRemote(_framesBetweenPackets, _enablePrediction, gameTime);

						if (remoteGamer.IsLocal) {
							player.updateCameraPosition();
							player.setArrowPlayer();
						}
					}
				}

			}
		}

		void UpdateNetworkSession(GameTime gameTime)
		{
			bool sendPacketThisFrame = false;

			_framesSinceLastSend++;

			if (_framesSinceLastSend >= _framesBetweenPackets) {
				sendPacketThisFrame = true;
				_framesSinceLastSend = 0;
			}

			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers)
				UpdateLocalGamer(gamer, gameTime);
			_disk.UpdateLocal(gameTime);
			if (_networkSession.IsHost)
				UpdateServer(gameTime, sendPacketThisFrame);

			_networkSession.Update();

			if (_networkSession == null)
				return;

			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
				if (gamer.IsHost)
					ServerReadInputFromClients(gamer, gameTime);
				else
					ClientReadGameStateFromServer(gamer, gameTime);

				Player player = gamer.Tag as Player;
				player.updateCameraPosition();
				player.setArrowPlayer();
			}
		}

		public Disk getDisk()
		{
			return _disk;
		}

		public void DrawOptions()
		{
			_spriteBatch.Begin();
			string quality =
			    string.Format("Network simulation = {0} ms, {1}% packet loss",
					  _networkSession.SimulatedLatency.TotalMilliseconds,
					  _networkSession.SimulatedPacketLoss * 100);

			string sendRate = string.Format("Packets per second = {0}",
							60 / _framesBetweenPackets);

			string prediction = string.Format("Prediction = {0}",
							  _enablePrediction ? "on" : "off");

			string smoothing = string.Format("Smoothing = {0}",
							 _enableSmoothing ? "on" : "off");

			// If we are the host, include prompts telling how to change the settings.
			if (_networkSession.IsHost) {
				quality += " (A to change)";
				sendRate += " (B to change)";
				prediction += " (X to toggle)";
				smoothing += " (Y to toggle)";
			}

			// Draw combined text to the screen.
			string message = quality + "\n" +
					 sendRate + "\n" +
					 prediction + "\n" +
					 smoothing;

			_spriteBatch.DrawString(_font, message, new Vector2(161, 321), Color.Black);
			_spriteBatch.DrawString(_font, message, new Vector2(160, 320), Color.White);

			_spriteBatch.End();
		}

	}
}
