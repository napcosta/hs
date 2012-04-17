using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace HockeySlam.Class.Screens
{
	class AvailableSessionMenuEntry : MenuEntry
	{
		#region Fields

		AvailableNetworkSession _availableSession;
		bool _gotQualityOfService;

		#endregion

		#region Properties

		public AvailableNetworkSession AvailableSession
		{
			get { return _availableSession; }
		}

		#endregion

		#region Initialization

		public AvailableSessionMenuEntry(AvailableNetworkSession availableSession)
			: base(getMenuItemText(availableSession))
		{
			_availableSession = availableSession;
		}

		static string getMenuItemText(AvailableNetworkSession session)
		{
			int totalSlots = session.CurrentGamerCount + session.OpenPublicGamerSlots;

			return string.Format("{0} ({1}/{2})", session.HostGamertag, session.CurrentGamerCount, totalSlots);
		}

		#endregion

		#region Update

		public override void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
		{
			base.Update(screen, isSelected, gameTime);

			if (screen.IsActive && !_gotQualityOfService) {
				QualityOfService qualityOfService = _availableSession.QualityOfService;

				if (qualityOfService.IsAvailable) {
					TimeSpan pingTime = qualityOfService.AverageRoundtripTime;

					Text += string.Format(" - {0:0} ms", pingTime.TotalMilliseconds);

					_gotQualityOfService = true;
				}
			}
		}

		#endregion
	}
}
