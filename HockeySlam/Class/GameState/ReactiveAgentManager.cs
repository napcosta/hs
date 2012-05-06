using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities;
using HockeySlam.Interface;


namespace HockeySlam.Class.GameState
{
	class ReactiveAgentManager: IGameEntity
	{

		GameManager _gameManager;
		Game _game;
		Camera _camera;
		bool _addAgentKeyPressed;

		List<ReactiveAgent> playerList = new List<ReactiveAgent>();

		public ReactiveAgentManager(GameManager gameManager, Game game, Camera camera)
		{
			_gameManager = gameManager;
			_game = game;
			_camera = camera;
		}

		public void addReactiveAgent()
		{
			playerList.Add(new ReactiveAgent(_gameManager, _game, _camera));
		}

		public void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();

			if (keyboard.IsKeyDown(Keys.R) && !_addAgentKeyPressed) {
				addReactiveAgent();
				_addAgentKeyPressed = true;
			} else if (keyboard.IsKeyUp(Keys.R) && _addAgentKeyPressed)
				_addAgentKeyPressed = false;

			foreach (ReactiveAgent agent in playerList) {
				agent.update(gameTime);
			}
		}

		public void Draw(GameTime gameTime)
		{
			foreach (ReactiveAgent agent in playerList) {
				agent.draw(gameTime);
			}
		}

		public void Initialize()
		{
			_addAgentKeyPressed = false;
		}

		public void LoadContent()
		{}
	}
}
