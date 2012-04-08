using System;
using Microsoft.Xna.Framework;

namespace HockeySlam.Class.Screens
{
	class PlayerIndexEventArgs : EventArgs
	{

		public PlayerIndexEventArgs(PlayerIndex playerIndex)
		{
			this.playerIndex = playerIndex;
		}

		public PlayerIndex PlayerIndex
		{
			get { return playerIndex; }
		}

		PlayerIndex playerIndex;
	}
}
