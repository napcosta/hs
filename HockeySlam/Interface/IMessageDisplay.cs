using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HockeySlam.Interface
{
	interface IMessageDisplay : IDrawable, IUpdateable
	{
		void showMessage(string message, params object[] parameters);
	}
}
