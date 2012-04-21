using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using HockeySlam.Class.GameEntities;
using HockeySlam;
using HockeySlam.Class.GameState;
using HockeySlam.Interface;


namespace HockeySlam.Class.GameEntities.Models
{
	public class Player : BaseModel, ICollidable, IDebugEntity, IReflectable
	{
		#region fields
		Vector2 _velocity;
		Matrix position;
		Vector3 lastPositionVector;
		Vector3 lastPosition;
		Vector3 _positionOfCollision;
		
		float tempRotation;
		float lastTempRotation;
		float _rotation;
		float _rotationOfCollision;

		List<Boolean> arrowKeysPressed;
		List<Keys> lastArrowKeysPressed;
		
		Game game;
		Camera camera;
		BoundingSphere stick;
		Effect effect;
		BoundingSphere upBody;
		BoundingSphere downBody;
		GameManager _gameManager;
		Vector3 _positionVector;

		public float Rotation
		{
			get;
			set;
		}
		
		/* RotationInput and PositionInput are two parameters 
		 * that will be set by the MultiplayerManager */
		public Vector4 RotationInput
		{
			get;
			set;
		}

		public Vector2 PositionInput
		{
			get;
			set;
		}
		/***************************************************/


		bool _isRemote;
		#endregion

		public Player(GameManager gameManager, Game game, Camera camera, bool isRemote) : base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\player");
			this.game = game;
			this.camera = camera;
			_gameManager = gameManager;
			_isRemote = isRemote;
		}

		public Vector3 getPositionVector()
		{
			return _positionVector;
		}

		public void setPositionVector(Vector3 vec)
		{
			_positionVector = vec;
		}

		public override void LoadContent()
		{
			effect = game.Content.Load<Effect>(@"Effects\SimpleEffect");
			base.LoadContent();
		}
		
		public override void Initialize()
		{	
			// TODO: Add your initialization code here
			tempRotation = 0;
			lastTempRotation = tempRotation;

			position = Matrix.Identity;
			_positionVector = Vector3.Zero;
			_positionVector.Y = 0.7f;
			lastPositionVector = _positionVector;

			_velocity = Vector2.Zero;

			Matrix pos = Matrix.CreateTranslation(0, 0.7f, 0);
			Matrix scale = Matrix.CreateScale(1.5f);
			world = world * scale * pos;

			arrowKeysPressed = new List<Boolean>();
			for(int i = 0; i < 4; i++)
				arrowKeysPressed.Add(false);
			lastArrowKeysPressed = new List<Keys>();

			lastPosition = new Vector3(0, 2, 0);
			stick = new BoundingSphere(new Vector3(2, 1f, 0), 0.5f);
			upBody = new BoundingSphere(new Vector3(0, 4.5f, 0), 1.2f);
			downBody = new BoundingSphere(new Vector3(0, 1.05f, -0.03f), 0.05f);

			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			DebugManager dm = (DebugManager)_gameManager.getGameEntity("debugManager");
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.register(this);
			cm.register(this);
			dm.registerDebugEntities(this);
			base.Initialize();
		}

