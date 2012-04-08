using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HockeySlam.Class.Screens
{
	class PauseMenuScreen : MenuScreen
	{
		public PauseMenuScreen()
			: base("Paused")
		{
			// Create our menu entries.
			MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
			MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");

			// Hook up menu event handlers.
			resumeGameMenuEntry.Selected += OnCancel;
			quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

			// Add entries to the menu.
			MenuEntries.Add(resumeGameMenuEntry);
			MenuEntries.Add(quitGameMenuEntry);
		}

		void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			const string message = "Are you sure you want to quit Hockey Slam?";

			MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

			confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

			ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
		}

		protected override void OnCancel(PlayerIndex playerIdex)
		{
			base.OnCancel(playerIdex);
		}

		void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
														   new MainMenuScreen());
		}
	}
}
