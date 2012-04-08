using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HockeySlam.GameState;

namespace HockeySlam.Screens
{
	// Base class for screens that contain menu options.
	class MenuScreen : GameScreen
	{
		#region Fields

		List<MenuEntry> menuEntries = new List<MenuEntry>();
		int selectedEntry = 0;
		string menuTitle;

		InputAction menuUp;
		InputAction menuDown;
		InputAction menuSelect;
		InputAction menuCancel;

		#endregion

		#region Properties

		protected IList<MenuEntry> MenuEntries
		{
			get { return menuEntries; }
		}

		#endregion

		#region Initialization

		public MenuScreen(string menuTitle)
		{
			this.menuTitle = menuTitle;

			TransitionOnTime = TimeSpan.FromSeconds(0.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			menuUp = new InputAction(
				new Buttons[] { Buttons.DPadUp, Buttons.LeftThumbstickUp },
				new Keys[] { Keys.Up },
				true);
			menuDown = new InputAction(
				new Buttons[] { Buttons.DPadDown, Buttons.LeftThumbstickDown },
				new Keys[] { Keys.Down },
				true);
			menuSelect = new InputAction(
				new Buttons[] { Buttons.A, Buttons.Start },
				new Keys[] { Keys.Enter, Keys.Space },
				true);
			menuCancel = new InputAction(
				new Buttons[] { Buttons.B, Buttons.Back },
				new Keys[] { Keys.Escape },
				true);
		}

		#endregion

		#region Handle Input

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			PlayerIndex playerIndex;

			if (menuUp.Evaluate(input, ControllingPlayer, out playerIndex))
			{
				selectedEntry--;
				if (selectedEntry < 0)
					selectedEntry = menuEntries.Count - 1;
			}

			if (menuDown.Evaluate(input, ControllingPlayer, out playerIndex))
			{
				selectedEntry++;
				if (selectedEntry >= menuEntries.Count)
					selectedEntry = 0;
			}

			if (menuSelect.Evaluate(input, ControllingPlayer, out playerIndex))
			{
				OnSelectEntry(selectedEntry, playerIndex);
			}

			else if (menuCancel.Evaluate(input, ControllingPlayer, out playerIndex))
			{
				OnCancel(playerIndex);
			}
		}

		protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
		{
			menuEntries[entryIndex].OnSelectEntry(playerIndex);
		}

		protected virtual void OnCancel(PlayerIndex playerIdex)
		{
			ExitScreen();
		}

		protected virtual void OnCancel(object sender, PlayerIndexEventArgs e)
		{
			OnCancel(e.PlayerIndex);
		}

		#endregion

		#region Update & Draw

		protected virtual void UpdateMenuEntryLocations()
		{
			// Make the menu entries slide into place during transitions, using a
			// power curve to make the movements slow down as it nears the end
			float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

			Vector2 position = new Vector2(0f, 380f);

			for(int i = 0; i < menuEntries.Count; i++)
			{
				MenuEntry menuEntry = menuEntries[i];

				position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;

				if (ScreenState == ScreenState.TransitionOn)
					position.X -= transitionOffset * 256;
				else
					position.X += transitionOffset * 512;

				menuEntry.Position = position;

				position.Y += menuEntry.GetHeight(this);
			}
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			for (int i = 0; i < menuEntries.Count; i++)
			{
				bool isSelectend = IsActive && (i == selectedEntry);

				menuEntries[i].Update(this, isSelectend, gameTime);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			UpdateMenuEntryLocations();

			GraphicsDevice graphics = ScreenManager.GraphicsDevice;
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			spriteBatch.Begin();

			for (int i = 0; i < menuEntries.Count; i++)
			{
				MenuEntry menuEntry = menuEntries[i];

				bool isSelected = IsActive && (i == selectedEntry);

				menuEntry.Draw(this, isSelected, gameTime);
			}

			float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

			Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2, 270);
			Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
			Color titleColor = Color.Black * TransitionAlpha;
			float titleScale = 1.25f;

			titlePosition.Y -= transitionOffset * 100;

			spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
									titleOrigin, titleScale, SpriteEffects.None, 0);
			spriteBatch.End();
		}

		#endregion
	}
}
