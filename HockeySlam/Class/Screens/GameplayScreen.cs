using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HockeySlam.GameState;

namespace HockeySlam.Screens
{
	class GameplayScreen : GameScreen
	{
		#region Fields

		ContentManager content;
		GameManager gameManager;
		SpriteFont gameFont;

		float pauseAlpha;

		InputAction pauseAction;

		#endregion

		#region Initialization

		public GameplayScreen()
		{
			TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			pauseAction = new InputAction(
				new Buttons[] { Buttons.Start, Buttons.Back },
				new Keys[] { Keys.Escape },
				true);
		}

		public override void Activate(bool instancePreserved)
		{
			if (!instancePreserved)
			{
				if (content == null)
					content = new ContentManager(ScreenManager.Game.Services, "Content");

				gameFont = content.Load<SpriteFont>("Fonts/GameFont");

				Thread.Sleep(1000);

				ScreenManager.Game.ResetElapsedTime();
			}
		}

		#endregion

		#region Update & Draw

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, false);
			if (gameManager == null)
				gameManager = new GameManager(ScreenManager.Game);

			if (coveredByOtherScreen)
				pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
			else
				pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

			if (IsActive)
			{
				gameManager.Update(gameTime);
			}
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			int playerIndex = (int)ControllingPlayer.Value;

			KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
			GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

			// The game pauses either if the user presses the pause button, or if
			// they unplug the active gamepad
			bool gamePadDisconnected = !gamePadState.IsConnected &&
									   input.GamePadWasConnected[playerIndex];

			PlayerIndex player;
			if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
				ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
		}

		public override void Draw(GameTime gameTime)
		{
			if (gameManager == null)
				gameManager = new GameManager(ScreenManager.Game);

			ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

			gameManager.Draw(gameTime);

			if (TransitionPosition > 0 || pauseAlpha > 0)
			{
				float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

				ScreenManager.FadeBackBufferToBlack(alpha);
			}
		}

		#endregion
	}
}
