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

namespace HockeySlam.Class.GameEntities.Models
{
	class Ice : BaseModel, IGameEntity
	{
		Effect _iceEffect;
		Effect _playersTrace;
		Effect _traceFadeEffect;

		ContentManager _content;
		GraphicsDevice _graphics;

		RenderTarget2D _reflectionTarg;
		RenderTarget2D _playersTarget;
		RenderTarget2D _traceFadeTarget;

		Texture2D _renderTargetTexture;
		Texture2D _traceTexture;
		Texture2D _traceFadeTexture;

		public List<IReflectable> _reflectedObjects = new List<IReflectable>();

		GameManager _gameManager;

		float _iceTransparency;
		float _blurAmount;
		int _blurType;
		int _numPlayers;
		TimeSpan _lastTime;

		public Ice(Game game, Camera camera, GameManager gameManager)
			: base(game, camera)
		{
			_content = game.Content;
			_graphics = game.GraphicsDevice;
			_model = _content.Load<Model>("Models/Plane2");

			_iceEffect = _content.Load<Effect>("Effects/IceEffect");
			_iceEffect.Parameters["IceSurfaceTexture"].SetValue(_content.Load<Texture2D>("Textures/IceSurface2"));

			_playersTrace = _content.Load<Effect>("Effects/PlayersTrace");
			_traceTexture = _content.Load<Texture2D>("Textures/trace");

			_traceFadeEffect = _content.Load<Effect>("Effects/TraceFade");

			_reflectionTarg = new RenderTarget2D(_graphics, _graphics.Viewport.Width, _graphics.Viewport.Height, 
												false, SurfaceFormat.Color, DepthFormat.Depth24);
			_playersTarget = new RenderTarget2D(_graphics, _graphics.Viewport.Width, _graphics.Viewport.Height, 
												false, SurfaceFormat.Color, DepthFormat.Depth24);
			_traceFadeTarget = new RenderTarget2D(_graphics, _graphics.Viewport.Width, _graphics.Viewport.Height,
												false, SurfaceFormat.Color, DepthFormat.Depth24);

			_renderTargetTexture = new Texture2D(_graphics, _playersTarget.Width, _playersTarget.Height);

			Color[] c = new Color[_playersTarget.Width*_playersTarget.Height];
			for(int i = 0; i < c.Length; i++)
				c[i] = Color.Black;
			_traceFadeTarget.SetData<Color>(c);

			_gameManager = gameManager;
			_numPlayers = 0;
			_lastTime = TimeSpan.Zero;
		}

		public override void Initialize()
		{
			base.Initialize();
			SetModelEffect(_iceEffect, false);
			_iceEffect.Parameters["viewportWidth"].SetValue(_graphics.Viewport.Width);
			_iceEffect.Parameters["viewportHeight"].SetValue(_graphics.Viewport.Height);
			_playersTrace.Parameters["viewportWidth"].SetValue(_graphics.Viewport.Width);
			_playersTrace.Parameters["viewportHeight"].SetValue(_graphics.Viewport.Height);

			_blurType = 0;
			_blurAmount = 0.001f;
			_iceTransparency = 0.8f;

			_iceEffect.Parameters["blurType"].SetValue(_blurType);
			_iceEffect.Parameters["blurAmount"].SetValue(_blurAmount);
			_iceEffect.Parameters["iceTransparency"].SetValue(_iceTransparency);

			_traceFadeEffect.Parameters["fade"].SetValue(0.001f);

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

		public void renderPlayersPosition(GameTime gameTime)
		{
			_graphics.SetRenderTarget(_playersTarget);
			_graphics.Clear(Color.Black);

			Rectangle[] _playerPos = new Rectangle[_numPlayers];
			SpriteBatch spriteBatch = new SpriteBatch(_graphics);

			int i = 0;
			foreach (IReflectable reflectable in _reflectedObjects) {
				if (reflectable is Player) {
					Player reflectedPlayer = (Player)reflectable;
					Vector3 playerPos = _graphics.Viewport.Project(reflectedPlayer.getPositionVector(), _camera.projection, _camera.view, Matrix.Identity);
					_playerPos[i] = new Rectangle((int)playerPos.X, (int)playerPos.Y, 2, 2);
					i++;
				}
			}

			spriteBatch.Begin(0,null,null,null,null,_playersTrace);
			foreach (Rectangle rec in _playerPos) {
				spriteBatch.Draw(_traceTexture, rec, Color.White);
			}
			spriteBatch.End();
			_graphics.SetRenderTarget(null);

			_playersTrace.Parameters["playerPosition"].SetValue(_playersTarget);

			_traceFadeTexture = new Texture2D(_graphics, _traceFadeTarget.Width, _traceFadeTarget.Height);
			Color[] content = new Color[_traceFadeTarget.Width*_traceFadeTarget.Height];
			_traceFadeTarget.GetData<Color>(content);
			_traceFadeTexture.SetData<Color>(content);

			_graphics.SetRenderTarget(_traceFadeTarget);
			spriteBatch.Begin(0, null, null, null, null, _traceFadeEffect);
			Rectangle rect = new Rectangle(0,0,_traceFadeTarget.Width, _traceFadeTarget.Height);
			spriteBatch.Draw(_traceFadeTexture, rect, Color.White);
			spriteBatch.End();
			_graphics.SetRenderTarget(null);

			_traceFadeEffect.Parameters["playerPosition"].SetValue(_playersTarget);
			_traceFadeEffect.Parameters["trace"].SetValue(_traceFadeTexture);
			_traceFadeTexture.Dispose();

			_graphics.Clear(Color.CornflowerBlue);

		}

		public void preDraw(GameTime gameTime)
		{
			renderPlayersPosition(gameTime);
			renderReflection(gameTime);
		}

		public void register(IReflectable reflectable)
		{
			_reflectedObjects.Add(reflectable);
			if (reflectable is Player)
				_numPlayers++;
		}

		public override void Draw(GameTime gameTime)
		{
			drawWithEffect(gameTime);
		}

		public float addTransparency()
		{
			_iceTransparency += 0.1f;
			if (_iceTransparency > 1)
				_iceTransparency = 1;
			_iceEffect.Parameters["iceTransparency"].SetValue(_iceTransparency);

			return _iceTransparency;
		}

		public float removeTransparency()
		{
			_iceTransparency -= 0.1f;
			if (_iceTransparency < 0)
				_iceTransparency = 0;
			_iceEffect.Parameters["iceTransparency"].SetValue(_iceTransparency);

			return _iceTransparency;
		}

		public float addBlur()
		{
			_blurAmount += 0.001f;
			_iceEffect.Parameters["blurAmount"].SetValue(_blurAmount);

			return _blurAmount;
		}

		public float removeBlur()
		{
			_blurAmount -= 0.001f;
			if (_blurAmount < 0)
				_blurAmount = 0;
			_iceEffect.Parameters["blurAmount"].SetValue(_blurAmount);

			return _blurAmount;
		}

		public int anotherBlurType()
		{
			_blurType = (_blurType + 1) % 2;
			_iceEffect.Parameters["blurType"].SetValue(_blurType);

			return _blurType;
		}

		public int getBlurType()
		{
			return _blurType;
		}

		public float getBlurAmount()
		{
			return _blurAmount;
		}

		public float getTransparency()
		{
			return _iceTransparency;
		}
	}
}
