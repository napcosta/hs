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
using HockeySlam.GameEntities;
using HockeySlam;
using HockeySlam.GameState;


namespace HockeySlam.GameEntities.Models
{
	class Player : BaseModel, ICollidable, IDebugEntity
	{
		#region fields
		Vector2 velocity;
		Matrix position;
		Vector3 positionVector;
		Vector3 lastPositionVector;
		float tempRotation;
		float lastTempRotation;
		List<Boolean> arrowKeysPressed;
		List<Keys> lastArrowKeysPressed;
		Vector3 lastPosition;
		Game game;
		Camera camera;
		BoundingSphere stick;
		Effect effect;
		BoundingSphere upBody;
		BoundingSphere downBody;
		GameManager gameManager;
		#endregion

		public Player(GameManager gameManager, Game game, Camera camera) : base(game, camera)
		{
			model = game.Content.Load<Model>(@"Models\player");
			this.game = game;
			this.camera = camera;
			this.gameManager = gameManager;
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
			positionVector = Vector3.Zero;
			positionVector.Z = -1.5f;
			lastPositionVector = positionVector;

			velocity = Vector2.Zero;

			Matrix pos = Matrix.CreateTranslation(0, 1f, 0);
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

			CollisionManager cm = (CollisionManager)gameManager.getGameEntity("collisionManager");
			DebugManager dm = (DebugManager)gameManager.getGameEntity("debugManager");
			cm.registre(this);
			dm.registreDebugEntities(this);
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
			float rotation = 0;
#if WINDOWS
			KeyboardState currentKeyboardState = Keyboard.GetState();
			float lastTempRotation = tempRotation;

			#region Position
			if (currentKeyboardState.IsKeyDown(Keys.S) && velocity.Y < 30) {
				velocity.Y += 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.S) && velocity.Y > 0) {
				velocity.Y -= (float)0.5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.W) && velocity.Y > -30) {
				velocity.Y -= 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.W) && velocity.Y < 0) {
				velocity.Y += (float)0.5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.D) && velocity.X > -30) {
				velocity.X -= 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.D) && velocity.X < 0) {
				velocity.X += (float)0.5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.A) && velocity.X < 30) {
				velocity.X += 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.A) && velocity.X > 0) {
				velocity.X -= (float)0.5;
			}
			#endregion
			#region Rotation

			Keys keyToConsider;

			if (currentKeyboardState.IsKeyDown(Keys.Left) && !arrowKeysPressed[0])
			{
				arrowKeysPressed[0] = true;
				lastArrowKeysPressed.Add(Keys.Left);
			}
			if (currentKeyboardState.IsKeyDown(Keys.Right) && !arrowKeysPressed[1])
			{
				arrowKeysPressed[1] = true;
				lastArrowKeysPressed.Add(Keys.Right);
			}
			if (currentKeyboardState.IsKeyDown(Keys.Up) && !arrowKeysPressed[2])
			{
				arrowKeysPressed[2] = true;
				lastArrowKeysPressed.Add(Keys.Up);
			}
			if (currentKeyboardState.IsKeyDown(Keys.Down) && !arrowKeysPressed[3])
			{
				arrowKeysPressed[3] = true;
				lastArrowKeysPressed.Add(Keys.Down);
			}

			if (currentKeyboardState.IsKeyUp(Keys.Left) && arrowKeysPressed[0])
			{
				arrowKeysPressed[0] = false;
				lastArrowKeysPressed.Remove(Keys.Left);
			}
			if (currentKeyboardState.IsKeyUp(Keys.Right) && arrowKeysPressed[1])
			{
				arrowKeysPressed[1] = false;
				lastArrowKeysPressed.Remove(Keys.Right);
			}
			if (currentKeyboardState.IsKeyUp(Keys.Up) && arrowKeysPressed[2])
			{
				arrowKeysPressed[2] = false;
				lastArrowKeysPressed.Remove(Keys.Up);
			}
			if (currentKeyboardState.IsKeyUp(Keys.Down) && arrowKeysPressed[3])
			{
				arrowKeysPressed[3] = false;
				lastArrowKeysPressed.Remove(Keys.Down);
			}

			if (lastArrowKeysPressed.Count != 0)
			{
				keyToConsider = lastArrowKeysPressed[lastArrowKeysPressed.Count - 1];

				if (keyToConsider == Keys.Left &&
					((tempRotation >= 0.0f && tempRotation <= MathHelper.PiOver2) ||
					(tempRotation <= -3 * MathHelper.PiOver2 && tempRotation >= -2 * MathHelper.Pi) ||
					(tempRotation >= 3 * MathHelper.PiOver2 && tempRotation <= 2 * MathHelper.Pi) ||
					(tempRotation <= 0.0f && tempRotation >= -MathHelper.PiOver2)))
				{
					rotation = -0.1f;
				}
				else if (keyToConsider == Keys.Left &&
					((tempRotation >= MathHelper.PiOver2 && tempRotation <= 3 * MathHelper.PiOver2) ||
					(tempRotation <= -MathHelper.PiOver2 && tempRotation >= -3 * MathHelper.Pi)))
				{
					rotation = 0.1f;
				}
				else if (keyToConsider == Keys.Right &&
					((tempRotation >= 0.0f && tempRotation <= MathHelper.PiOver2) ||
					(tempRotation <= -3 * MathHelper.PiOver2 && tempRotation >= -2 * MathHelper.Pi) ||
					(tempRotation >= 3 * MathHelper.PiOver2 && tempRotation <= 2 * MathHelper.Pi) ||
					(tempRotation <= 0.0f && tempRotation >= -MathHelper.PiOver2)))
				{
					rotation = 0.1f;
				}
				else if (keyToConsider == Keys.Right &&
					((tempRotation >= MathHelper.PiOver2 && tempRotation <= 3 * MathHelper.PiOver2) ||
					(tempRotation <= -MathHelper.PiOver2 && tempRotation >= -3 * MathHelper.Pi)))
				{
					rotation = -0.1f;
				}
				else if (keyToConsider == Keys.Up &&
					((tempRotation >= 0.0f && tempRotation <= MathHelper.Pi) ||
					(tempRotation >= -2 * MathHelper.Pi && tempRotation <= -MathHelper.Pi)))
				{
					rotation = 0.1f;
				}
				else if (keyToConsider == Keys.Up &&
					((tempRotation >= MathHelper.Pi && tempRotation <= 2 * MathHelper.Pi) ||
					(tempRotation <= 0 && tempRotation >= -MathHelper.Pi)))
				{
					rotation = -0.1f;
				}
				else if (keyToConsider == Keys.Down &&
				((tempRotation >= 0.0f && tempRotation <= MathHelper.Pi) ||
				(tempRotation >= -2 * MathHelper.Pi && tempRotation <= -MathHelper.Pi)))
				{
					rotation = -0.1f;
				}
				else if (keyToConsider == Keys.Down &&
					((tempRotation >= MathHelper.Pi && tempRotation <= 2 * MathHelper.Pi) ||
					(tempRotation <= 0 && tempRotation >= -MathHelper.Pi)))
				{
					rotation = 0.1f;
				}
				else rotation = 0.0f;

			}
			else rotation = 0;
			#endregion