		public override void Draw(GameTime gameTime)
		{
			Vector3 diffuseColor;
			diffuseColor = new Vector3(1, 0.25f, 0.25f);
			//diffuseColor[1] = new Vector3(0.25f, 1, 0.25f);
			//diffuseColor[2] = new Vector3(0.25f, 0.25f, 1);
			//diffuseColor[3] = new Vector3(0.5f, 0.5f, 0.5f);
			base.DrawEffect(effect, diffuseColor);
			//base.Draw(gameTime);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// TODO: Add your update code here
			_rotation = 0;
#if WINDOWS
			KeyboardState currentKeyboardState = Keyboard.GetState();
			float lastTempRotation = tempRotation;

			#region Position

				if (PositionInput.Y == 2 && _velocity.Y < 30) {
					_velocity.Y += 1;
				} else if (PositionInput.Y == 0 && _velocity.Y > 0) {
					_velocity.Y -= (float)0.5;
				}

				if (PositionInput.Y == 1 && _velocity.Y > -30) {
					_velocity.Y -= 1;
				} else if (PositionInput.Y == 0 && _velocity.Y < 0) {
					_velocity.Y += (float)0.5;
				}

				if (PositionInput.X == 2 && _velocity.X > -30) {
					_velocity.X -= 1;
				} else if (PositionInput.X == 0 && _velocity.X < 0) {
					_velocity.X += (float)0.5;
				}

				if (PositionInput.X == 1 && _velocity.X < 30) {
					_velocity.X += 1;
				} else if (PositionInput.X == 0 && _velocity.X > 0) {
					_velocity.X -= (float)0.5;
				}
			#endregion
			#region Rotation

			int indexToConsider;

			indexToConsider = getPriorityIndex(); //Index from the PriorityVector
			
			/* 0 -> UP
			 * 1 -> DOWN
			 * 2 -> LEFT
			 * 3 -> RIGHT */
			if (indexToConsider == 2 &&
				((tempRotation >= 0.0f && tempRotation <= MathHelper.PiOver2) ||
				(tempRotation <= -3 * MathHelper.PiOver2 && tempRotation >= -2 * MathHelper.Pi) ||
				(tempRotation >= 3 * MathHelper.PiOver2 && tempRotation <= 2 * MathHelper.Pi) ||
				(tempRotation <= 0.0f && tempRotation >= -MathHelper.PiOver2)))
			{
				_rotation = -0.1f;
			}
			else if (indexToConsider == 2 &&
				((tempRotation >= MathHelper.PiOver2 && tempRotation <= 3 * MathHelper.PiOver2) ||
				(tempRotation <= -MathHelper.PiOver2 && tempRotation >= -3 * MathHelper.Pi)))
			{
				_rotation = 0.1f;
			}
			else if (indexToConsider == 3 &&
				((tempRotation >= 0.0f && tempRotation <= MathHelper.PiOver2) ||
				(tempRotation <= -3 * MathHelper.PiOver2 && tempRotation >= -2 * MathHelper.Pi) ||
				(tempRotation >= 3 * MathHelper.PiOver2 && tempRotation <= 2 * MathHelper.Pi) ||
				(tempRotation <= 0.0f && tempRotation >= -MathHelper.PiOver2)))
			{
				_rotation = 0.1f;
			}
			else if (indexToConsider == 3 &&
				((tempRotation >= MathHelper.PiOver2 && tempRotation <= 3 * MathHelper.PiOver2) ||
				(tempRotation <= -MathHelper.PiOver2 && tempRotation >= -3 * MathHelper.Pi)))
			{
				_rotation = -0.1f;
			}
			else if (indexToConsider == 0 &&
				((tempRotation >= 0.0f && tempRotation <= MathHelper.Pi) ||
				(tempRotation >= -2 * MathHelper.Pi && tempRotation <= -MathHelper.Pi)))
			{
				_rotation = 0.1f;
			}
			else if (indexToConsider == 0 &&
				((tempRotation >= MathHelper.Pi && tempRotation <= 2 * MathHelper.Pi) ||
				(tempRotation <= 0 && tempRotation >= -MathHelper.Pi)))
			{
				_rotation = -0.1f;
			}
			else if (indexToConsider == 1 &&
			((tempRotation >= 0.0f && tempRotation <= MathHelper.Pi) ||
			(tempRotation >= -2 * MathHelper.Pi && tempRotation <= -MathHelper.Pi)))
			{
				_rotation = -0.1f;
			}
			else if (indexToConsider == 1 &&
				((tempRotation >= MathHelper.Pi && tempRotation <= 2 * MathHelper.Pi) ||
				(tempRotation <= 0 && tempRotation >= -MathHelper.Pi)))
			{
				_rotation = 0.1f;
			}
			else _rotation = 0.0f;
			//Console.WriteLine("RotationInput -> " + RotationInput + " Rotation -> " + _rotation);
			#endregion

#else
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            Vector2 leftThumStick = currentGamePadState.ThumbSticks.Left;

            Vector2 maxVelocity;
            maxVelocity.X = 100 * Math.Abs(leftThumStick.Y);
            maxVelocity.Y = 100 * Math.Abs(leftThumStick.X);

            if (leftThumStick.X != 0 && _velocity.Y > -maxVelocity.Y && _velocity.Y < maxVelocity.Y)
            {
                _velocity.Y += leftThumStick.X;
            }
			else if (_velocity.Y < -(float)0.5)
            {
                _velocity.Y += (float)0.5;
            }
			else if (_velocity.Y > (float)0.5)
            {
				_velocity.Y -= (float)0.5;
            }
            else _velocity.Y = 0;

            if (leftThumStick.Y != 0 && _velocity.X > -maxVelocity.X && _velocity.X < maxVelocity.X)
            {
                _velocity.X += leftThumStick.Y;
            }
			else if (_velocity.X < -(float)0.5)
            {
				_velocity.X += (float)0.5;
            }
			else if (_velocity.X > (float)0.5)
            {
				_velocity.X -= (float)0.5;
            }
            else _velocity.X = 0;
#endif
			Vector2 normalizedVelocity = normalizeVelocity(_velocity);
			float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

			updateMeshWorld(gameTime, _rotation, normalizedVelocity, time);
			updatePositionVector(gameTime, normalizedVelocity, time);
			updateBoundingSpheres(gameTime, lastTempRotation, normalizedVelocity, time);
		}

