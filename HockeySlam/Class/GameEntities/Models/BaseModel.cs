using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HockeySlam.GameEntities;
using HockeySlam;

namespace HockeySlam.GameEntities.Models
{
	class BaseModel : IGameEntity
	{
		Camera camera;
		Game game;

		public Model model
		{
			get;
			protected set;
		}
		protected Matrix world = Matrix.Identity;

		public BaseModel(Game game, Camera camera)
		{
			this.camera = camera;
			this.game = game;
		}

		public virtual void LoadContent()
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

		}

        public virtual void Update(GameTime gameTime)
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
						game.GraphicsDevice.SetVertexBuffer(mmp.VertexBuffer, mmp.VertexOffset);
						game.GraphicsDevice.Indices = mmp.IndexBuffer;
						foreach (EffectPass ep in mmp.Effect.CurrentTechnique.Passes)
						{
							ep.Apply();
							game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
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

		public virtual void Draw(GameTime gameTime)
		{
			DrawModelViaVertexBuffer();
		}

		public virtual Matrix GetWorld()
		{
			return world;
		}

		public virtual void Initialize() { }
	}
}