#else
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            Vector2 leftThumStick = currentGamePadState.ThumbSticks.Left;

            Vector2 maxVelocity;
            maxVelocity.X = 100 * Math.Abs(leftThumStick.Y);
            maxVelocity.Y = 100 * Math.Abs(leftThumStick.X);

            if (leftThumStick.X != 0 && velocity.Y > -maxVelocity.Y && velocity.Y < maxVelocity.Y)
            {
                velocity.Y += leftThumStick.X;
            }
			else if (velocity.Y < -(float)0.5)
            {
                velocity.Y += (float)0.5;
            }
			else if (velocity.Y > (float)0.5)
            {
				velocity.Y -= (float)0.5;
            }
            else velocity.Y = 0;

            if (leftThumStick.Y != 0 && velocity.X > -maxVelocity.X && velocity.X < maxVelocity.X)
            {
                velocity.X += leftThumStick.Y;
            }
			else if (velocity.X < -(float)0.5)
            {
				velocity.X += (float)0.5;
            }
			else if (velocity.X > (float)0.5)
            {
				velocity.X -= (float)0.5;
            }
            else velocity.X = 0;
#endif
			Vector2 normalizedVelocity = normalizeVelocity(velocity);
			float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

			updateMeshWorld(gameTime, rotation, normalizedVelocity, time);
			updatePositionVector(gameTime, normalizedVelocity, time);
			updateBoundingSpheres(gameTime, lastTempRotation, normalizedVelocity, time);
		}

		private void updateMeshWorld(GameTime gameTime, float rotation, Vector2 normalizedVelocity, float time) 
		{
			tempRotation = (tempRotation + rotation) % MathHelper.TwoPi;
			Matrix oldWorld = world;
			world = Matrix.Identity;
			world *= Matrix.CreateRotationY(rotation);
			world *= oldWorld;
			position = Matrix.CreateTranslation(time * velocity.Y*normalizedVelocity.Y, 0, time * velocity.X*normalizedVelocity.X);

			world = world * position;
		}
		private float abs(float num)
		{
			if (num > 0)
				return num;
			else if (num < 0)
				return -num;
			else
				return 0;
		}

		private Vector2 normalizeVelocity(Vector2 velocity)
		{
			//Console.WriteLine(velocity.X + " <-> " + velocity.Y);
			if (velocity.X != 0 && velocity.Y != 0) {
				double degree = Math.Atan2(abs(velocity.Y), abs(velocity.X));
				velocity.X = (float)Math.Cos(degree);
				velocity.Y = (float)Math.Sin(degree);
				return velocity;
			}
			Vector2 vec = new Vector2(1, 1);
			return vec;
		}

		private void updatePositionVector(GameTime gameTime, Vector2 normalizedVelocity, float time)
		{
			positionVector += new Vector3(time * velocity.Y * normalizedVelocity.Y, 0, time * velocity.X * normalizedVelocity.X);

			if (positionVector != lastPositionVector || tempRotation != lastTempRotation) {
				notify();
				lastPositionVector = positionVector;
				lastTempRotation = tempRotation;
			}
		}

		private void updateBoundingSpheres(GameTime gameTime, float lastTempRotation, Vector2 normalizedVelocity, float time)
		{	
			lastPosition = new Vector3(time * velocity.Y * normalizedVelocity.Y, 0, time * velocity.X * normalizedVelocity.X);

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
			CollisionManager cm = (CollisionManager)gameManager.getGameEntity("collisionManager");
			if (cm.verifyCollision(this).Count != 0)
				Console.WriteLine("CollisionOcurred");
			else Console.WriteLine("NOT-CollisionOcurred");
		}

		public void DrawDebug()
		{
			BoundingSphereRender.Render(stick, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
			BoundingSphereRender.Render(upBody, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
			BoundingSphereRender.Render(downBody, game.GraphicsDevice, camera.view, camera.projection, Color.Brown);
		}
	}
}
