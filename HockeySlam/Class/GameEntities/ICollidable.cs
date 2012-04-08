using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HockeySlam.GameEntities
{
	interface ICollidable
	{
		List<BoundingSphere> getBoundingSpheres();
		Boolean collisionOccured(List<BoundingSphere> area);
		void notify();
	}
}
