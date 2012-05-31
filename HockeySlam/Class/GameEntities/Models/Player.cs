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
using Microsoft.Xna.Framework.Net;

using HockeySlam.Class.GameEntities;
using HockeySlam;
using HockeySlam.Class.GameState;
using HockeySlam.Interface;
using HockeySlam.Class.Networking;


namespace HockeySlam.Class.GameEntities.Models
{
	public enum KeyboardKey
	{
		UP,
		DOWN,
		LEFT,
		RIGHT,
		NONE
	};

	public class Player : BaseModel, ICollidable, IDebugEntity, IReflectable
	{
		#region fields
		//Vector2 _velocity;
		float _maxVelocity;
		Matrix position;
		
		float _rotation;

		List<Boolean> arrowKeysPressed;
		List<Keys> lastArrowKeysPressed;

		BoundingSphere stick;
		Effect effect;
		BoundingSphere upBody;
		BoundingSphere downBody;
		GameManager _gameManager;
		ArrowManager _arrowManager;
		//Vector3 _positionVector;
		Texture2D _arrow;

		Matrix _scale;

		VertexPositionColor[] _verts;
		VertexBuffer _vertexBuffer;
		BasicEffect _basicEffect;
		Disk _disk;

		bool _deactivateKeyboard;

		Vector3 _lastBouncePosition;
		bool[] _keyDeactivated = new bool[4];
		bool _collidedWithCourt;
		int _team;

		/* -------------------------- Prediction --------------------------- */

		public struct PlayerState
		{
			public Vector2 Velocity;
			public Vector3 Position;
			public float Rotation;
			public Vector3 PositionOfCollision;
			public Vector3 LastPositionVector;
			public float LastRotation;
			public float RotationOfCollision;
		}

		public PlayerState simulationState;

		public PlayerState previousState;

		public PlayerState displayState;

		float _currentSmoothing;

		RollingAverage _clockDelta = new RollingAverage(100);

		/* -------------------------- AGENTS ----------------------------- */
		bool _isSinglePlayer;
		/* --------------------------------------------------------------- */

		//public float Rotation
		//{
		//        get;
		//        set;
		//}

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

		public Player(GameManager gameManager, Game game, Camera camera, int team, bool isSinglePlayer)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\player");
			_gameManager = gameManager;
			_isSinglePlayer = isSinglePlayer;
			_team = team;
		}

		public float getRotation()
		{
			return _rotation;
		}

		public Vector3 getPositionVector()
		{
			return displayState.Position;
		}

		public void setPositionVector(Vector3 vec)
		{
			displayState.Position = vec;
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

			if (_team == 2)
				_arrow = _game.Content.Load<Texture2D>("Textures/Arrow");
			else _arrow = _game.Content.Load<Texture2D>("Textures/ArrowBlue");

			base.LoadContent();
		}

		public override void Initialize()
		{
			// TODO: Add your initialization code here

			position = Matrix.Identity;

			Vector3 positionVector = Vector3.Zero;
			positionVector.Y = 0.7f;
			simulationState.Position = positionVector;
			simulationState.LastPositionVector = positionVector;

			simulationState.Velocity = Vector2.Zero;
			_maxVelocity = 12;

			simulationState.Rotation = 0;

			previousState = simulationState;
			displayState = simulationState;

			Matrix pos = Matrix.CreateTranslation(0, 0.7f, 0);
			_scale = Matrix.CreateScale(1.5f);
			world = world * _scale * pos;

			arrowKeysPressed = new List<Boolean>();
			for (int i = 0; i < 4; i++)
				arrowKeysPressed.Add(false);
			lastArrowKeysPressed = new List<Keys>();

			//lastPosition = new Vector3(0, 2, 0);
			stick = new BoundingSphere(new Vector3(2, 1f, 0), 0.5f);
			upBody = new BoundingSphere(new Vector3(0, 4.5f, 0), 1.2f);
			downBody = new BoundingSphere(new Vector3(0, 1.05f, -0.03f), 0.05f);

			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			DebugManager dm = (DebugManager)_gameManager.getGameEntity("debugManager");
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.register(this);
			cm.register(this);
			dm.registerDebugEntities(this);

			MultiplayerManager mm = (MultiplayerManager)_gameManager.getGameEntity("multiplayerManager");
			if (mm != null)
				_disk = mm.getDisk();
			else
				_disk = (Disk)_gameManager.getGameEntity("disk");
			_arrowManager = (ArrowManager)_gameManager.getGameEntity("arrowManager");

			_deactivateKeyboard = false;

			_lastBouncePosition = Vector3.Zero;
			_keyDeactivated[(int)KeyboardKey.UP] = false;
			_keyDeactivated[(int)KeyboardKey.DOWN] = false;
			_keyDeactivated[(int)KeyboardKey.LEFT] = false;
			_keyDeactivated[(int)KeyboardKey.RIGHT] = false;

			base.Initialize();
		}

