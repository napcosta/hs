using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HockeySlam.GameState;

namespace HockeySlam.Screens
{
	class MessageBoxScreen : GameScreen
	{
		#region Fields

		string message;
		Texture2D gradientTexture;

		InputAction menuSelect;
		InputAction menuCancel;

		#endregion

		#region Events

		public event EventHandler<PlayerIndexEventArgs> Accepted;
		public event EventHandler<PlayerIndexEventArgs> Cancelled;

		#endregion

		#region Initialization

		public MessageBoxScreen(string message)
			: this(message, true)
		{ }

		public MessageBoxScreen(string message, bool includeUsageText)
		{
			const string usageText = "\nOK -> Button A or Enter" +
									 "\nCANCEL -> Button B or Esc";

			if (includeUsageText)
				this.message = message + usageText;
			else
				this.message = message;

			IsPopup = true;

			TransitionOnTime = TimeSpan.FromSeconds(0.2);
			TransitionOffTime = TimeSpan.FromSeconds(0.2);

			menuSelect = new InputAction(
				new Buttons[] { Buttons.A },
				new Keys[] { Keys.Enter },
				true);
			menuCancel = new InputAction(
				new Buttons[] { Buttons.B },
				new Keys[] { Keys.Escape },
				true);
		}

		public override void Activate(bool instancePreserved)
		{
			if (!instancePreserved)
			{
				ContentManager content = ScreenManager.Game.Content;
				gradientTexture = content.Load<Texture2D>("Screens/gradient");
			}
		}

		#endregion

		#region Handle Input

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			PlayerIndex playerIndex;

			if (menuSelect.Evaluate(input, ControllingPlayer, out playerIndex))
			{
				if (Accepted != null)
					Accepted(this, new PlayerIndexEventArgs(playerIndex));

				ExitScreen();
			}
			else if (menuCancel.Evaluate(input, ControllingPlayer, out playerIndex))
			{
				if (Cancelled != null)
					Cancelled(this, new PlayerIndexEventArgs(playerIndex));

				ExitScreen();
			}
		}

		#endregion

		#region

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
			Vector2 textSize = font.MeasureString(message);
			Vector2 textPosition = (viewportSize - textSize) / 2;

			// The background includes a border somewhat larger than the text itself
			const int hPad = 32;
			const int vPad = 16;

			Rectangle backgroudRectangle = new Rectangle((int)textPosition.X - hPad,
														 (int)textPosition.Y - vPad,
														 (int)textSize.X + hPad * 2,
														 (int)textSize.Y + vPad * 2);
			Color color = Color.White * TransitionAlpha;

			spriteBatch.Begin();

			spriteBatch.Draw(gradientTexture, backgroudRectangle, color);
			spriteBatch.DrawString(font, message, textPosition, color);
			spriteBatch.End();
		}

		#endregion
	}
}
