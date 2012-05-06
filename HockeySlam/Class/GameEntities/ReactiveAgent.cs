using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using HockeySlam.Class.GameEntities.Models;

using HockeySlam.Class.GameState;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities
{
	class ReactiveAgent : IDebugEntity
	{
		BoundingFrustum _fov;
		Player _player;
		float _viewDistance=50;
		Game _game;
		Camera _camera;
		Court _court;
		Disk _disk;
		BoundingSphere _boundingSphere;
		Random _randomGenerator;
		Vector2 _direction;
		Vector3 _fovTarget;
		float _fovRotation;
		float _farPlane;

		private Matrix view
		{
			get;
			set;
		}

		private Matrix projection
		{
			get;
			set;
		}
		public ReactiveAgent(GameManager gameManager, Game game, Camera camera)
		{
			_player = new Player(gameManager, game, camera);
			_game = game;
			_camera = camera;
			_fovRotation = 0;
			float x, y, z;
			x = (float)Math.Cos(_fovRotation) * _viewDistance;
			z = (float)Math.Sin(_fovRotation) * _viewDistance;
			y = _player.getPositionVector().Y;
			//Vector3 vec = new Vector3(_player.getPositionVector().X-10f, _player.getPositionVector().Y, _player.getPositionVector().Z-10f);
			view = Matrix.CreateLookAt(_player.getPositionVector(), new Vector3(x, y, z), Vector3.Up);

			_farPlane = 20;

			projection = Matrix.CreatePerspectiveFieldOfView(
			    MathHelper.PiOver4,
			    (float)game.Window.ClientBounds.Width /
			    (float)game.Window.ClientBounds.Height,
			    1, _farPlane);

			_fov = new BoundingFrustum(view * projection);

			DebugManager dm = (DebugManager)gameManager.getGameEntity("debugManager");
			dm.registerDebugEntities(this);

			_court = (Court)gameManager.getGameEntity("court");
			_disk = (Disk)gameManager.getGameEntity("disk");

			_boundingSphere = new BoundingSphere(_player.getPositionVector(), 2.3f);

			_randomGenerator = new Random();
			_direction = Vector2.Zero;
			_direction.Y = -1;

			_player.Initialize();
			_player.LoadContent();
		}



		public void DrawDebug()
		{
			BoundingSphereRender.Render(_fov, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Red);
			BoundingSphereRender.Render(_boundingSphere, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Blue);

		}

		private bool moveTowardsDisk()
		{
			Vector2 newPositionInput = Vector2.Zero;
			List<BoundingSphere> listBounding = _disk.getBoundingSpheres();

			if (listBounding[0].Intersects(_boundingSphere)) {
				_player.PositionInput = Vector2.Zero;
				return true;
			}
			if (_player.getVelocity().X > _player.getMaxVelocity()/2 || _player.getVelocity().Y > _player.getMaxVelocity()/2) {
				if(_player.getVelocity().X > _player.getMaxVelocity()/2)
					newPositionInput.X = 0;
				if (_player.getVelocity().Y > _player.getMaxVelocity() / 2)
					newPositionInput.Y = 0;
				_player.PositionInput = newPositionInput;
				return false;
			}
			if (_player.getPositionVector().X > _disk.getPosition().X)
				newPositionInput.X = 1;
			if (_player.getPositionVector().X < _disk.getPosition().X)
				newPositionInput.X = 2;
			if (_player.getPositionVector().X == _disk.getPosition().X)
				newPositionInput.X = 0;
			/*if (_player.getPositionVector().Z > _disk.getPosition().Y)
				newPositionInput.Y = 1;
			if (_player.getPositionVector().Z < _disk.getPosition().Y)
				newPositionInput.Y = 2;
			if (_player.getPositionVector().Z == _disk.getPosition().Y)
				newPositionInput.Y = 0;*/

			_player.PositionInput = newPositionInput;

			return false;
		}

		public void update(GameTime gameTime)
		{
			generateKeys();
			_player.Update(gameTime);
			Vector3 playerPosition = _player.getPositionVector();
			_boundingSphere.Center = playerPosition;
			float x, y, z;
			x = (float)Math.Cos(_fovRotation) * _viewDistance + playerPosition.X;
			z = (float)Math.Sin(_fovRotation) * _viewDistance + playerPosition.Z;
			y = _player.getPositionVector().Y;
			view = Matrix.CreateLookAt(_player.getPositionVector(), new Vector3(x, y, z), Vector3.Up);
			_fov.Matrix = view * projection;
		}

		public void draw(GameTime gameTime)
		{
			_player.Draw(gameTime);
		}

		private void generateKeys()
		{
			bool isDiskInRange;
			if (isDiskAhead()) {
				isDiskInRange = moveTowardsDisk();
				if (isDiskInRange) {
					System.Console.WriteLine("BALL IN RANGE");
					shoot();
				}
			} else if (isWallAhead())
				rotate();
			else
				moveRandomly();
		}

		private void moveTowardsDirection()
		{
			Vector2 newPositionInput = Vector2.Zero;
			if (_direction.X > 0)
				newPositionInput.X = 1;
			if (_direction.X < 0)
				newPositionInput.X = 2;
			if (_direction.Y > 0)
				newPositionInput.Y = 2;
			if (_direction.Y < 0)
				newPositionInput.Y = 1;

			_player.PositionInput = newPositionInput;
		}

		private void rotate()
		{
			if (_randomGenerator.Next(2) == 0) {
				_fovRotation += 0.2f;
				_direction.X = (float)Math.Sin(_fovRotation);
			} else {
				_fovRotation -= 0.2f;
				_direction.Y = (float)Math.Cos(_fovRotation);
			}
		}

		private void moveRandomly()
		{
			if (_randomGenerator.Next(2) == 0)
				moveTowardsDirection();
			else
				rotate();
		}

		private void shoot()
		{
			_player.PositionInput = Vector2.Zero;
			_player.setRotation(0.1f);
		}

		private bool isDiskAhead()
		{
			List<BoundingSphere> boundingList = _disk.getBoundingSpheres();
			foreach(BoundingSphere bs in boundingList) {
				if(bs.Intersects(_fov))
					return true;		
			}
			return false;
		}

		private bool isWallAhead()
		{
			List<BoundingBox> boundingList = _court.getBoundingBoxes();
			foreach (BoundingBox bb in boundingList) {
				if (bb.Intersects(_fov))
					return true;
			}
			return false;
		}
	}
}