		public override void Draw(GameTime gameTime)
		{
			if (!isOutOfScreen()) {
				_arrowManager.unregister(this);
			}
			Vector3 diffuseColor;

			if (_team == 1)
				diffuseColor = new Vector3(0.25f, 0.25f, 1);
			else
				diffuseColor = new Vector3(1, 0.25f, 0.25f);
			//diffuseColor[1] = new Vector3(0.25f, 1, 0.25f);
			//diffuseColor[2] = new Vector3(0.25f, 0.25f, 1);
			//diffuseColor[3] = new Vector3(0.5f, 0.5f, 0.5f);
			updateMeshWorld(gameTime);
			base.DrawEffect(effect, diffuseColor);
		}

		/// <summary>
		/// Updates a local player
		/// </summary>
		/// <param name="positionInput"></param>
		/// <param name="rotationInput"></param>
		public void UpdateLocal(Vector2 positionInput, Vector4 rotationInput, GameTime gameTime)
		{
			this.PositionInput = positionInput;
			this.RotationInput = rotationInput;

			UpdateState(ref simulationState, gameTime);

			displayState = simulationState;
		}

		public void UpdateRemoteOnServer(GameTime gameTime)
		{
			UpdateState(ref simulationState, gameTime);
			displayState = simulationState;
		}

		/// <summary>
		/// Applies prediction and smoothing to a remotely controlled player.
		/// </summary>
		public void UpdateRemote(int framesBetweenPackets, bool enablePrediction, GameTime gameTime)
		{
			float smoothingDecay = 1.0f / framesBetweenPackets;

			_currentSmoothing -= smoothingDecay;

			if (_currentSmoothing < 0)
				_currentSmoothing = 0;

			if (enablePrediction) {
				UpdateState(ref simulationState, gameTime);

				if (_currentSmoothing > 0)
					UpdateState(ref previousState, gameTime);
			}

			if (_currentSmoothing > 0)
				ApplySmoothing();
			else
				displayState = simulationState;
		}

		private void ApplySmoothing()
		{
			displayState.Position = Vector3.Lerp(simulationState.Position,
						 previousState.Position,
						 _currentSmoothing);

			displayState.Velocity = Vector2.Lerp(simulationState.Velocity,
							     previousState.Velocity,
							     _currentSmoothing);

			displayState.Rotation = MathHelper.Lerp(simulationState.Rotation,
								    previousState.Rotation,
								    _currentSmoothing);
		}

		public void ClientWriteNetworkPacket(PacketWriter packetWriter)
		{
			packetWriter.Write(PositionInput);
			packetWriter.Write(RotationInput);
		}

		public void ServerWriteNetworkPacket(PacketWriter packetWriter)
		{
			packetWriter.Write(simulationState.Position);
			packetWriter.Write(simulationState.Velocity);
			packetWriter.Write(simulationState.Rotation);

			packetWriter.Write(PositionInput);
			packetWriter.Write(RotationInput);
		}

		public void ReadInputFromClient(PacketReader packetReader, GameTime gameTime)
		{
			PositionInput = packetReader.ReadVector2();
			RotationInput = packetReader.ReadVector4();
		}

		public void ReadNetworkPacket(PacketReader packetReader, GameTime gameTime, TimeSpan latency,
					      bool enablePrediction, bool enableSmoothing, float packetSendTime)
		{
			if (enableSmoothing) {
				previousState = displayState;
				_currentSmoothing = 1;
			} else
				_currentSmoothing = 0;

			//float packetSendTime = packetReader.ReadSingle();

			simulationState.Position = packetReader.ReadVector3();
			simulationState.Velocity = packetReader.ReadVector2();
			simulationState.Rotation = packetReader.ReadSingle();

			PositionInput = packetReader.ReadVector2();
			RotationInput = packetReader.ReadVector4();

			if (enablePrediction)
				ApplyPrediction(gameTime, latency, packetSendTime);
		}

