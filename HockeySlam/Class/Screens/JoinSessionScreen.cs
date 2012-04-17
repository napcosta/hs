using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

using HockeySlam.Class.Networking;

namespace HockeySlam.Class.Screens
{
	class JoinSessionScreen : MenuScreen
	{
		#region Fields

		const int _maxSearchResults = 8;
		
		AvailableNetworkSessionCollection _availableSessions;

		#endregion

		#region Initialization

		public JoinSessionScreen(AvailableNetworkSessionCollection availableSessions)
			: base("Join Game")
		{
			_availableSessions = availableSessions;

			foreach (AvailableNetworkSession availableSession in _availableSessions) {
				MenuEntry menuEntry = new AvailableSessionMenuEntry(availableSession);
				menuEntry.Selected += AvaibleSessionMenuEntrySelected;
				MenuEntries.Add(menuEntry);

				if (MenuEntries.Count >= _maxSearchResults)
					break;
			}

			MenuEntry backMenuEntry = new MenuEntry("Back");
			backMenuEntry.Selected += BackMenuEntrySelected;
			MenuEntries.Add(backMenuEntry);
		}

		#endregion

		#region Event Handlers

		void AvaibleSessionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			AvailableSessionMenuEntry menuEntry = (AvailableSessionMenuEntry)sender;
			AvailableNetworkSession availableSession = menuEntry.AvailableSession;

			try {
				IAsyncResult asyncResult = NetworkSession.BeginJoin(availableSession, null, null);

				NetworkBusyScreen busyScreen = new NetworkBusyScreen(asyncResult);
				busyScreen.OperationCompleted += JoinSessionOperationCompleted;

				ScreenManager.AddScreen(busyScreen, ControllingPlayer);
			} catch (Exception exception) { 
				Console.WriteLine(exception.Message);
				ScreenManager.Game.Exit();
			}
		}

		void JoinSessionOperationCompleted(object sender, OperationCompletedEventArgs e)
		{
			try {
				NetworkSession networkSession = NetworkSession.EndJoin(e.AsynchResult);

				NetworkSessionComponent.Create(ScreenManager, networkSession);

				ScreenManager.AddScreen(new LobbyScreen(networkSession), null);
				_availableSessions.Dispose();
			} catch (Exception exception) {
				Console.WriteLine(exception.Message);
				ScreenManager.Game.Exit(); 
			}
		}

		void BackMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			_availableSessions.Dispose();
			ExitScreen();
		}

		#endregion

	}
}
