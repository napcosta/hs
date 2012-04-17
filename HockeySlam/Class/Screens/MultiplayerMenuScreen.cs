using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;

namespace HockeySlam.Class.Screens
{
	class MultiplayerMenuScreen : MenuScreen
	{
		#region Initialization

		public MultiplayerMenuScreen()
			: base("Multiplayer Menu")
		{
			MenuEntry localGameMenuEntry = new MenuEntry("Local Game");
			MenuEntry liveGameMenuEntry = new MenuEntry("LIVE Game");
			MenuEntry exitMenuEntry = new MenuEntry("Exit");

			localGameMenuEntry.Selected += localGameMenuEntrySelected;
			liveGameMenuEntry.Selected += liveGameMenuEntrySelected;
			exitMenuEntry.Selected += OnCancel;

			MenuEntries.Add(localGameMenuEntry);
			MenuEntries.Add(liveGameMenuEntry);
			MenuEntries.Add(exitMenuEntry);
		}
		#endregion

		#region Handle Input

		private void localGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new CreateOrFindSessionScreen(NetworkSessionType.SystemLink), e.PlayerIndex);
		}

		private void liveGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			ScreenManager.AddScreen(new CreateOrFindSessionScreen(NetworkSessionType.PlayerMatch), e.PlayerIndex);
		}

		#endregion
	}
}
