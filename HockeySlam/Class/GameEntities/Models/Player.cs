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
	public enum KeyboardKey { NONE, UP, DOWN, LEFT, RIGHT };

	public class Player : BaseModel, ICollidable, IDebugEntity, IReflectable
	{
		#region fields
		Vector2 _velocity;
		int _maxVelocity;
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

		BoundingSphere stick;
		Effect effect;
		BoundingSphere upBody;
		BoundingSphere downBody;
		GameManager _gameManager;
		Vector3 _positionVector;
		Texture2D _arrow;

		Matrix _scale;

		VertexPositionColor[] _verts;
		VertexBuffer _vertexBuffer;
		BasicEffect _basicEffect;
		Disk _disk;
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


		#endregion

		public Player(GameManager gameManager, Game game, Camera camera)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\player");
			_gameManager = gameManager;
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
			effect = _game.Content.Load<Effect>(@"Effects\SimpleEffect");
			_verts = new VertexPositionColor[3];
			_verts[0] = new VertexPositionColor(new Vector3(0, 0.8f, -10), Color.Blue);
			_verts[1] = new VertexPositionColor(new Vector3(10, 0.8f, -10), Color.Red);
			_verts[2] = new VertexPositionColor(new Vector3(5, 0.8f, 10), Color.Green);
			_vertexBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPositionColor),
				_verts.Length, BufferUsage.None);
			_vertexBuffer.SetData(_verts);
			_basicEffect = new BasicEffect(_game.GraphicsDevice);

			_arrow = _game.Content.Load<Texture2D>("Textures/Arrow");
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
			_maxVelocity = 12;

			Matrix pos = Matrix.CreateTranslation(0, 0.7f, 0);
			_scale = Matrix.CreateScale(1.5f);
			world = world * _scale * pos;

			arrowKeysPressed = new List<Boolean>();
			for (int i = 0; i < 4; i++)
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

			_disk = ((MultiplayerManager)_gameManager.getGameEntity("multiplayerManager")).getDisk();
			base.Initialize();
		}

		public override void Draw(GameTime gameTime)
		{
			isOutOfScreen();
			Vector3 diffuseColor;
			diffuseColor = new Vector3(1, 0.25f, 0.25f);
			//diffuseColor[1] = new Vector3(0.25f, 1, 0.25f);
			//diffuseColor[2] = new Vector3(0.25f, 0.25f, 1);
			//diffuseColor[3] = new Vector3(0.5f, 0.5f, 0.5f);
			updateMeshWorld(gameTime);
			base.DrawEffect(effect, diffuseColor);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// TODO: Add your update code here
			_rotation = 0;
			float lastRotation = Rotation;
#if WINDOWS
			KeyboardState currentKeyboardState = Keyboard.GetState();


			UpdatePosition();
			UpdateRotation();
			isOutOfScreen();
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

			Rotation = (Rotation + _rotation) % MathHelper.TwoPi;
			updatePositionVector(gameTime, normalizedVelocity, lastRotation, time);
			updateBoundingSpheres(gameTime, lastRotation, time);
		}

		private void UpdateRotation()
		{
			KeyboardKey indexToConsider;

			indexToConsider = getPriorityIndex(); //Index from the PriorityVector

			if (indexToConsider == KeyboardKey.LEFT &&
				((Rotation >= 0.0f && Rotation <= MathHelper.PiOver2) ||
				(Rotation <= -3 * MathHelper.PiOver2 && Rotation >= -2 * MathHelper.Pi) ||
				(Rotation >= 3 * MathHelper.PiOver2 && Rotation <= 2 * MathHelper.Pi) ||
				(Rotation <= 0.0f && Rotation >= -MathHelper.PiOver2))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.LEFT &&
				  ((Rotation >= MathHelper.PiOver2 && Rotation <= 3 * MathHelper.PiOver2) ||
				  (Rotation <= -MathHelper.PiOver2 && Rotation >= -3 * MathHelper.Pi))) {
				_rotation = 0.1f;
			} else if (indexToConsider == KeyboardKey.RIGHT &&
				  ((Rotation >= 0.0f && Rotation <= MathHelper.PiOver2) ||
				  (Rotation <= -3 * MathHelper.PiOver2 && Rotation >= -2 * MathHelper.Pi) ||
				  (Rotation >= 3 * MathHelper.PiOver2 && Rotation <= 2 * MathHelper.Pi) ||
				  (Rotation <= 0.0f && Rotation >= -MathHelper.PiOver2))) {
				_rotation = 0.1f;
			} else if (indexToConsider == KeyboardKey.RIGHT &&
				  ((Rotation >= MathHelper.PiOver2 && Rotation <= 3 * MathHelper.PiOver2) ||
				  (Rotation <= -MathHelper.PiOver2 && Rotation >= -3 * MathHelper.Pi))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.UP &&
				  ((Rotation >= 0.0f && Rotation <= MathHelper.Pi) ||
				  (Rotation >= -2 * MathHelper.Pi && Rotation <= -MathHelper.Pi))) {
				_rotation = 0.1f;
			} else if (indexToConsider == KeyboardKey.UP &&
				  ((Rotation >= MathHelper.Pi && Rotation <= 2 * MathHelper.Pi) ||
				  (Rotation <= 0 && Rotation >= -MathHelper.Pi))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.DOWN &&
			  ((Rotation >= 0.0f && Rotation <= MathHelper.Pi) ||
			  (Rotation >= -2 * MathHelper.Pi && Rotation <= -MathHelper.Pi))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.DOWN &&
				  ((Rotation >= MathHelper.Pi && Rotation <= 2 * MathHelper.Pi) ||
				  (Rotation <= 0 && Rotation >= -MathHelper.Pi))) {
				_rotation = 0.1f;
			} else
				_rotation = 0.0f;
			//Console.WriteLine("RotationInput -> " + RotationInput + " Rotation -> " + _rotation);
		}

		private void UpdatePosition()
		{
			if (PositionInput.Y == 2 && _velocity.Y < _maxVelocity) {
				_velocity.Y += 0.6f;
			} else if (PositionInput.Y == 0 && _velocity.Y > 0) {
				_velocity.Y -= 0.3f;
			}

			if (PositionInput.Y == 1 && _velocity.Y > -_maxVelocity) {
				_velocity.Y -= 0.6f;
			} else if (PositionInput.Y == 0 && _velocity.Y < 0) {
				_velocity.Y += 0.3f;
			}

			if (PositionInput.X == 2 && _velocity.X > -_maxVelocity) {
				_velocity.X -= 0.6f;
			} else if (PositionInput.X == 0 && _velocity.X < 0) {
				_velocity.X += 0.3f;
			}

			if (PositionInput.X == 1 && _velocity.X < _maxVelocity) {
				_velocity.X += 0.6f;
			} else if (PositionInput.X == 0 && _velocity.X > 0) {
				_velocity.X -= 0.3f;
			}

			if (_velocity.X >= -0.3f && _velocity.X <= 0.3f)
				_velocity.X = 0;
			if (_velocity.Y >= -0.3f && _velocity.Y <= 0.3f)
				_velocity.Y = 0;

			//Console.WriteLine(_velocity);

		}

		private KeyboardKey getPriorityIndex()
		{
			float temp = 0;
			KeyboardKey indexTemp = KeyboardKey.NONE;

			if (temp < RotationInput.X) {
				temp = RotationInput.X;
				indexTemp = KeyboardKey.UP;
			}
			if (temp < RotationInput.Y) {
				temp = RotationInput.Y;
				indexTemp = KeyboardKey.DOWN;
			}
			if (temp < RotationInput.Z) {
				temp = RotationInput.Z;
				indexTemp = KeyboardKey.LEFT;
			}
			if (temp < RotationInput.W) {
				temp = RotationInput.W;
				indexTemp = KeyboardKey.RIGHT;
			}
			return indexTemp;
		}

		private void updateMeshWorld(GameTime gameTime)
		{
			Matrix oldWorld = world;
			world = Matrix.Identity;
			world *= Matrix.CreateRotationY(Rotation);
			//world *= oldWorld;
			position = Matrix.CreateTranslation(_positionVector.X, _positionVector.Y, _positionVector.Z);

			world = world * _scale * position;
		}

		private void updatePositionVector(GameTime gameTime, Vector2 normalizedVelocity, float lastRotation, float time)
		{
			_positionVector += new Vector3(time * _velocity.Y * normalizedVelocity.Y, 0, time * _velocity.X * normalizedVelocity.X);

			if (_positionVector != lastPositionVector || Rotation != lastRotation) {
				notify();
				lastPositionVector = _positionVector;
				lastTempRotation = tempRotation;
			}
		}

		private void updateBoundingSpheres(GameTime gameTime, float lastRotation, float time)
		{
			float upBodyY = upBody.Center.Y;
			float downBodyY = downBody.Center.Y;

			upBody.Center = _positionVector;
			upBody.Center.Y = upBodyY;

			downBody.Center = _positionVector;
			downBody.Center.Y = downBodyY;

			stick.Center = Vector3.Zero;
			stick.Center.X += 2f * ((float)Math.Sin(Rotation + MathHelper.PiOver2));
			stick.Center.Z += 2f * ((float)Math.Cos(Rotation + MathHelper.PiOver2));
			stick.Center += downBody.Center;
			stick.Center.Y = 1f;
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

			if (collidedWith.Count != 0 && (_positionOfCollision != _positionVector)) {
				foreach (ICollidable collided in collidedWith)
					VerifyDiskCollision(collided);
				_positionOfCollision = _positionVector;
				if (_rotation != 0)
					_rotationOfCollision = Rotation;
			}
		}

		private void VerifyDiskCollision(ICollidable collided)
		{
			int rotationStrength = 15;
			if (collided is Disk) {
				Disk disk = (Disk)collided;
				disk.AddVelocity(_velocity);
				List<BoundingSphere> diskSpheres = disk.getBoundingSpheres();
				if (_rotation != 0 && diskSpheres[0].Intersects(stick)) {
					Vector2 goVelocity = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
					if (_rotation > 0)
						goVelocity *= -1;
					disk.AddRotationVelocity(new Vector2(rotationStrength, rotationStrength) * goVelocity);
				}
			}

		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(stick, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Brown);
			BoundingSphereRender.Render(upBody, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Brown);
			BoundingSphereRender.Render(downBody, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Brown);
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
			if (_velocity != null)
				return _velocity;
			return Vector2.Zero;
		}

		public bool positionHasChandeg()
		{
			return lastPosition != _positionVector;
		}

		public void updateCameraPosition()
		{
			_camera.updateLocalPlayerPosition(_positionVector);
		}

		private bool isOutOfScreen()
		{
			Vector3 project = _game.GraphicsDevice.Viewport.Project(_positionVector, _camera.projection, _camera.view, Matrix.Identity);
			//Console.WriteLine(project);
			if (project.X < 0) {
				drawLeftTriangle(project);
				return true;
			}
			return false;
			/*	if(project.X > _game.GraphicsDevice.Viewport.Width)
					return true;
				if (project.Y < 0 || project.Y > _game.GraphicsDevice.Viewport.Height)
					return true;
				return false;*/
		}

		private void drawLeftTriangle(Vector3 project)
		{
			Rectangle src = new Rectangle(0, (int)project.Y, 30, 30);

			SpriteBatch spriteBatch = new SpriteBatch(_game.GraphicsDevice);
			
			spriteBatch.Begin(SpriteSortMode.FrontToBack, null);
			spriteBatch.Draw(_arrow, src, null, Color.White, -MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0);
			spriteBatch.End();

			_game.GraphicsDevice.BlendState = BlendState.Opaque;
			_game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		private void calculateLeftVertex(Vector3 project, out float m, out float b)
		{
			//float m;
			//float b;
			Vector3 diskScreenCoord = _game.GraphicsDevice.Viewport.Project(_disk.getPosition(), _camera.projection, _camera.view, Matrix.Identity);
			m = (diskScreenCoord.Y - project.Y) / (diskScreenCoord.Y + project.Y);
			b = project.Y - m * project.X;
			//return b;
		}
	}
}