		private int getPriorityIndex()
		{
			float temp = 0;
			int indexTemp = -1;

			if (temp < RotationInput.X) {
				temp = RotationInput.X;
				indexTemp = 0;
			}
			if (temp < RotationInput.Y) {
				temp = RotationInput.Y;
				indexTemp = 1;
			}
			if (temp < RotationInput.Z) {
				temp = RotationInput.Z;
				indexTemp = 2;
			}
			if (temp < RotationInput.W) {
				temp = RotationInput.W;
				indexTemp = 3;
			}	
			return indexTemp;
		}

		private void updateMeshWorld(GameTime gameTime, float rotation, Vector2 normalizedVelocity, float time) 
		{
			tempRotation = (tempRotation + rotation) % MathHelper.TwoPi;
			Matrix oldWorld = world;
			world = Matrix.Identity;
			world *= Matrix.CreateRotationY(rotation);
			world *= oldWorld;
			position = Matrix.CreateTranslation(time * _velocity.Y*normalizedVelocity.Y, 0, time * _velocity.X*normalizedVelocity.X);

			world = world * position;
		}

		private void updatePositionVector(GameTime gameTime, Vector2 normalizedVelocity, float time)
		{
			_positionVector += new Vector3(time * _velocity.Y * normalizedVelocity.Y, 0, time * _velocity.X * normalizedVelocity.X);

			if (_positionVector != lastPositionVector || tempRotation != lastTempRotation) {
				notify();
				lastPositionVector = _positionVector;
				lastTempRotation = tempRotation;
			}
		}

		private void updateBoundingSpheres(GameTime gameTime, float lastTempRotation, Vector2 normalizedVelocity, float time)
		{	
			lastPosition = new Vector3(time * _velocity.Y * normalizedVelocity.Y, 0, time * _velocity.X * normalizedVelocity.X);

			upBody.Center += lastPosition;
			downBody.Center += lastPosition;

			if (lastTempRotation != tempRotation) {
				stick.Center = Vector3.Zero;
				stick.Center.X += 2f * ((float)Math.Sin(tempRotation + MathHelper.PiOver2));
				stick.Center.Z += 2f * ((float)Math.Cos(tempRotation + MathHelper.PiOver2));
				stick.Center += upBody.Center;
				stick.Center.Y = 1f;
			}
			else stick.Center += lastPosition;
		}

		public List<BoundingSphere> getBoundingSpheres()
		{
			List<BoundingSphere> bs = new List<BoundingSphere>();

			bs.Add(stick);
			bs.Add(upBody);
			bs.Add(downBody);

			return bs;
		}

		public bool collisionOccured(List<BoundingSphere> bss)
		{
			foreach (BoundingSphere bs in bss) {
				if (bs.Intersects(stick))
					return true;
				if (bs.Intersects(upBody))
					return true;
				if (bs.Intersects(downBody))
					return true;
			}

			return false;
		}

		public void notify()
		{
			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			List<ICollidable> collidedWith = cm.verifyCollision(this);

			if (collidedWith.Count != 0 && (_positionOfCollision != _positionVector || _rotationOfCollision != tempRotation)) {
				Console.WriteLine("CollisionOcurred");
				foreach(ICollidable collided in collidedWith)
					if (collided is Disk) {
						Disk disk = (Disk)collided;
						disk.AddVelocity(_velocity);
						if (_rotation != 0)
							disk.AddVelocity(new Vector2(10, 10) * -(new Vector2((float)Math.Sin(tempRotation), (float)Math.Cos(tempRotation))));
					}
				_positionOfCollision = _positionVector;
				if (_rotation != 0)
					_rotationOfCollision = tempRotation;
			} else Console.WriteLine("NOT-CollisionOcurred");
		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(stick, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
			BoundingSphereRender.Render(upBody, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
			BoundingSphereRender.Render(downBody, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
		}

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

		public void updateVelocity(Vector2 velocity)
		{
			_velocity = velocity;
		}

		public Vector2 getVelocity()
		{
			if(_velocity != null)
				return _velocity;
			return Vector2.Zero;
		}

		public bool positionHasChandeg()
		{
			return lastPosition != _positionVector;
		}
	}
}
