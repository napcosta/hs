using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Class.GameState;

namespace HockeySlam.Class.Screens
{
	class MultiplayerGameplayScreen : GameplayScreen
	{

		NetworkSession _networkSession;

		public MultiplayerGameplayScreen(NetworkSession networkSession)
		{
			_networkSession = networkSession;

			TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}

		public override void addGameEntities()
		{
			Camera camera = new Camera(ScreenManager.Game, new Vector3(85, 85, 0), Vector3.Zero, new Vector3(0, 1, 0));
			_gameManager.AddEntity("camera1", camera);
			_gameManager.AddEntity("debugManager", new DebugManager());
			_gameManager.AddEntity("collisionManager", new CollisionManager());
			//_gameManager.AddEntity("court", new Court(ScreenManager.Game, camera));
			Player localPlayer = new Player(_gameManager, ScreenManager.Game, camera, false);
			_gameManager.AddEntity("player1", localPlayer);
			_gameManager.AddEntity("multiplayerManager", new MultiplayerManager(ScreenManager.Game, camera, _gameManager, _networkSession, localPlayer));
			_gameManager.AddEntity("disk", new Disk(_gameManager, ScreenManager.Game, camera));
			_gameManager.AddEntity("ice", new Ice(ScreenManager.Game, camera));
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			
			if (_networkSession != null) {
				foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
					if (!HandlePlayerInput(gameTime, input, gamer.SignedInGamer.PlayerIndex))
						break;
				}
			}
		}
	}
}
