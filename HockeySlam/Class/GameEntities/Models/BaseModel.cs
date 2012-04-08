using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HockeySlam.Class.GameEntities;
using HockeySlam;
using HockeySlam.Interface;
namespace HockeySlam.Class.GameEntities.Models
{
	class BaseModel : IGameEntity
	{
		Camera camera;
		Game game;
		Vector3 ambientLightColor;
		Vector3 lightDirection;
		Vector3 diffuseLightColor;
		Matrix[] modelTransforms;
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

		public virtual void Initialize()
		{
			modelTransforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(modelTransforms);
			ambientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
			

			
			lightDirection = new Vector3(-1.0f, -1.0f, 0);
			lightDirection.Normalize();
			diffuseLightColor = new Vector3(0.7f, 0.7f, 0.7f);
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

		protected void DrawEffect(Effect effect, Vector3 diffuseColor)
		{
			effect.Parameters["View"].SetValue(camera.view);
			effect.Parameters["Projection"].SetValue(camera.projection);
			effect.Parameters["AmbientLightColor"].SetValue(ambientLightColor);
			effect.Parameters["LightDirection"].SetValue(-lightDirection);
			effect.Parameters["DiffuseLightColor"].SetValue(diffuseLightColor);
			foreach (ModelMesh mesh in model.Meshes) {
				effect.Parameters["World"].SetValue(modelTransforms[mesh.ParentBone.Index] * world);
				foreach (ModelMeshPart meshPart in mesh.MeshParts) {
					game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
					game.GraphicsDevice.Indices = meshPart.IndexBuffer;
					effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
					effect.CurrentTechnique.Passes[0].Apply();
					game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
				}
			}
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
						iem.World = GetParentTransform(mm.ParentBone) * world;
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

	}
}
