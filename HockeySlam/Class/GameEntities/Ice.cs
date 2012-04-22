using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Class.GameState;
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

		GameManager _gameManager;

		public Ice(Game game, Camera camera, GameManager gameManager)
			: base(game, camera)
		{
			_content = game.Content;
			_graphics = game.GraphicsDevice;
			_model = _content.Load<Model>("Models/Plane");
			_iceEffect = _content.Load<Effect>("Effects/IceEffect");
			_iceEffect.Parameters["IceSurfaceTexture"].SetValue(_content.Load<Texture2D>("Textures/IceSurface2"));
			_reflectionTarg = new RenderTarget2D(_graphics, _graphics.Viewport.Width,
			_graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
			_gameManager = gameManager;
		}

		public override void Initialize()
		{
			base.Initialize();
			SetModelEffect(_iceEffect, false);
			_iceEffect.Parameters["viewportWidth"].SetValue(_graphics.Viewport.Width);
			_iceEffect.Parameters["viewportHeight"].SetValue(_graphics.Viewport.Height);
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
			_graphics.Clear(Color.White);

			foreach (IReflectable reflectable in _reflectedObjects) {
				reflectable.setClipPlane(clipPlane);
				reflectable.Draw(gameTime, reflectionCamera);
				reflectable.setClipPlane(null);
			}

			_graphics.SetRenderTarget(null);
			_iceEffect.Parameters["ReflectionMap"].SetValue(_reflectionTarg);
			_graphics.Clear(Color.CornflowerBlue);
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
			drawWithEffect(gameTime);
		}
	}
}
