using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using HockeySlam.Class.Networking;

namespace HockeySlam.Class.Screens
{
	class NetworkBusyScreen : GameScreen
	{
		#region Fields

		IAsyncResult _asyncResult;
		
		Texture2D _playerSprite;
		Texture2D _gradientTexture;
		float _timer;
		float _spriteIntervale;
		int _spriteWidth;
		int _spriteHeigth;
		int _currentFrame;
		Rectangle _sourceRect;
		Vector2 _origin;

		#endregion

		#region Events

		public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

		#endregion

		#region Initialization

		public NetworkBusyScreen(IAsyncResult asyncResult)
		{
			_asyncResult = asyncResult;

			IsPopup = true;

			TransitionOnTime = TimeSpan.FromSeconds(0.1);
			TransitionOffTime = TimeSpan.FromSeconds(0.2);

			_timer = 0;
			_spriteIntervale = 100f;
			_spriteWidth = 341;
			_spriteHeigth = 576;
			_currentFrame = 0;
		}

		public override void LoadContent()
		{
			ContentManager content = new ContentManager(ScreenManager.Game.Services, "Content");
			_playerSprite = content.Load<Texture2D>("Sprites/PlayerSprite");
			_gradientTexture = content.Load<Texture2D>("Screens/gradient");
		}

		#endregion

		#region Update & Draw

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			if ((_asyncResult != null) && _asyncResult.IsCompleted) {
				if (OperationCompleted != null) {
					OperationCompleted(this, new OperationCompletedEventArgs(_asyncResult));
				}

				ExitScreen();

				_asyncResult = null;
			}

			_timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

			if(_timer > _spriteIntervale) {
				_currentFrame = (_currentFrame+1)%12;
				_timer = 0;
			}

			_sourceRect = new Rectangle(_currentFrame*_spriteWidth, 0, _spriteWidth, _spriteHeigth);
			_origin = new Vector2(_sourceRect.Width/2, _sourceRect.Height/2);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			string message = "Busy Network...";

			const int hPad = 32;
			const int vPad = 16;

			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
			Vector2 textSize = font.MeasureString(message);

			Vector2 playerSize = new Vector2(_spriteWidth);

			textSize.X = Math.Max(textSize.X, playerSize.X);
			textSize.Y += playerSize.Y + vPad;

			Vector2 textPosition = (viewportSize - textSize) / 2;

			Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
														  (int)textPosition.Y - vPad,
														  (int)textSize.X + hPad * 2,
														  (int)textSize.Y + vPad * 2);

			Color color = Color.White * TransitionAlpha;

			spriteBatch.Begin();

			spriteBatch.Draw(_gradientTexture, backgroundRectangle, color);
			spriteBatch.DrawString(font, message, textPosition, color);

			Vector2 playerPosition = new Vector2(textPosition.X + textSize.X / 2, textPosition.Y + textSize.Y - playerSize.Y / 2);

			spriteBatch.Draw(_playerSprite, playerPosition, _sourceRect, color, 0f, playerSize / 2, 0.5f, SpriteEffects.None, 0);

			spriteBatch.End();

			base.Draw(gameTime);
		}
		#endregion
	}
}
