using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using HockeySlam.Class.GameEntities;

namespace HockeySlam.Interface
{
	interface IReflectable
	{
		void Draw(GameTime gameTime, Camera camera);
		void setClipPlane(Vector4? plane);
	}
}
