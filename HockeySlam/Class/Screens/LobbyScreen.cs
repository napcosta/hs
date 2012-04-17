using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using HockeySlam.Class.Networking;


namespace HockeySlam.Class.Screens
{
	class LobbyScreen : GameScreen
	{
		#region Fields

		NetworkSession _networkSession;
		Texture2D _isReadyTexture;

		#endregion

		#region Initialization

		public LobbyScreen(NetworkSession networkSession)
		{
			_networkSession = networkSession;
			TransitionOnTime = TimeSpan.FromSeconds(0.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
		}

		public override void LoadContent()
		{
			ContentManager content = ScreenManager.Game.Content;

			_isReadyTexture = content.Load<Texture2D>("Textures/ready");
		}

		#endregion

		#region Update

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			if (!IsExiting) {
				if (_networkSession.SessionState == NetworkSessionState.Playing)
					Console.WriteLine("The Game Will Start");
				//LoadingScreen.Load(ScreenManager, true, null, new MultiplayerGameplayScreen(_networkSession));
				else if (_networkSession.IsHost && _networkSession.IsEveryoneReady)
					_networkSession.StartGame();
			}
		}

		#endregion

		#region HandleInput

		public override void HandleInput(GameTime gameTime, GameState.InputState input)
		{
			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
				PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;

				PlayerIndex unwantedOutput;

				if (input.IsMenuSelected(playerIndex, out unwantedOutput))
					handleMenuSelected(gamer);
				else if (input.IsMenuCancel(playerIndex, out unwantedOutput))
					handleMenuCancel(gamer);
			}
		}

		private void handleMenuCancel(LocalNetworkGamer gamer)
		{
			if (gamer.IsReady)
				gamer.IsReady = false;
			else {
				PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;
				NetworkSessionComponent.LeaveSession(ScreenManager, playerIndex);
			}
		}

		private void handleMenuSelected(LocalNetworkGamer gamer)
		{
			if (!gamer.IsReady)
				gamer.IsReady = true;
			else if (gamer.IsHost) {
				MessageBoxScreen messageBox = new MessageBoxScreen("Are you sure you want to start the game,\neven though not all players are ready?");

				messageBox.Accepted += ConfirmStartGameMessageBoxAccepted;

				ScreenManager.AddScreen(messageBox, gamer.SignedInGamer.PlayerIndex);
			}
		}

		private void ConfirmStartGameMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
		{
			if (_networkSession.SessionState == NetworkSessionState.Lobby)
				_networkSession.StartGame();
		}

		#endregion

		#region Draw

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			Vector2 position = new Vector2(100, 150);

			float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

			if (ScreenState == ScreenState.TransitionOn)
				position.X -= transitionOffset * 256;
			else
				position.X += transitionOffset * 256;

			spriteBatch.Begin();

			int gamerCount = 0;

			foreach (NetworkGamer gamer in _networkSession.AllGamers) {
				drawGamer(gamer, position);

				if (++gamerCount == 8) {
					position.X += 433;
					position.Y = 150;
				} else
					position.Y += ScreenManager.Font.LineSpacing;
			}

			string title = "Lobby";

			Vector2 titlePosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width/2, 80);
			Vector2 titleOrigin = font.MeasureString(title) / 2;
			Color titleColor = Color.Black * TransitionAlpha;
			float titleScale = 1.25f;

			titlePosition.Y -= transitionOffset * 100;

			spriteBatch.DrawString(font, title, titlePosition, titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void drawGamer(NetworkGamer gamer, Vector2 position)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			Vector2 iconWidth = new Vector2(30, 0);
			Vector2 iconOffset = new Vector2(0, 3);

			Vector2 iconPosition = position + iconOffset;

			if (gamer.IsReady) {
				spriteBatch.Draw(_isReadyTexture, iconPosition, Color.White * TransitionAlpha);
			}

			string text = gamer.Gamertag;

			if (gamer.IsHost)
				text += " (HOST)";

			Color color = (gamer.IsLocal) ? Color.Yellow : Color.White;

			spriteBatch.DrawString(font, text, position + iconWidth * 2, color * TransitionAlpha);
		}

		#endregion
	}
}
