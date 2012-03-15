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

		protected override void LoadContent()
		{
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (Effect e in mesh.Effects)
				{
					IEffectLights iel = e as IEffectLights;
					if (iel != null)
					{
						iel.EnableDefaultLighting();
					}
				}
			}

			base.LoadContent();
		}

        public override void Update(GameTime gameTime)
		{
		}

		protected float GetMaxMeshRadius()
		{
			float radius = 0.0f;
			foreach (ModelMesh mm in model.Meshes)
			{
				if (mm.BoundingSphere.Radius > radius)
				{
					radius = mm.BoundingSphere.Radius;
				}
			}
			return radius;
		}

		private Matrix GetParentTransform(ModelBone mb)
		{
			return (mb == model.Root) ? mb.Transform :
				mb.Transform * GetParentTransform(mb.Parent);
		}

		private void DrawModelViaVertexBuffer()
		{
			foreach (ModelMesh mm in model.Meshes)
			{
				foreach (ModelMeshPart mmp in mm.MeshParts)
				{
					IEffectMatrices iem = mmp.Effect as IEffectMatrices;
					if ((mmp.Effect != null) && (iem != null))
					{
						iem.World = world * GetParentTransform(mm.ParentBone);
						iem.Projection = camera.projection;
						iem.View = camera.view;
						GraphicsDevice.SetVertexBuffer(mmp.VertexBuffer, mmp.VertexOffset);
						GraphicsDevice.Indices = mmp.IndexBuffer;
						foreach (EffectPass ep in mmp.Effect.CurrentTechnique.Passes)
						{
							ep.Apply();
							GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
								mmp.NumVertices, mmp.StartIndex, mmp.PrimitiveCount);
						}
					}
				}
			}
		}

		private void drawModel()
		{
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (Effect e in mesh.Effects)
				{
					IEffectMatrices iem = e as IEffectMatrices;
					if (iem != null)
					{
						iem.Projection = camera.projection;
						iem.View = camera.view;
						iem.World = GetWorld() * GetParentTransform(mesh.ParentBone);
					}
				}

				mesh.Draw();
			}
		}

		public override void  Draw(GameTime gameTime)
		{
			DrawModelViaVertexBuffer();
			base.Draw(gameTime);
		}

		public virtual Matrix GetWorld()
		{
			return world;
		}
	}
}
