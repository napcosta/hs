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
using Microsoft.Xna.Framework.Net;
using HockeySlam.Class.Networking;

namespace HockeySlam.Class.GameEntities.Models
{
	public class Disk : BaseModel, IGameEntity, ICollidable, IDebugEntity, IReflectable
	{
		
		//Vector2 _velocity;
		int _maxVelocity;
		//Vector3 _position;
		

		Matrix _rotation;
		Matrix _scale;
		float drag = 0.1f;
		float moreDrag = 0.3f;

		GameManager _gameManager;

		float _currentSmoothing;

		RollingAverage _clockDelta = new RollingAverage(100);

		public struct DiskState
		{
			public Vector3 Position;
			public Vector2 Velocity;
			public bool IsColliding;
			public BoundingSphere CollisionArea;
		}

		// This is the latest master copy of the Disk state, used by our local
		// physics computations and prediction. This state will jerk whenever
		// a new packet is received.
		DiskState simulationState;

		// This is a copy of the state from immediately before the last
		// network packet was received
		DiskState previousState;

		// This is the player state that is drawn onto the screen. It is gradually
		// interpolated from the _previousState toward the _simulationState, in
		// order to smooth out any sudden jumps caused by discontinuities when
		// a network packet suddenly modifies the _simulationState
		DiskState displayState;

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
			simulationState.Velocity = Vector2.Zero;
			_maxVelocity = 12;
			simulationState.Position = new Vector3(4, 1, 0);
			simulationState.IsColliding = false;

			Matrix pos = Matrix.CreateTranslation(4, 1, 0);
			_rotation = Matrix.CreateRotationX(MathHelper.PiOver2);
			_scale = Matrix.CreateScale(0.5f);

			world = world * _rotation * _scale * pos;

			simulationState.CollisionArea = new BoundingSphere(new Vector3(4,1,0), 0.45f);

			previousState = simulationState;
			displayState = simulationState;

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

			bs.Add(displayState.CollisionArea);

			return bs;
		}

		public void notify()
		{
			CollisionManager cm = (CollisionManager)_gameManager.getGameEntity("collisionManager");
			List<ICollidable> collided = cm.verifyCollision(this);
		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(displayState.CollisionArea, _game.GraphicsDevice, _camera.view, _camera.projection, Color.Brown);
		}

		public void AddVelocity(Vector2 velocity)
		{
			simulationState.Velocity += velocity;

			if (simulationState.Velocity.X > _maxVelocity)
				simulationState.Velocity.X = _maxVelocity;
			else if (simulationState.Velocity.X < -_maxVelocity)
				simulationState.Velocity.X = -_maxVelocity;

			if (simulationState.Velocity.Y > _maxVelocity)
				simulationState.Velocity.Y = _maxVelocity;
			else if (simulationState.Velocity.Y < -_maxVelocity)
				simulationState.Velocity.Y = -_maxVelocity;

			simulationState.IsColliding = true;
		}

		public void AddRotationVelocity(Vector2 velocity)
		{
			simulationState.Velocity += velocity;
		}

		/// <summary>
		/// Updates a local disk
		/// </summary>
		/// <param name="positionInput"></param>
		/// <param name="rotationInput"></param>
		public void UpdateLocal(GameTime gameTime)
		{
			UpdateState(ref simulationState, gameTime);

			displayState = simulationState;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		void UpdateState(ref DiskState state, GameTime gameTime)
		{
			if (!_isSinglePlayer || _playerWithDisk == null) {


				if (!state.IsColliding) {
					UpdateVelocityX(ref state, drag, moreDrag);
					UpdateVelocityY(ref state, drag, moreDrag);
				}

				state.IsColliding = false;

				Vector2 normalizedVelocity = normalizeVelocity(state.Velocity);
				float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

				state.Position.X += time * state.Velocity.Y * normalizedVelocity.Y;
				state.Position.Z += time * state.Velocity.X * normalizedVelocity.X;

				state.CollisionArea.Center.X = state.Position.X;
				state.CollisionArea.Center.Z = state.Position.Z;
				notify();
			} else {
				state.Position = _playerWithDisk.getPlayer().getStickPosition();
				state.CollisionArea.Center = _playerWithDisk.getPlayer().getStickPosition();
				state.Velocity = Vector2.Zero;
			}

		}

		public void ServerWriteNetworkPacket(PacketWriter packetWriter)
		{
			packetWriter.Write(simulationState.Position);
			packetWriter.Write(simulationState.Velocity);
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
		}

		private void UpdateVelocityY(ref DiskState state, float drag, float moreDrag)
		{
			if (Math.Abs(state.Velocity.Y) > _maxVelocity) {
				if (state.Velocity.Y >= moreDrag)
					state.Velocity.Y -= moreDrag;
				else if (state.Velocity.Y <= -moreDrag)
					state.Velocity.Y += moreDrag;
				else state.Velocity.Y = 0;
			} else {
				if (state.Velocity.Y >= drag)
					state.Velocity.Y -= drag;
				else if (state.Velocity.Y <= -drag)
					state.Velocity.Y += drag;
				else state.Velocity.Y = 0;
			}
		}

		private void UpdateVelocityX(ref DiskState state, float drag, float moreDrag)
		{
			if (Math.Abs(state.Velocity.X) <= _maxVelocity) {
				if (state.Velocity.X >= drag)
					state.Velocity.X -= drag;
				else if (state.Velocity.X <= -drag)
					state.Velocity.X += drag;
				else state.Velocity.X = 0;
			} else {
				if (state.Velocity.X >= moreDrag)
					state.Velocity.X -= moreDrag;
				else if (state.Velocity.X <= -moreDrag)
					state.Velocity.X += moreDrag;
				else state.Velocity.X = 0;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			Matrix pos = Matrix.CreateTranslation(displayState.Position.X, displayState.Position.Y, displayState.Position.Z);

			world = Matrix.Identity;
			world *= _rotation * _scale * pos;

			_camera.updateDiskPosition(displayState.Position);

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
			return displayState.Position;
		}

		public List<BoundingBox> getBoundingBoxes()
		{
			return null;
		}

		public Boolean collisionOccured(ICollidable collideObject)
		{
			List<BoundingSphere> bss = collideObject.getBoundingSpheres();

			foreach (BoundingSphere bs in bss) {
				if (bs.Intersects(displayState.CollisionArea))
					return true;
			}
			return false;
		}

		public void bounce(Vector2 newVelocity)
		{
			displayState.Velocity = newVelocity;
		}

		public Vector2 getVelocity()
		{
			return displayState.Velocity;
		}

		public void setPosition(Vector3 position)
		{
			displayState.Position = position;
		}

		/* -------------------------- AGENTS --------------------------- */

		public void newPlayerWithDisk(ReactiveAgent player) {
			player.removePlayerDisk();
			_playerWithDisk = player;
		}

		// Direction coordinates must be beetween 0 and 1
		public void shoot(Vector2 direction)
		{
			displayState.Velocity = direction*_maxVelocity*5;
			_playerWithDisk = null;
		}

		public ReactiveAgent getPlayerWithDisk()
		{
			return _playerWithDisk;
		}
	}
}
