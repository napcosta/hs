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
	public class Disk : BaseModel, IGameEntity, ICollidable, IDebugEntity, IReflectable
	{
		BoundingSphere _collisionArea;
		Vector2 _velocity;
		int _maxVelocity;
		Vector3 _position;
		bool _isColliding;

		Matrix _rotation;
		Matrix _scale;
		float drag = 0.1f;
		float moreDrag = 0.3f;

		GameManager _gameManager;

		/* -------------------- AGENTS ----------------------*/
		ReactiveAgent _playerWithDisk;
		bool _isSinglePlayer;

		public Disk(GameManager gameManager, Game game, Camera camera, bool isSinglePlayer)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\disk");
			_game = game;
			_camera = camera;
			_gameManager = gameManager;
			_isSinglePlayer = isSinglePlayer;
		}

		public override void Initialize()
		{
			_velocity = Vector2.Zero;
			_maxVelocity = 12;
			_position = new Vector3(4, 1, 0);
			_isColliding = false;

			Matrix pos = Matrix.CreateTranslation(4, 1, 0);
			_rotation = Matrix.CreateRotationX(MathHelper.PiOver2);
			_scale = Matrix.CreateScale(0.5f);

			world = world * _rotation * _scale * pos;

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

		public void notify()
		{
			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			List<ICollidable> collided = cm.verifyCollision(this);
		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(_collisionArea, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Brown);
		}

		public void AddVelocity(Vector2 velocity)
		{
			_velocity += velocity;

			if (_velocity.X > _maxVelocity)
				_velocity.X = _maxVelocity;
			else if (_velocity.X < -_maxVelocity)
				_velocity.X = -_maxVelocity;

			if (_velocity.Y > _maxVelocity)
				_velocity.Y = _maxVelocity;
			else if (_velocity.Y < -_maxVelocity)
				_velocity.Y = -_maxVelocity;

			_isColliding = true;
		}

		public void AddRotationVelocity(Vector2 velocity)
		{
			_velocity += velocity;
		}

		public override void Update(GameTime gameTime)
		{
			if (!_isSinglePlayer || _playerWithDisk == null) {


				if (!_isColliding) {
					UpdateVelocityX(drag, moreDrag);
					UpdateVelocityY(drag, moreDrag);
				}

				_isColliding = false;
				
				Vector2 normalizedVelocity = normalizeVelocity(_velocity);
				float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

				_position.X += time * _velocity.Y * normalizedVelocity.Y;
				_position.Z += time * _velocity.X * normalizedVelocity.X;

				_collisionArea.Center.X = _position.X;
				_collisionArea.Center.Z = _position.Z;
				notify();
			} else {
				_position = _playerWithDisk.getPlayer().getStickPosition();
				_collisionArea.Center = _playerWithDisk.getPlayer().getStickPosition();
				_velocity = Vector2.Zero;
			}
			
			base.Update(gameTime);
		}

		private void UpdateVelocityY(float drag, float moreDrag)
		{
			if (Math.Abs(_velocity.Y) > _maxVelocity) {
				if (_velocity.Y >= moreDrag)
					_velocity.Y -= moreDrag;
				else if (_velocity.Y <= -moreDrag)
					_velocity.Y += moreDrag;
				else _velocity.Y = 0;
			} else {
				if (_velocity.Y >= drag)
					_velocity.Y -= drag;
				else if (_velocity.Y <= -drag)
					_velocity.Y += drag;
				else _velocity.Y = 0;
			}
		}

		private void UpdateVelocityX(float drag, float moreDrag)
		{
			if (Math.Abs(_velocity.X) <= _maxVelocity) {
				if (_velocity.X >= drag)
					_velocity.X -= drag;
				else if (_velocity.X <= -drag)
					_velocity.X += drag;
				else _velocity.X = 0;
			} else {
				if (_velocity.X >= moreDrag)
					_velocity.X -= moreDrag;
				else if (_velocity.X <= -moreDrag)
					_velocity.X += moreDrag;
				else _velocity.X = 0;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			Matrix pos = Matrix.CreateTranslation(_position.X, _position.Y, _position.Z);

			world = Matrix.Identity;
			world *= _rotation * _scale * pos;

			_camera.updateDiskPosition(_position);

			base.Draw(gameTime);
		}

		/* This draw is used for the ice reflection */
		void IReflectable.Draw(GameTime gameTime, Camera camera)
		{
			Camera lastCamera = _camera;
			_camera = camera;
			_game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			Draw(gameTime);
			_game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			_camera = lastCamera;
		}

		void IReflectable.setClipPlane(Vector4? plane)
		{
		}

		public Vector3 getPosition()
		{
			return _position;
		}

		public void synchPosition(Vector3 pos)
		{
			_position = pos;
		}


		public List<BoundingBox> getBoundingBoxes()
		{
			return null;
		}

		public Boolean collisionOccured(ICollidable collideObject)
		{
			List<BoundingSphere> bss = collideObject.getBoundingSpheres();

			foreach (BoundingSphere bs in bss) {
				if (bs.Intersects(_collisionArea))
					return true;
			}
			return false;
		}

		public void bounce(Vector2 newVelocity)
		{
			_velocity = newVelocity;
		}

		public Vector2 getVelocity()
		{
			return _velocity;
		}

		/* -------------------------- AGENTS --------------------------- */

		public void newPlayerWithDisk(ReactiveAgent player) {
			player.removePlayerDisk();
			_playerWithDisk = player;
		}

		// Direction coordinates must be beetween 0 and 1
		public void shoot(Vector2 direction)
		{
			_velocity = direction*_maxVelocity*5;
			_playerWithDisk = null;
		}

		public ReactiveAgent getPlayerWithDisk()
		{
			return _playerWithDisk;
		}
	}
}
