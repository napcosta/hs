using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using HockeySlam.Class.Networking;
using HockeySlam.Class.GameEntities.Models;


namespace HockeySlam.Class.Screens
{
	class LobbyScreen : GameScreen
	{
		#region Fields

		NetworkSession _networkSession;
		Texture2D _isReadyTexture;
		Texture2D _gradient;
		SpriteFont _smallerFont;

		PacketReader _packetReader;
		PacketWriter _packetWriter;

		KeyboardState _currentKeyBoard;
		KeyboardState _lastKeyBoard;

		#endregion

		#region Initialization

		public LobbyScreen(NetworkSession networkSession)
		{
			_networkSession = networkSession;
			TransitionOnTime = TimeSpan.FromSeconds(0.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
			_packetReader = new PacketReader();
			_packetWriter = new PacketWriter();

		}

		public override void LoadContent()
		{
			ContentManager content = ScreenManager.Game.Content;

			_isReadyTexture = content.Load<Texture2D>("Textures/ready");
			_gradient = content.Load<Texture2D>("Screens/gradient");

			_smallerFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/SmallerGameFont");
		}

		#endregion

		#region Update

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			foreach (Gamer gamer in _networkSession.AllGamers) {
				if (gamer.Tag == null)
					gamer.Tag = 1;
			}

			if (!IsExiting) {
				if (_networkSession.SessionState == NetworkSessionState.Playing)
					LoadingScreen.Load(ScreenManager, true, null, new GameplayScreen(_networkSession));
				else if (_networkSession.IsHost && _networkSession.IsEveryoneReady)
					_networkSession.StartGame();
			}
		}

		#endregion

		#region HandleInput

		public override void HandleInput(GameTime gameTime, GameState.InputState input)
		{
			foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers) {
				ReadIncomingPackets(gamer);
				PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;

				_currentKeyBoard = Keyboard.GetState(playerIndex);

				PlayerIndex unwantedOutput;

				if (input.IsMenuSelected(playerIndex, out unwantedOutput))
					handleMenuSelected(gamer);
				else if (input.IsMenuCancel(playerIndex, out unwantedOutput))
					handleMenuCancel(gamer);
				else if(isSelectingTeam1())
					handleChangeToTeam(gamer, 1);
				else if(isSelectingTeam2())
					handleChangeToTeam(gamer, 2);

				_lastKeyBoard = _currentKeyBoard;
			}
		}

		private void ReadIncomingPackets(LocalNetworkGamer gamer)
		{
			while (gamer.IsDataAvailable) {
				NetworkGamer sender;

				gamer.ReceiveData(_packetReader, out sender);

				if (sender.IsLocal)
					continue;

				int newTeam = _packetReader.ReadByte();
				sender.Tag = newTeam;
			}
		}

		private void handleChangeToTeam(LocalNetworkGamer gamer, int team)
		{
			if ((int)gamer.Tag == team)
				return;

			gamer.Tag = team;
			_packetWriter.Write(team);

			gamer.SendData(_packetWriter, SendDataOptions.InOrder);
		}

		private bool isSelectingTeam1()
		{
			return _currentKeyBoard.IsKeyDown(Keys.Left) && _lastKeyBoard.IsKeyUp(Keys.Left);
		}

		private bool isSelectingTeam2()
		{
			return _currentKeyBoard.IsKeyDown(Keys.Right) && _lastKeyBoard.IsKeyUp(Keys.Right);
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

			Vector2 positionTeam1 = new Vector2(100, 150);
			Vector2 positionTeam2 = new Vector2(400, 150);
			

			float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

			if (ScreenState == ScreenState.TransitionOn)
				positionTeam1.X -= transitionOffset * 256;
			else
				positionTeam1.X += transitionOffset * 256;

			spriteBatch.Begin();

			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Rectangle rec = new Rectangle((int)(0.1*viewport.Width), (int)(0.1*viewport.Height), 
										  (int)(viewport.Width - 0.2 * viewport.Width), (int)(viewport.Height - 0.2 * viewport.Height));

			spriteBatch.Draw(_gradient, rec, Color.White);

			foreach (NetworkGamer gamer in _networkSession.AllGamers) {
				if (gamer.Tag == null)
					gamer.Tag = 1;

				if ((int)gamer.Tag == 1) {
					drawGamer(gamer, positionTeam1);
					positionTeam1.Y += ScreenManager.Font.LineSpacing;
				} else {
					drawGamer(gamer, positionTeam2);
					positionTeam2.Y += ScreenManager.Font.LineSpacing;
				}
			}

			string title = "Lobby";

			Vector2 titlePosition = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width/2, 80);
			Vector2 titleOrigin = font.MeasureString(title) / 2;
			Color titleColor = Color.Black * TransitionAlpha;
			float titleScale = 1.25f;

			titlePosition.Y -= transitionOffset * 100;

			spriteBatch.DrawString(font, title, titlePosition, titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0);

			title = "Team 1";

			titlePosition.X = 230;
			titlePosition.Y = 130;

			spriteBatch.DrawString(font, title, titlePosition, Color.CadetBlue, 0, titleOrigin, 1, SpriteEffects.None, 0);

			title = "Team 2";

			titlePosition.X = 530;
			titlePosition.Y = 130;

			spriteBatch.DrawString(font, title, titlePosition, Color.IndianRed, 0, titleOrigin, 1, SpriteEffects.None, 0);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void drawGamer(NetworkGamer gamer, Vector2 position)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			Vector2 iconWidth = new Vector2(30, 0);
			Vector2 iconOffset = new Vector2(0, 3);

			Vector2 iconPosition = position + iconOffset + new Vector2(10,0);

			Rectangle rec = new Rectangle((int)iconPosition.X, (int)iconPosition.Y, 20, 20);

			if (gamer.IsReady) {
				spriteBatch.Draw(_isReadyTexture, rec, Color.White * TransitionAlpha);
			}

			string text = gamer.Gamertag;

			if (gamer.IsHost)
				text += " (HOST)";

			Color color = (gamer.IsLocal) ? Color.Yellow : Color.White;

			spriteBatch.DrawString(_smallerFont, text, position + iconWidth * 2, color * TransitionAlpha);
		}

		#endregion
	}
}
