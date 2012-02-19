using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HockeySlam
{
	class BaseModel
	{
		public Model model
		{
			get;
			protected set;
		}
		protected Matrix world = Matrix.Identity;

		public BaseModel(Model m)
		{
			model = m;
		}

		public virtual void Update()
		{

		}

		public void Draw(Camera camera)
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
		}

		public virtual Matrix GetWorld()
		{
			return world;
		}
	}
}
