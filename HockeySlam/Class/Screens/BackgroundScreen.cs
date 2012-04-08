using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using HockeySlam.GameState;

namespace HockeySlam.Screens
{
	class BackgroundScreen : GameScreen
	{
		#region Fields

		ContentManager content;
		Texture2D backgroudTexture;

		#endregion

		#region Initialization

		public BackgroundScreen()
		{
			TransitionOnTime = TimeSpan.FromSeconds(0.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}

		public override void Activate(bool instancePreserved)
		{
			if (!instancePreserved)
			{
				if (content == null)
					content = new ContentManager(ScreenManager.Game.Services, "Content");

				backgroudTexture = content.Load<Texture2D>("Screens/OpenScreen3");
			}
		}

		public override void Unload()
		{
			content.Unload();
		}

		#endregion

		#region Update & Draw

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, false);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

			spriteBatch.Begin();
			spriteBatch.Draw(backgroudTexture, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
			spriteBatch.End();
		}

		#endregion
	}
}
