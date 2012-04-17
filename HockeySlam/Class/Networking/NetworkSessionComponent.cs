using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

using HockeySlam.Class.GameState;
using HockeySlam.Class.Screens;
using HockeySlam.Interface;

namespace HockeySlam.Class.Networking
{
	class NetworkSessionComponent : GameComponent
	{
		#region Fields

		public const int _maxGamers = 6;
		public const int _maxLocalGamers = 4;

		ScreenManager _screenManager;
		NetworkSession _networkSession;
		IMessageDisplay _messageDisplay;

		bool _notifyWhenPlayersJoinOrLeave;
		string _sessionEndMessage;

		#endregion

		#region Initialization

		public NetworkSessionComponent(ScreenManager screenManager, NetworkSession networkSession)
			: base(screenManager.Game)
		{
			_screenManager = screenManager;
			_networkSession = networkSession;

			_networkSession.GamerJoined += GamerJoined;
			_networkSession.GamerLeft += GamerLeft;
			_networkSession.SessionEnded += NetworkSessionEnded;
		}

		public static void Create(ScreenManager screenManager, NetworkSession networkSession)
		{
			Game game = screenManager.Game;

			game.Services.AddService(typeof(NetworkSession), networkSession);
			game.Components.Add(new NetworkSessionComponent(screenManager, networkSession));
		}

		public override void Initialize()
		{
			base.Initialize();

			_messageDisplay = (IMessageDisplay)Game.Services.GetService(typeof(IMessageDisplay));

			if (_messageDisplay != null)
				_notifyWhenPlayersJoinOrLeave = true;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing) {
				Game.Components.Remove(this);
				Game.Services.RemoveService(typeof(NetworkSession));

				if(_networkSession != null) {
					_networkSession.Dispose();
					_networkSession = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Update

		public override void Update(GameTime gameTime)
		{
			if (_networkSession == null)
				return;

			try {
				_networkSession.Update();

				if (_networkSession.SessionState == NetworkSessionState.Ended)
					leaveSession();
			} catch (Exception exception) {
				Console.WriteLine("NetworkSession.Update threw " + exception);
				_sessionEndMessage = "Network Error";

				leaveSession();
			}
		}

		#endregion

		#region Event Handlers

		private void GamerJoined(object sender, GamerJoinedEventArgs e)
		{
			if (_notifyWhenPlayersJoinOrLeave) {
				_messageDisplay.showMessage("Gamer Joined: " + e.Gamer.Gamertag);
			}
		}

		private void GamerLeft(object sender, GamerLeftEventArgs e)
		{
			if (_notifyWhenPlayersJoinOrLeave) {
				_messageDisplay.showMessage("Gamer Left: " + e.Gamer.Gamertag);
			}
		}

		private void NetworkSessionEnded(object sender, NetworkSessionEndedEventArgs e)
		{
			switch (e.EndReason) {
				case NetworkSessionEndReason.ClientSignedOut:
					_sessionEndMessage = null;
					break;

				case NetworkSessionEndReason.HostEndedSession:
					_sessionEndMessage = "Host has ended the session";
					break;

				case NetworkSessionEndReason.RemovedByHost:
					_sessionEndMessage = "You where removed by the Host";
					break;

				case NetworkSessionEndReason.Disconnected:
				default:
					_sessionEndMessage = "You where disconnected";
					break;
			}

			_notifyWhenPlayersJoinOrLeave = false;
		}

		#endregion

		#region Methods

		public static bool IsOnlineSessionType(NetworkSessionType sessionType)
		{
			switch (sessionType) {
				case NetworkSessionType.Local:
				case NetworkSessionType.SystemLink:
					return false;

				case NetworkSessionType.PlayerMatch:
				case NetworkSessionType.Ranked:
					return true;

				default:
					throw new NotSupportedException();
			}
		}

		public static IEnumerable<SignedInGamer> ChooseGamers(NetworkSessionType sessionType, PlayerIndex playerIndex)
		{
			List<SignedInGamer> gamers = new List<SignedInGamer>();

			SignedInGamer primaryGamer = Gamer.SignedInGamers[playerIndex];

			if (primaryGamer == null)
				throw new GamerPrivilegeException();

			gamers.Add(primaryGamer);

			foreach (SignedInGamer gamer in Gamer.SignedInGamers) {
				if (gamers.Count >= _maxLocalGamers)
					break;

				if (gamer == primaryGamer)
					continue;

				if (IsOnlineSessionType(sessionType)) {
					if (!gamer.IsSignedInToLive)
						continue;
					if (!gamer.Privileges.AllowOnlineSessions)
						continue;
				}

				if (primaryGamer.IsGuest && !gamer.IsGuest && gamers[0] == primaryGamer)
					gamers.Insert(0, gamer);
				else
					gamers.Add(gamer);
			}

			return gamers;
		}

		public static void LeaveSession(ScreenManager screenManager, PlayerIndex playerIndex)
		{
			NetworkSessionComponent self = FindSessionComponent(screenManager.Game);

			if (self != null) {
				string message;

				if (self._networkSession.IsHost)
					message = "Are you sure you what to end this session?";
				else message = "Are you sure you what to leave this session?";

				MessageBoxScreen confirmMessageBox = new MessageBoxScreen(message);

				confirmMessageBox.Accepted += delegate {
					self.leaveSession();
				};

				screenManager.AddScreen(confirmMessageBox, playerIndex);
			}
		}

		void leaveSession()
		{
			Dispose();

			MessageBoxScreen messageBox;

			if (!string.IsNullOrEmpty(_sessionEndMessage))
				messageBox = new MessageBoxScreen(_sessionEndMessage, false);
			else messageBox = null;

			GameScreen[] screens = _screenManager.GetScreens();

			for (int i = 0; i < screens.Length; i++) {
				if (screens[i] is MainMenuScreen) {
					for (int j = i + 1; j < screens.Length; j++)
						screens[j].ExitScreen();

					if (messageBox != null)
						_screenManager.AddScreen(messageBox, null);

					return;
				}
			}

			LoadingScreen.Load(_screenManager, false, null, new BackgroundScreen(), new MainMenuScreen(), messageBox);
		}

		static NetworkSessionComponent FindSessionComponent(Game game)
		{
			return game.Components.OfType<NetworkSessionComponent>().FirstOrDefault();
		}

		#endregion

	}
}
