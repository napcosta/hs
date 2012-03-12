using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HockeySlam
{
	class BaseModel : DrawableGameComponent
	{

		Camera camera;

		public Model model
		{
			get;
			protected set;
		}
		protected Matrix world = Matrix.Identity;

		public BaseModel(Game game): base (game)
		{
			camera = ((Game1)Game).camera;
		}

        public override void Update(GameTime gameTime)
		{
			camera = ((Game1)Game).camera;
		}

		public override void  Draw(GameTime gameTime)
		{
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in model.Meshes) {
				foreach (BasicEffect be in mesh.Effects) {
					be.EnableDefaultLighting();
					be.Projection = camera.projection;
					be.View = camera.view;
					be.World = GetWorld() * mesh.ParentBone.Transform;
				}

				mesh.Draw();
			}

			base.Draw(gameTime);
		}

		public virtual Matrix GetWorld()
		{
			return world;
		}
	}
}
