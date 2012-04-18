using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class.Screens
{
	class SinglePlayerGameScreen : GameplayScreen
	{

		public override void addGameEntities()
		{
			Camera camera = new Camera(ScreenManager.Game, new Vector3(85, 85, 0), Vector3.Zero, new Vector3(0, 1, 0));
			_gameManager.AddEntity("camera1", camera);
			_gameManager.AddEntity("debugManager", new DebugManager());
			_gameManager.AddEntity("collisionManager", new CollisionManager());
			//_gameManager.AddEntity("court", new Court(ScreenManager.Game, camera));
			_gameManager.AddEntity("player1", new Player(_gameManager, ScreenManager.Game, camera, false));
			_gameManager.AddEntity("disk", new Disk(_gameManager, ScreenManager.Game, camera));
			_gameManager.AddEntity("ice", new Ice(ScreenManager.Game, camera));
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			HandlePlayerInput(gameTime, input, ControllingPlayer.Value);
		}

	}
}