		private void ApplyPrediction(GameTime gameTime, TimeSpan latency, float packetSendTime)
		{

			float localTime = (float)gameTime.TotalGameTime.TotalSeconds;

			float timeDelta = localTime - packetSendTime;

			_clockDelta.AddValue(timeDelta);

			float timeDerivation = timeDelta - _clockDelta.AverageValue;

			latency += TimeSpan.FromSeconds(timeDerivation);

			TimeSpan oneFrame = TimeSpan.FromSeconds(1.0 / 60.0);

			while (latency >= oneFrame) {
				UpdateState(ref simulationState, gameTime);
				latency -= oneFrame;
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		public void UpdateState(ref PlayerState state, GameTime gameTime)
		{
			// TODO: Add your update code here
			_rotation = 0;
			//float lastRotation = Rotation;

			Vector2 normalizedVelocity = normalizeVelocity(state.Velocity);
			float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

			updatePositionVector(gameTime, normalizedVelocity, state.LastRotation, time, ref state);
			updateBoundings(gameTime, state.LastRotation, time);

			UpdatePosition(ref state);
			UpdateRotation(ref state);
			isOutOfScreen();

			//setRotation(_rotation);
			if (state.Position != state.LastPositionVector || state.Rotation != state.LastRotation) {
				notify(state);
				state.LastPositionVector = displayState.Position;
				//lastTempRotation = tempRotation;
			}

			if (!_collidedWithCourt) {
				activateKeyboard();
			}
		}

		private void UpdateRotation(ref PlayerState state)
		{
			KeyboardKey indexToConsider;

			indexToConsider = getPriorityIndex(); //Index from the PriorityVector

			if (indexToConsider == KeyboardKey.LEFT &&
				((state.Rotation >= 0.0f && state.Rotation <= MathHelper.PiOver2) ||
				(state.Rotation <= -3 * MathHelper.PiOver2 && state.Rotation >= -2 * MathHelper.Pi) ||
				(state.Rotation >= 3 * MathHelper.PiOver2 && state.Rotation <= 2 * MathHelper.Pi) ||
				(state.Rotation <= 0.0f && state.Rotation >= -MathHelper.PiOver2))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.LEFT &&
				  ((state.Rotation >= MathHelper.PiOver2 && state.Rotation <= 3 * MathHelper.PiOver2) ||
				  (state.Rotation <= -MathHelper.PiOver2 && state.Rotation >= -3 * MathHelper.Pi))) {
				_rotation = 0.1f;
			} else if (indexToConsider == KeyboardKey.RIGHT &&
				  ((state.Rotation >= 0.0f && state.Rotation <= MathHelper.PiOver2) ||
				  (state.Rotation <= -3 * MathHelper.PiOver2 && state.Rotation >= -2 * MathHelper.Pi) ||
				  (state.Rotation >= 3 * MathHelper.PiOver2 && state.Rotation <= 2 * MathHelper.Pi) ||
				  (state.Rotation <= 0.0f && state.Rotation >= -MathHelper.PiOver2))) {
				_rotation = 0.1f;
			} else if (indexToConsider == KeyboardKey.RIGHT &&
				  ((state.Rotation >= MathHelper.PiOver2 && state.Rotation <= 3 * MathHelper.PiOver2) ||
				  (state.Rotation <= -MathHelper.PiOver2 && state.Rotation >= -3 * MathHelper.Pi))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.UP &&
				  ((state.Rotation >= 0.0f && state.Rotation <= MathHelper.Pi) ||
				  (state.Rotation >= -2 * MathHelper.Pi && state.Rotation <= -MathHelper.Pi))) {
				_rotation = 0.1f;
			} else if (indexToConsider == KeyboardKey.UP &&
				  ((state.Rotation >= MathHelper.Pi && state.Rotation <= 2 * MathHelper.Pi) ||
				  (state.Rotation <= 0 && state.Rotation >= -MathHelper.Pi))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.DOWN &&
			  ((state.Rotation >= 0.0f && state.Rotation <= MathHelper.Pi) ||
			  (state.Rotation >= -2 * MathHelper.Pi && state.Rotation <= -MathHelper.Pi))) {
				_rotation = -0.1f;
			} else if (indexToConsider == KeyboardKey.DOWN &&
				  ((state.Rotation >= MathHelper.Pi && state.Rotation <= 2 * MathHelper.Pi) ||
				  (state.Rotation <= 0 && state.Rotation >= -MathHelper.Pi))) {
				_rotation = 0.1f;
			} else
				_rotation = 0.0f;
			//Console.WriteLine("RotationInput -> " + RotationInput + " Rotation -> " + _rotation);

			setRotation(ref state, _rotation);
		}

		private void UpdatePosition(ref PlayerState state)
		{
			if (_keyDeactivated[(int)KeyboardKey.DOWN] && PositionInput.Y == 2)
				state.Velocity.Y = 0;
			else if (PositionInput.Y == 2 && state.Velocity.Y < _maxVelocity) {
				state.Velocity.Y += 0.6f;
			} else if (PositionInput.Y == 0 && state.Velocity.Y > 0) {
				state.Velocity.Y -= 0.3f;
			}

			if (_keyDeactivated[(int)KeyboardKey.UP] && PositionInput.Y == 1)
				state.Velocity.Y = 0;
			else if (PositionInput.Y == 1 && state.Velocity.Y > -_maxVelocity) {
				state.Velocity.Y -= 0.6f;
			} else if (PositionInput.Y == 0 && state.Velocity.Y < 0) {
				state.Velocity.Y += 0.3f;
			}

			if (_keyDeactivated[(int)KeyboardKey.RIGHT] && PositionInput.X == 2)
				state.Velocity.X = 0;
			else if (PositionInput.X == 2 && state.Velocity.X > -_maxVelocity) {
				state.Velocity.X -= 0.6f;
			} else if (PositionInput.X == 0 && state.Velocity.X < 0) {
				state.Velocity.X += 0.3f;
			}

			if (_keyDeactivated[(int)KeyboardKey.LEFT] && PositionInput.X == 1)
				state.Velocity.X = 0;
			else if (PositionInput.X == 1 && state.Velocity.X < _maxVelocity) {
				state.Velocity.X += 0.6f;
			} else if (PositionInput.X == 0 && state.Velocity.X > 0) {
				state.Velocity.X -= 0.3f;
			}

			if (state.Velocity.X >= -0.3f && state.Velocity.X <= 0.3f)
				state.Velocity.X = 0;
			if (state.Velocity.Y >= -0.3f && state.Velocity.Y <= 0.3f)
				state.Velocity.Y = 0;
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
			world *= Matrix.CreateRotationY(displayState.Rotation);
			//world *= oldWorld;
			position = Matrix.CreateTranslation(displayState.Position.X, displayState.Position.Y, displayState.Position.Z);
			//System.Console.WriteLine(simulationState.Rotation);
			world = world * _scale * position;
		}

		private void updatePositionVector(GameTime gameTime, Vector2 normalizedVelocity, float lastRotation, float time, ref PlayerState state)
		{
			state.Position += new Vector3(time * state.Velocity.Y * normalizedVelocity.Y, 0, time * state.Velocity.X * normalizedVelocity.X);
		}

		private void updateBoundings(GameTime gameTime, float lastRotation, float time)
		{
			float upBodyY = upBody.Center.Y;
			float downBodyY = downBody.Center.Y;

			upBody.Center = displayState.Position;
			upBody.Center.Y = upBodyY;

			downBody.Center = displayState.Position;
			downBody.Center.Y = downBodyY;

			stick.Center = Vector3.Zero;
			stick.Center.X += 2f * ((float)Math.Sin(displayState.Rotation + MathHelper.PiOver2));
			stick.Center.Z += 2f * ((float)Math.Cos(displayState.Rotation + MathHelper.PiOver2));
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

		public Boolean collisionOccured(ICollidable collideObject)
		{
			List<BoundingSphere> bss = collideObject.getBoundingSpheres();

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

		public void notify(PlayerState state)
		{
			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			List<ICollidable> collidedWith = cm.verifyCollision(this);

			_collidedWithCourt = false;

			if (collidedWith.Count != 0 && (state.PositionOfCollision != state.Position)) {
				foreach (ICollidable collided in collidedWith) {
					verifyDiskCollision(collided, state);
					verifyCourtCollision(collided);
				}

				state.PositionOfCollision = state.Position;
				if (_rotation != 0)
					state.RotationOfCollision = state.Rotation;
			}
		}

		private void verifyCourtCollision(ICollidable collided)
		{
			if (collided is Court)
				_collidedWithCourt = true;
		}

		private void activateKeyboard()
		{
			_keyDeactivated[(int)KeyboardKey.UP] = false;
			_keyDeactivated[(int)KeyboardKey.DOWN] = false;
			_keyDeactivated[(int)KeyboardKey.LEFT] = false;
			_keyDeactivated[(int)KeyboardKey.RIGHT] = false;
		}

		private void verifyDiskCollision(ICollidable collided, PlayerState state)
		{
			int rotationStrength = 15;
			if (collided is Disk && !_isSinglePlayer) {
				Disk disk = (Disk)collided;
				disk.AddVelocity(state.Velocity);
				List<BoundingSphere> diskSpheres = disk.getBoundingSpheres();
				if (_rotation != 0 && diskSpheres[0].Intersects(stick)) {
					Vector2 goVelocity = new Vector2((float)Math.Cos(state.Rotation), (float)Math.Sin(state.Rotation));
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
			displayState.Velocity = velocity;
		}

		public Vector2 getVelocity()
		{
			if (displayState.Velocity != null)
				return displayState.Velocity;
			return Vector2.Zero;
		}

		public bool positionHasChandeg()
		{
			return displayState.LastPositionVector != displayState.Position;
		}

		public void updateCameraPosition()
		{
			_camera.updateLocalPlayerPosition(displayState.Position);
		}

		public void setArrowPlayer()
		{
			Vector3 project = _game.GraphicsDevice.Viewport.Project(new Vector3(displayState.Position.X, displayState.Position.Y + 4.3f, displayState.Position.Z), _camera.projection, _camera.view, Matrix.Identity);
			_arrowManager.setLocalPlayer(new Vector2(project.X, project.Y), _team);
		}

		private bool isOutOfScreen()
		{
			Vector3 project = _game.GraphicsDevice.Viewport.Project(displayState.Position, _camera.projection, _camera.view, Matrix.Identity);
			if (project.X < 0) {
				_arrowManager.updatePosition(this, new Vector2(0, project.Y), MathHelper.PiOver2);
				return true;
			}
			if (project.X > _game.GraphicsDevice.Viewport.Width) {
				_arrowManager.updatePosition(this, new Vector2(_game.GraphicsDevice.Viewport.Width, project.Y), -MathHelper.PiOver2);
				return true;
			}
			if (project.Y < 0) {
				_arrowManager.updatePosition(this, new Vector2(project.X, 0), 0);
				return true;
			}
			if (project.Y > _game.GraphicsDevice.Viewport.Height) {
				_arrowManager.updatePosition(this, new Vector2(project.X, _game.GraphicsDevice.Viewport.Height), MathHelper.Pi);
				return true;
			}
			return false;
		}

		public List<BoundingBox> getBoundingBoxes()
		{
			return null;
		}

		public float getMaxVelocity()
		{
			return _maxVelocity;
		}

		public void bounce(Vector2 newVelocity)
		{
		}

		public void updatePositionInput(Vector2 positionInput)
		{
			if (!_deactivateKeyboard) // to prevent going through walls
				PositionInput = positionInput;
		}

		public void updateRotationInput(Vector4 rotationInput)
		{
			RotationInput = rotationInput;
		}

		public void deactivateKeys(bool[] keysDeactivated)
		{
			_keyDeactivated = keysDeactivated;
		}

		public void setRotation(ref PlayerState state, float rotation)
		{
			state.LastRotation = state.Rotation;
			state.Rotation = (state.Rotation + rotation) % MathHelper.TwoPi;
		}

		public int getTeam()
		{
			return _team;
		}

		/* ---------------------------- AGENTS ------------------------------- */

		public Vector3 getStickPosition()
		{
			return stick.Center;
		}
	}
}
