using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using HockeySlam.Class.GameState;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities.Models
{
	class Court : BaseModel, IReflectable, IDebugEntity, ICollidable
	{
		Matrix rotation = Matrix.Identity;
		Effect _effect;
		GameManager _gameManager;

		BoundingBox _leftBox;
		BoundingBox _rightBox;
		BoundingBox _frontBox;
		BoundingBox _backBox;

		BoundingBox _team1Goal;
		BoundingBox _team2Goal;

		public Court(Game game, Camera camera, GameManager gameManager)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\court2");
			_effect = game.Content.Load<Effect>(@"Effects\SimpleEffect");
			_gameManager = gameManager;
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}

		public override void Initialize()
		{
			_leftBox = new BoundingBox(new Vector3(-51, 0, 56), new Vector3(51, 10, 65));
			_backBox = new BoundingBox(new Vector3(-41, 0,-65), new Vector3(-51, 10, 65));
			_rightBox = new BoundingBox(new Vector3(-51, 0, -56), new Vector3(51, 10, -65));
			_frontBox = new BoundingBox(new Vector3(41, 0, -65), new Vector3(51, 10, 65));

			_team1Goal = new BoundingBox(new Vector3(-10, 0, 55), new Vector3(10, 10, 65));
			_team2Goal = new BoundingBox(new Vector3(-10, 0, -55), new Vector3(10, 10, -65));

			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.register(this);

			DebugManager dm = (DebugManager)_gameManager.getGameEntity("debugManager");
			dm.registerDebugEntities(this);

			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			cm.register(this);

			base.Initialize();
		}

		public override void Draw(GameTime gameTime)
		{
			Vector3 diffuseColor;
			diffuseColor = new Vector3(0.75f, 0.75f, 0.8f);
			base.DrawEffect(_effect, diffuseColor);
		}

		public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}

		void IReflectable.Draw(GameTime gameTime, Camera camera)
		{
			Camera lastCamera = _camera;
			_camera = camera;
			Draw(gameTime);
			_camera = lastCamera;
		}

		void IReflectable.setClipPlane(Vector4? plane)
		{
			
		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(_leftBox, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Yellow);
			BoundingSphereRender.Render(_backBox, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Yellow);
			BoundingSphereRender.Render(_rightBox, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Yellow);
			BoundingSphereRender.Render(_frontBox, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Yellow);

			BoundingSphereRender.Render(_team1Goal, _game.GraphicsDevice, _camera.view, _camera.projection, Color.AliceBlue);
			BoundingSphereRender.Render(_team2Goal, _game.GraphicsDevice, _camera.view, _camera.projection, Color.PaleVioletRed);
		}

		public void notify()
		{
			
		}


		public bool collisionOccured(ICollidable collideObject)
		{
			bool collisionOccured = false;
			bool isObjectPlayer = collideObject is Player;
			Vector3 playerPosition = Vector3.Zero;
			bool[] deactivateKeys = new bool[4];
			Player player = null;
			if (isObjectPlayer) {
				player = (Player)collideObject;
				playerPosition = player.getPositionVector();
				for (int i = 0; i < 4; i++)
					deactivateKeys[i] = false;
			}

			List<BoundingSphere> bs = collideObject.getBoundingSpheres();
			foreach (BoundingSphere sphere in bs) {
				if (!collisionOccured) {
					if (_leftBox.Intersects(sphere)) {
						Vector2 bounceVelocity = collideObject.getVelocity();
						if(bounceVelocity.X > 0)
							bounceVelocity.X *= -1;
						collideObject.bounce(bounceVelocity);
						if (isObjectPlayer)
							deactivateKeys[(int)KeyboardKey.LEFT] = true;
						collisionOccured = true;
					}
					if (_rightBox.Intersects(sphere)) {
						Vector2 bounceVelocity = collideObject.getVelocity();
						if(bounceVelocity.X < 0)
							bounceVelocity.X *= -1;
						collideObject.bounce(bounceVelocity);
						if (isObjectPlayer)
							deactivateKeys[(int)KeyboardKey.RIGHT] = true;
						collisionOccured = true;
					}
					if (_backBox.Intersects(sphere)) {
						Vector2 bounceVelocity = collideObject.getVelocity();
						if(bounceVelocity.Y < 0)
							bounceVelocity.Y *= -1;
						collideObject.bounce(bounceVelocity);
						if (isObjectPlayer)
							deactivateKeys[(int)KeyboardKey.UP] = true;
						collisionOccured = true;
					}
					if (_frontBox.Intersects(sphere)) {
						Vector2 bounceVelocity = collideObject.getVelocity();
						if(bounceVelocity.Y > 0)
							bounceVelocity.Y *= -1;
						collideObject.bounce(bounceVelocity);
						if (isObjectPlayer)
							deactivateKeys[(int)KeyboardKey.DOWN] = true;
						collisionOccured = true;
					}

					if (isObjectPlayer)
						player.deactivateKeys(deactivateKeys);
				}

			}

			return collisionOccured;
		}

		public List<BoundingBox> getBoundingBoxes()
		{
			List<BoundingBox> boundingBoxes = new List<BoundingBox>();

			boundingBoxes.Add(_leftBox);
			boundingBoxes.Add(_rightBox);
			boundingBoxes.Add(_backBox);
			boundingBoxes.Add(_frontBox);

			return boundingBoxes;
		}

		public List<BoundingSphere> getBoundingSpheres()
		{
			return null;
		}

		public void bounce(Vector2 newVelocity) { }

		public Vector2 getVelocity()
		{
			return Vector2.Zero;
		}

		public BoundingBox getTeam1Goal()
		{
			return _team1Goal;
		}

		public BoundingBox getTeam2Goal()
		{
			return _team2Goal;
		}

		public Vector2 getTeam1GoalPosition()
		{
			return new Vector2(0, 56);
		}

		public Vector2 getTeam2GoalPosition()
		{
			return new Vector2(0, -56);
		}
	}
}