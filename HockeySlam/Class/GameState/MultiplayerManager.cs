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
using Microsoft.Xna.Framework.Input;

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
		int servUpdate = 0;
		KeyboardState currentKeyboardState;
		KeyboardState previousKeyboardState;
		float _currentSmoothing;
		SpriteBatch _spriteBatch;
		SpriteFont _font;
		// latency and packet loss simulation
		enum NetworkQuality
		{
			Typical,	// 100ms latency, 10% packet loss
			Poor,		// 200ms latency, 20% packet loss
			Perfect,	// 0ms latency, 0% packet loss
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
			_networkSession.SimulatedLatency = TimeSpan.FromMilliseconds(200.0f);
		}

		public void Draw(GameTime gameTime)
		{

			foreach (NetworkGamer gamer in _networkSession.AllGamers) {

				Player player = gamer.Tag as Player;

				player.Draw(gameTime);
			}
			_disk.Draw(gameTime);
		}

		public void Update(GameTime gameTime)
		{
			UpdateNetworkSession(gameTime);
		}


		/// <summary>
		/// Updates locally, reads the player's input and sends it to the server
		/// </summary>
		void LocalGamerUpdate(LocalNetworkGamer gamer, GameTime gameTime)
		{
			/* Look up what player is associated with this local player,
			 * and read the latest user inputs for it. The server will later
			 * use these values to control the player movement. */
			Player localPlayer = gamer.Tag as Player;

			ReadPlayerInput(localPlayer, gamer.SignedInGamer.PlayerIndex);

			/* Only send if we are not the server. There is no point sending packets
			 * to ourselves, because we already know what they will contain */
			if (!_networkSession.IsHost) {
				_packetWriter.Write(localPlayer.PositionInput);
				_packetWriter.Write(localPlayer.RotationInput);

				gamer.SendData(_packetWriter, SendDataOptions.InOrder, _networkSession.Host);
			} else {
				localPlayer.UpdateState(ref localPlayer._simulationState);
				localPlayer._displayState = localPlayer._simulationState;
			}

			// The client as well as the host are updating. The client will sync with the
			// server later
			localPlayer.Update(gameTime);
			localPlayer._displayState = localPlayer._simulationState;
			if(!gamer.IsHost)
				_disk.Update(gameTime);
		}

		void ReadPlayerInput(Player player, PlayerIndex playerIndex)
		{
			Vector2 positionInput = Vector2.Zero;

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
			}
			else if (keyboard.IsKeyUp(Keys.Down) && _rotationInput.Y != 0) {
				keyPriority = _rotationInput.Y - 1;
				_rotationInput.Y = 0;
				_priority--;
			}
			else if (keyboard.IsKeyUp(Keys.Left) && _rotationInput.Z != 0) {
				keyPriority = _rotationInput.Z - 1;
				_rotationInput.Z = 0;
				_priority--;
			}
			else if (keyboard.IsKeyUp(Keys.Right) && _rotationInput.W != 0) {
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

			player.updatePositionInput(positionInput);
			player.updateRotationInput(_rotationInput);

			previousKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();
			UpdateOptions();
		}

		/// <summary>
		/// Updates the server and sends the packets to the clients
		/// </summary>
		void UpdateServer(GameTime gameTime)
		{
			_disk.Update(gameTime);
			Vector3 diskPosition = _disk.getPosition();
			
			foreach (NetworkGamer gamer in _networkSession.AllGamers) {

				Player player = gamer.Tag as Player;

				if (!gamer.IsHost)
					player.Update(gameTime);
				
				_packetWriter.Write(diskPosition);
				_packetWriter.Write(gamer.Id);

				_packetWriter.Write(player._simulationState.Position);
				_packetWriter.Write(player._simulationState.Rotation);
				_packetWriter.Write(player._simulationState.Velocity);
			//	_packetWriter.Write(player.getPositionVector());
			//	_packetWriter.Write(player.Rotation);

			//Send the combined data for all players to everyone in the session.
				LocalNetworkGamer server = (LocalNetworkGamer)_networkSession.Host;
				server.SendData(_packetWriter, SendDataOptions.InOrder);
			}
			
		}

		public List<Player> GetPlayers()
		{
			List<Player> players = new List<Player>();
			foreach(Gamer gamer in _networkSession.AllGamers) {
				Player player = gamer.Tag as Player;
				players.Add(player);
			}

			return players;
		}

		void ServerReadInputFromClients(LocalNetworkGamer gamer)
		{
			//keep reading as long as incoming packets are available
			while (gamer.IsDataAvailable) {
				NetworkGamer sender;

				gamer.ReceiveData(_packetReader, out sender);

				if (!sender.IsLocal) {
					//Look up the player associated with whoever sent this packet.
					Player remotePlayer = sender.Tag as Player;

					remotePlayer.PositionInput = _packetReader.ReadVector2();
					remotePlayer.RotationInput = _packetReader.ReadVector4();
				}
			}
		}

		void ClientReadGameStateFromServer(LocalNetworkGamer gamer)
		{

			float smoothingDecay = 1.0f / _framesBetweenPackets;

			_currentSmoothing -= smoothingDecay;

			if (_currentSmoothing < 0)
				_currentSmoothing = 0;

			while (gamer.IsDataAvailable) {
				NetworkGamer sender;

				gamer.ReceiveData(_packetReader, out sender);
				Vector3 diskPosition = _packetReader.ReadVector3();
				_disk.synchPosition(diskPosition);
				while (_packetReader.Position < _packetReader.Length) {
					//Read the state of one Player from the network packet

					byte gamerId = _packetReader.ReadByte();
					Vector3 position = _packetReader.ReadVector3();
					float rotation = _packetReader.ReadSingle();

					Vector2 velocity = _packetReader.ReadVector2();


					NetworkGamer remoteGamer = _networkSession.FindGamerById(gamerId);

					if (remoteGamer != null) {
						Player player = remoteGamer.Tag as Player;
						player._simulationState.Velocity = velocity;
						player._simulationState.Position = position;
						player._simulationState.Rotation = rotation;

						if (_enablePrediction) {
							// Predict how the remote player will move by
							// updating our local copy of its simulation state
							player.UpdateState(ref player._simulationState);
							if (_currentSmoothing > 0) {
								// If both smoothing and prediction are active,
								// also apply prediction to the previous state.
								player.UpdateState(ref player._previousState);
							}
						}

						if (_currentSmoothing > 0) {
							// Interpolate the display state gradually from the
							// previous state to the current simulation state
							ApplySmoothing(player);
						} else {
							// Copy the simulation state directly into the display state.
							player._displayState = player._simulationState;
						}
						/*player._displayState.Position = position;
						player._displayState.Rotation = rotation;*/
						//player.setPositionVector(position);
						//player.Rotation = rotation;
						
					}
				}
			}
		}

		private void ApplySmoothing(Player player)
		{
			player._displayState.Position = Vector3.Lerp(player._simulationState.Position,
								     player._previousState.Position,
								     _currentSmoothing);

			player._displayState.Velocity = Vector2.Lerp(player._simulationState.Velocity,
								     player._previousState.Velocity,
								     _currentSmoothing);
			
			player._displayState.Rotation = MathHelper.Lerp(player._simulationState.Rotation,
								     player._previousState.Rotation,
								     _currentSmoothing);
		}

		void UpdateNetworkSession(GameTime gameTime)
		{
			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers)
				LocalGamerUpdate(gamer, gameTime);

			if (_networkSession.IsHost)
				UpdateServer(gameTime);

			_networkSession.Update();

			if (_networkSession == null)
				return;

			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
				if (gamer.IsHost)
					ServerReadInputFromClients(gamer);
				else 
					ClientReadGameStateFromServer(gamer);

				Player player = gamer.Tag as Player;
				player.updateCameraPosition();
				player.setArrowPlayer();
			}
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
				_networkSession.SessionProperties[3] = _enablePrediction ? 1 : 0;
			} else {
				_networkQuality = (NetworkQuality)_networkSession.SessionProperties[0];
				_framesBetweenPackets = _networkSession.SessionProperties[1].Value;
				_enablePrediction = _networkSession.SessionProperties[2] != 0;
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

		bool IsPressed(Keys key)
		{
			return ((currentKeyboardState.IsKeyDown(key) &&
				 previousKeyboardState.IsKeyUp(key)));
		}

		private void HandleInput()
		{
			previousKeyboardState = currentKeyboardState;

			currentKeyboardState = Keyboard.GetState();
		}

		public Disk getDisk()
		{
			return _disk;
		}

		/// <summary>
		/// Draws the current latency and packet loss simulation settings.
		/// </summary>
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

			_game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

	}
}
