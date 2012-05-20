using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Input;

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
		int servUpdate = 0;

		public MultiplayerManager(Game game, Camera camera, GameManager gameManager, NetworkSession networkSession)
		{
			_game = game;
			_camera = camera;
			_gameManager = gameManager;
			_networkSession = networkSession;
		}

		public void LoadContent()
		{ }

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

		public void Update(GameTime gameTime)
		{
			UpdateNetworkSession(gameTime);
		}

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
			}

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
		}

		/* Updates the server and sends the packets to the clients */
		void UpdateServer(GameTime gameTime)
		{

			_disk.Update(gameTime);
			Vector3 diskPosition = _disk.getPosition();
			
			if (servUpdate == 3)
				_packetWriter.Write(diskPosition);
			
			foreach (NetworkGamer gamer in _networkSession.AllGamers) {

				Player player = gamer.Tag as Player;

				player.Update(gameTime);
				if (servUpdate == 3) {
					_packetWriter.Write(gamer.Id);
					_packetWriter.Write(player.getPositionVector());
					_packetWriter.Write(player.Rotation);
				}
			}
			//Send the combined data for all players to everyone in the session.
			LocalNetworkGamer server = (LocalNetworkGamer)_networkSession.Host;
			if (servUpdate == 3) {
				server.SendData(_packetWriter, SendDataOptions.InOrder);
				servUpdate = 0;
			} else {
				servUpdate++;
			}
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
					
					NetworkGamer remoteGamer = _networkSession.FindGamerById(gamerId);

					if (remoteGamer != null) {
						Player player = remoteGamer.Tag as Player;
						player.setPositionVector(position);
						player.Rotation = rotation;
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

		public Disk getDisk()
		{
			return _disk;
		}

	}
}
