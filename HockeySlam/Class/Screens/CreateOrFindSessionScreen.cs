using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

using HockeySlam.Class.Networking;

namespace HockeySlam.Class.Screens
{
	class CreateOrFindSessionScreen : MenuScreen
	{
		#region Fields

		NetworkSessionType _sessionType;

		#endregion

		#region Initialization

		public CreateOrFindSessionScreen(NetworkSessionType sessionType)
			: base((sessionType == NetworkSessionType.SystemLink) ? "Local Game Menu" : "LIVE Game Menu")
		{
			MenuEntry createGameMenuEntry = new MenuEntry("Create Game");
			MenuEntry joinGameMenuEntry = new MenuEntry("Join Game");
			MenuEntry exitMenuEntry = new MenuEntry("Exit");

			_sessionType = sessionType;

			createGameMenuEntry.Selected += CreateGameMenuEntrySelected;
			joinGameMenuEntry.Selected += FindSessionMenuEntrySelected;
			exitMenuEntry.Selected += OnCancel;

			MenuEntries.Add(createGameMenuEntry);
			MenuEntries.Add(joinGameMenuEntry);
			MenuEntries.Add(exitMenuEntry);
		}

		#endregion

		#region Handle Input

		private void CreateGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			try {
				IAsyncResult asyncResult = NetworkSession.BeginCreate(_sessionType, 4, 6, null, null);

				NetworkBusyScreen busyScreen = new NetworkBusyScreen(asyncResult);

				busyScreen.OperationCompleted += CreateSessionOperationCompleted;

				ScreenManager.AddScreen(busyScreen, ControllingPlayer);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message);
				ScreenManager.Game.Exit();
			}
		}

		private void CreateSessionOperationCompleted(object sender, OperationCompletedEventArgs e)
		{
			try {
				NetworkSession networkSession = NetworkSession.EndCreate(e.AsynchResult);

				NetworkSessionComponent.Create(ScreenManager, networkSession);

				ScreenManager.AddScreen(new LobbyScreen(networkSession), null);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message);
				ScreenManager.Game.Exit();
			}
		}

		private void FindSessionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
		{
			try {
				IEnumerable<SignedInGamer> localGamers = NetworkSessionComponent.ChooseGamers(_sessionType, ControllingPlayer.Value);

				IAsyncResult asyncResult = NetworkSession.BeginFind(_sessionType, localGamers, null, null, null);

				NetworkBusyScreen busyScreen = new NetworkBusyScreen(asyncResult);

				busyScreen.OperationCompleted += FindSessionOperationCompleted;
				ScreenManager.AddScreen(busyScreen, ControllingPlayer);
			} catch (Exception exception) {
				Console.WriteLine(exception.Message);
				ScreenManager.Game.Exit();
			}
		}

		void FindSessionOperationCompleted(object sender, OperationCompletedEventArgs e)
		{
			GameScreen nextScreen;

			try {
				AvailableNetworkSessionCollection availableSessions = NetworkSession.EndFind(e.AsynchResult);

				if (availableSessions.Count == 0) {
					availableSessions.Dispose();
					nextScreen = new MessageBoxScreen("No sessions found", false);
				} else {
					nextScreen = new JoinSessionScreen(availableSessions);
				}
			} catch (Exception exception) { 
				nextScreen = null;
				Console.WriteLine(exception.Message);
				ScreenManager.Game.Exit();
			}

			ScreenManager.AddScreen(nextScreen, ControllingPlayer);
		}				

		#endregion

	}
}
