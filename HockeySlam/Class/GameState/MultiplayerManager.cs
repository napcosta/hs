using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

using HockeySlam.Interface;
using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.Networking;

namespace HockeySlam.Class.GameState
{
	class MultiplayerManager : IGameEntity
	{

		Dictionary<NetworkGamer, Player> _playersInGame;
		Camera _camera;
		GameManager _gameManager;
		Game _game;
		Player _localPlayer;

		NetworkSession _networkSession;
		PacketReader _packetReader;
		PacketWriter _packetWriter;

		public MultiplayerManager(Game game, Camera camera, GameManager gameManager, NetworkSession networkSession, Player localPlayer)
		{
			_game = game;
			_camera = camera;
			_gameManager = gameManager;
			_networkSession = networkSession;
			_localPlayer = localPlayer;
		}

		public void LoadContent()
		{ }

		public void Initialize()
		{
			_playersInGame = new Dictionary<NetworkGamer, Player>();
			_packetReader = new PacketReader();
			_packetWriter = new PacketWriter();
		}


		public void Draw(GameTime gameTime)
		{
			foreach (KeyValuePair<NetworkGamer, Player> gamer in _playersInGame) {
				gamer.Value.Draw(gameTime);
			}
		}

		public void Update(GameTime gameTime)
		{ 
			GamerCollection<NetworkGamer> _remoteGamers = _networkSession.RemoteGamers;

			foreach (NetworkGamer gamer in _remoteGamers) {
				if (_playersInGame.Count >= _remoteGamers.Count)
					break;
				if (_playersInGame.ContainsKey(gamer))
					continue;

				addRemotePlayer(gamer);
			}

			updateGamers(gameTime);
		}

		private void updateGamers(GameTime gameTime)
		{
			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
				_packetWriter.Write(_localPlayer.getVelocity());
				_packetWriter.Write(0.0f);

				gamer.SendData(_packetWriter, SendDataOptions.InOrder);
			}

			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
				while (gamer.IsDataAvailable) {
					NetworkGamer sender;
					gamer.ReceiveData(_packetReader, out sender);

					if (sender.IsLocal)
						continue;

					Player player = _playersInGame[sender];

					player.updateVelocity(_packetReader.ReadVector2());
				}
			}

			foreach(KeyValuePair<NetworkGamer, Player> player in _playersInGame)
				player.Value.Update(gameTime);
		}	


		private void addRemotePlayer(NetworkGamer gamer)
		{
			Player newPlayer = new Player(_gameManager, _game, _camera, true);
			_playersInGame.Add(gamer, newPlayer);

			newPlayer.Initialize();
			newPlayer.LoadContent();
		}
	}
}
