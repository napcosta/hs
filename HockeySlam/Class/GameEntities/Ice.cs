using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities
{
	class Ice : BaseModel, IGameEntity
	{
		Effect _iceEffect;

		ContentManager _content;
		GraphicsDevice _graphics;

		RenderTarget2D _reflectionTarg;
		public List<IReflectable> _reflectedObjects = new List<IReflectable>();

		public Ice(Game game, Camera camera)
			: base(game, camera)
		{
			_content = game.Content;
			_graphics = game.GraphicsDevice;
			_model = _content.Load<Model>("Models/Plane");
			_iceEffect = _content.Load<Effect>("Effects/IceEffect");
			_iceEffect.Parameters["viewportWidth"].SetValue(_graphics.Viewport.Width);
			_iceEffect.Parameters["viewportHeight"].SetValue(_graphics.Viewport.Height);
			_reflectionTarg = new RenderTarget2D(_graphics, _graphics.Viewport.Width,
				_graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
		}

		public void renderReflection(GameTime gameTime)
		{
			Vector3 reflectedCameraPosition = _camera.getPosition();
			Vector3 reflectedCameraTarget = _camera.getTarget();

			reflectedCameraPosition.Y = -reflectedCameraPosition.Y;
			reflectedCameraTarget.Y = -reflectedCameraTarget.Y;

			Camera reflectionCamera = new Camera(_game, reflectedCameraPosition, reflectedCameraTarget, Vector3.Up);
			_iceEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.view);

			Vector4 clipPlane = new Vector4(0, 1, 0, 0);
			_graphics.SetRenderTarget(_reflectionTarg);
			_graphics.Clear(Color.Black);

			foreach (IReflectable reflectable in _reflectedObjects) {
				reflectable.setClipPlane(clipPlane);
				reflectable.Draw(gameTime, reflectionCamera);
				reflectable.setClipPlane(null);
			}

			_graphics.SetRenderTarget(null);
			_iceEffect.Parameters["ReflectionMap"].SetValue(_reflectionTarg);
		}

		public void preDraw(GameTime gameTime)
		{
			renderReflection(gameTime);
		}

		public void register(IReflectable reflectable)
		{
			_reflectedObjects.Add(reflectable);
		}

		public override void Draw(GameTime gameTime)
		{
			_iceEffect.Parameters["World"].SetValue(world);
			_iceEffect.Parameters["View"].SetValue(_camera.view);
			_iceEffect.Parameters["Projection"].SetValue(_camera.projection);
			_iceEffect.Parameters["CameraPosition"].SetValue(_camera.getPosition());
			foreach (ModelMesh mesh in _model.Meshes) {
				foreach (ModelMeshPart meshPart in mesh.MeshParts) {
					_game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
					_game.GraphicsDevice.Indices = meshPart.IndexBuffer;
					_iceEffect.CurrentTechnique.Passes[0].Apply();
					_game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
				}
			}
			base.Draw(gameTime);
		}
	}
}
