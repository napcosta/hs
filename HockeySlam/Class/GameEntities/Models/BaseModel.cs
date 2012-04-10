using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HockeySlam.Class.GameEntities;
using HockeySlam;
using HockeySlam.Interface;
namespace HockeySlam.Class.GameEntities.Models
{
	class BaseModel : IGameEntity
	{
		protected Camera _camera;
		protected Game _game;
		Vector3 ambientLightColor;
		Vector3 lightDirection;
		Vector3 diffuseLightColor;
		Matrix[] modelTransforms;
		public Model _model
		{
			get;
			protected set;
		}
		protected Matrix world = Matrix.Identity;

		public BaseModel(Game game, Camera camera)
		{
			_camera = camera;
			_game = game;
		}

		public virtual void Initialize()
		{
			modelTransforms = new Matrix[_model.Bones.Count];
			_model.CopyAbsoluteBoneTransformsTo(modelTransforms);
			ambientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
			

			
			lightDirection = new Vector3(-1.0f, -1.0f, 0);
			lightDirection.Normalize();
			diffuseLightColor = new Vector3(0.7f, 0.7f, 0.7f);
		}

		public virtual void LoadContent()
		{
			foreach (ModelMesh mesh in _model.Meshes)
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
			foreach (ModelMesh mm in _model.Meshes)
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
			return (mb == _model.Root) ? mb.Transform :
				mb.Transform * GetParentTransform(mb.Parent);
		}

		protected void DrawEffect(Effect effect, Vector3 diffuseColor)
		{
			effect.Parameters["View"].SetValue(_camera.view);
			effect.Parameters["Projection"].SetValue(_camera.projection);
			effect.Parameters["AmbientLightColor"].SetValue(ambientLightColor);
			effect.Parameters["LightDirection"].SetValue(-lightDirection);
			effect.Parameters["DiffuseLightColor"].SetValue(diffuseLightColor);
			foreach (ModelMesh mesh in _model.Meshes) {
				effect.Parameters["World"].SetValue(modelTransforms[mesh.ParentBone.Index] * world);
				foreach (ModelMeshPart meshPart in mesh.MeshParts) {
					_game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
					_game.GraphicsDevice.Indices = meshPart.IndexBuffer;
					effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
					effect.CurrentTechnique.Passes[0].Apply();
					_game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
				}
			}
		}

		private void DrawModelViaVertexBuffer()
		{
			foreach (ModelMesh mm in _model.Meshes)
			{
				foreach (ModelMeshPart mmp in mm.MeshParts)
				{
					IEffectMatrices iem = mmp.Effect as IEffectMatrices;
					if ((mmp.Effect != null) && (iem != null))
					{
						iem.World = GetParentTransform(mm.ParentBone) * world;
						iem.Projection = _camera.projection;
						iem.View = _camera.view;
						_game.GraphicsDevice.SetVertexBuffer(mmp.VertexBuffer, mmp.VertexOffset);
						_game.GraphicsDevice.Indices = mmp.IndexBuffer;
						foreach (EffectPass ep in mmp.Effect.CurrentTechnique.Passes)
						{
							ep.Apply();
							_game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
								mmp.NumVertices, mmp.StartIndex, mmp.PrimitiveCount);
						}
					}
				}
			}
		}

		private void drawModel()
		{
			Matrix[] transforms = new Matrix[_model.Bones.Count];
			_model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in _model.Meshes)
			{
				foreach (Effect e in mesh.Effects)
				{
					IEffectMatrices iem = e as IEffectMatrices;
					if (iem != null)
					{
						iem.Projection = _camera.projection;
						iem.View = _camera.view;
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

		public Vector2 normalizeVelocity(Vector2 velocity)
		{
			//Console.WriteLine(velocity.X + " <-> " + velocity.Y);
			if (velocity.X != 0 && velocity.Y != 0) {
				double degree = Math.Atan2(abs(velocity.Y), abs(velocity.X));
				velocity.X = (float)Math.Cos(degree);
				velocity.Y = (float)Math.Sin(degree);
				return velocity;
			}
			Vector2 vec = new Vector2(1, 1);
			return vec;
		}

		public float abs(float num)
		{
			if (num > 0)
				return num;
			else if (num < 0)
				return -num;
			else
				return 0;
		}

	}
}
