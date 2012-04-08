using Microsoft.Xna.Framework;

namespace HockeySlam.Screens
{
	class MainMenuScreen : MenuScreen
	{
		#region Initialization

		public MainMenuScreen()
			: base("Main Menu")
		{
			MenuEntry playGameMenuEntry = new MenuEntry("Play Game");
			MenuEntry exitMenuEntry = new MenuEntry("Exit");

			playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
			exitMenuEntry.Selected += OnCancel;

			MenuEntries.Add(playGameMenuEntry);
			MenuEntries.Add(exitMenuEntry);
		}

		#endregion

		#region Handle Input

		void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen());
		}

		protected override void OnCancel(object sender, PlayerIndexEventArgs e)
		{
			OnCancel(e.PlayerIndex);
		}

		protected override void OnCancel(PlayerIndex playerIndex)
		{
			const string message = "Are you sure you want to exit Hockey Slam?";

			MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);
			confirmExitMessageBox.Accepted += ConfirmedExitMessageBoxAccepted;

			ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
		}

		void ConfirmedExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.Game.Exit();
		}

		#endregion
	}
}
