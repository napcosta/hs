using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HockeySlam.Interface
{
	public interface ICollidable
	{
		List<BoundingSphere> getBoundingSpheres();
		List<BoundingBox> getBoundingBoxes();
		Boolean collisionOccured(ICollidable collideObject);
		void notify();
		void bounce(Vector2 newVelocity);
		Vector2 getVelocity();
	}
}
