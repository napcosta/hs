using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class.Screens
{
	class GameplayScreen : GameScreen
	{
		#region Fields

		ContentManager _content;
		protected GameManager _gameManager;
		SpriteFont _gameFont;

		float _pauseAlpha;

		InputAction _pauseAction;

		#endregion

		#region Initialization

		public GameplayScreen()
		{
			TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			_pauseAction = new InputAction(
				new Buttons[] { Buttons.Start, Buttons.Back },
				new Keys[] { Keys.Escape },
				true);

			_gameManager = null;
		}

		public void addGameManager()
		{
			_gameManager = new GameManager(ScreenManager.Game);
			addGameEntities();
			_gameManager.startGame();
		}

		public virtual void addGameEntities() { }

		public override void Activate(bool instancePreserved)
		{
			if (!instancePreserved)
			{
				if (_content == null)
					_content = new ContentManager(ScreenManager.Game.Services, "Content");

				_gameFont = _content.Load<SpriteFont>("Fonts/GameFont");

				Thread.Sleep(1000);

				ScreenManager.Game.ResetElapsedTime();
			}
		}

		#endregion

		#region Update & Draw

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, false);

			if (_gameManager == null)
				addGameManager();

			if (coveredByOtherScreen)
				_pauseAlpha = Math.Min(_pauseAlpha + 1f / 32, 1);
			else
				_pauseAlpha = Math.Max(_pauseAlpha - 1f / 32, 0);

			if (IsActive)
			{
				_gameManager.Update(gameTime);
			}
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{ }

		protected bool HandlePlayerInput(GameTime gameTime, InputState input, PlayerIndex playerIndex)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			KeyboardState keyboardState = input.CurrentKeyboardStates[(int)playerIndex];
			GamePadState gamePadState = input.CurrentGamePadStates[(int)playerIndex];

			// The game pauses either if the user presses the pause button, or if
			// they unplug the active gamepad
			bool gamePadDisconnected = !gamePadState.IsConnected &&
									   input.GamePadWasConnected[(int)playerIndex];

			PlayerIndex player;
			if (_pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected) {
				ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
				return false;
			}

			return true;
		}

		public override void Draw(GameTime gameTime)
		{
			if (_gameManager == null)
				addGameManager();

			ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

			_gameManager.Draw(gameTime);

			if (TransitionPosition > 0 || _pauseAlpha > 0)
			{
				float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2);

				ScreenManager.FadeBackBufferToBlack(alpha);
			}
		}

		#endregion
	}
}
