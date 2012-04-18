using Microsoft.Xna.Framework;

namespace HockeySlam.Class.Screens
{
	class MainMenuScreen : MenuScreen
	{
		#region Initialization

		public MainMenuScreen()
			: base("Main Menu")
		{
			MenuEntry singlePlayerMenuEntry = new MenuEntry("Single Player");
			MenuEntry multiplayerMenuEntry = new MenuEntry("Multiplayer");
			MenuEntry exitMenuEntry = new MenuEntry("Exit");

			singlePlayerMenuEntry.Selected += SinglePlayerMenuEntrySelected;
			multiplayerMenuEntry.Selected += MultiplayerMenuEntrySelected;
			exitMenuEntry.Selected += OnCancel;

			MenuEntries.Add(singlePlayerMenuEntry);
			MenuEntries.Add(multiplayerMenuEntry);
			MenuEntries.Add(exitMenuEntry);
		}

		#endregion

		#region Handle Input

		void MultiplayerMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new MultiplayerMenuScreen(), e.PlayerIndex);
		}

		void SinglePlayerMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen(null));
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
