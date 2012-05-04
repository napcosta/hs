using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameState
{
	class CollisionManager : IGameEntity
	{

		List<ICollidable> collidableObjects;

		public void Initialize() 
		{
			collidableObjects = new List<ICollidable>();
		}

		public void register(ICollidable collidable)
		{
			collidableObjects.Add(collidable);
		}

		public List<ICollidable> verifyCollision(ICollidable collidable) 
		{
			List<ICollidable> collidedWith = new List<ICollidable>();
			foreach (ICollidable collidableObject in collidableObjects)
			{
				if (collidableObject != collidable && collidableObject.collisionOccured(collidable)) {
					collidedWith.Add(collidableObject);
					Console.WriteLine("Collision");
				}
			}

			return collidedWith;
		}

		public void Update(GameTime gameTime) { }
		public void Draw(GameTime gameTime) { }
		public void LoadContent() { }
		
	}
}
