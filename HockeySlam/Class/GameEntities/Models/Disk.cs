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
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameState;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities.Models
{
	class Disk : BaseModel, IGameEntity, ICollidable, IDebugEntity, IReflectable
	{
		BoundingSphere _collisionArea;
		Vector2 _velocity;
		Vector3 _position;
		GameManager _gameManager;

		public Disk(GameManager gameManager, Game game, Camera camera)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\disk");
			_game = game;
			_camera = camera;
			_gameManager = gameManager;
		}

		public override void Initialize()
		{
			_velocity = Vector2.Zero;
			_position = new Vector3(4, 1, 0);

			Matrix pos = Matrix.CreateTranslation(4, 1, 0);
			Matrix rotation = Matrix.CreateRotationX(MathHelper.PiOver2);
			Matrix scale = Matrix.CreateScale(0.5f);

			world = world * rotation * scale * pos;

			_collisionArea = new BoundingSphere(new Vector3(4,1,0), 0.45f);

			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			DebugManager dm = (DebugManager)_gameManager.getGameEntity("debugManager");
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.register(this);
			cm.register(this);
			dm.registerDebugEntities(this);

			base.Initialize();
		}

		public List<BoundingSphere> getBoundingSpheres()
		{
			List<BoundingSphere> bs = new List<BoundingSphere>();

			bs.Add(_collisionArea);

			return bs;
		}

		public bool collisionOccured(List<BoundingSphere> bss)
		{
			foreach (BoundingSphere bs in bss) {
				if(bs.Intersects(_collisionArea))
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
			BoundingSphereRender.Render(_collisionArea, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Brown);
		}

		public void AddVelocity(Vector2 velocity)
		{
			if(_velocity.X >= -30.0f && _velocity.X <= 30.0f && _velocity.Y >= -30.0f && _velocity.Y <= 30.0f)
				_velocity += velocity;
		}

		public override void Update(GameTime gameTime)
		{
			float drag = 0.5f;

			if (_velocity.X >= drag)
				_velocity.X -= drag;
			else if (_velocity.X <= -drag)
				_velocity.X += drag;
			else _velocity.X = 0;

			if (_velocity.Y >= drag)
				_velocity.Y -= drag;
			else if (_velocity.Y <= -drag)
				_velocity.Y += drag;
			else _velocity.Y = 0;

			Vector2 normalizedVelocity = normalizeVelocity(_velocity);
			float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

			_position.X += time * _velocity.Y * normalizedVelocity.Y;
			_position.Z += time * _velocity.X * normalizedVelocity.X;

			_collisionArea.Center.X = _position.X;
			_collisionArea.Center.Z = _position.Z;

			Matrix position = Matrix.CreateTranslation(time * _velocity.Y * normalizedVelocity.Y, 0, time * _velocity.X * normalizedVelocity.X);
			world *= position;

			base.Update(gameTime);
		}

		void IReflectable.Draw(GameTime gameTime, Camera camera)
		{
			Camera lastCamera = _camera;
			_camera = camera;
			_game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			base.Draw(gameTime);
			_game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			_camera = lastCamera;
		}

		void IReflectable.setClipPlane(Vector4? plane)
		{
		}
	}
}
