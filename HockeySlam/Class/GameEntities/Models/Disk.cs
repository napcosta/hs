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
using HockeySlam.GameEntities;
using HockeySlam.GameState;

namespace HockeySlam.GameEntities.Models
{
	class Disk : BaseModel, IGameEntity, ICollidable, IDebugEntity
	{
		BoundingSphere collisionArea;
		Game game;
		Camera camera;

		GameManager gameManager;

		public Disk(GameManager gameManager, Game game, Camera camera)
			: base(game, camera)
		{
			model = game.Content.Load<Model>(@"Models\disk");
			this.game = game;
			this.camera = camera;
			this.gameManager = gameManager;
		}

		public override void Initialize()
		{

			Matrix pos = Matrix.CreateTranslation(4, 1, 0);
			Matrix rotation = Matrix.CreateRotationX(MathHelper.PiOver2);
			Matrix scale = Matrix.CreateScale(0.5f);

			world = world * rotation * scale * pos;

			collisionArea = new BoundingSphere(new Vector3(4,1,0), 0.45f);

			CollisionManager cm = (CollisionManager)gameManager.getGameEntity("collisionManager");
			DebugManager dm = (DebugManager)gameManager.getGameEntity("debugManager");
			cm.registre(this);
			dm.registreDebugEntities(this);

			base.Initialize();
		}

		public List<BoundingSphere> getBoundingSpheres()
		{
			List<BoundingSphere> bs = new List<BoundingSphere>();

			bs.Add(collisionArea);

			return bs;
		}

		public bool collisionOccured(List<BoundingSphere> bss)
		{
			foreach (BoundingSphere bs in bss) {
				if(bs.Intersects(collisionArea))
					return true;
			}
			return false;
		}

		public void notify()
		{
			//CollisionManager cm = (CollisionManager)gameManager.getGameEntity("collisionManager");
			//if (cm.verifyCollision(this))
			//    Console.WriteLine("CollisionOcurred");
		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(collisionArea, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
		}
	}
}
