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


namespace HockeySlam.Class.GameEntities.Models
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	class Player : BaseModel
	{
		Vector2 velocity;
        Matrix position = Matrix.Identity;
		float tempRotation = 0.0f;
		List<Boolean> arrowKeysPressed;
		List<Keys> lastArrowKeysPressed;


		public Player(Game game, Camera camera) : base(game, camera)
		{
			model = game.Content.Load<Model>(@"Models\player");
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run. This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here
			velocity = Vector2.Zero;

			Matrix pos = Matrix.CreateTranslation(0, 0, -2f);
			Matrix scale = Matrix.CreateScale(1.5f);
			world = world * scale * pos;

			arrowKeysPressed = new List<Boolean>();
			for(int i = 0; i < 4; i++)
				arrowKeysPressed.Add(false);
			lastArrowKeysPressed = new List<Keys>();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			// TODO: Add your update code here
			float rotation = 0;

#if WINDOWS
			KeyboardState currentKeyboardState = Keyboard.GetState();

			#region Position
			if (currentKeyboardState.IsKeyDown(Keys.S) && velocity.X > -30) {
				velocity.X -= 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.S) && velocity.X < 0) {
				velocity.X += (float)0.5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.W) && velocity.X < 30) {
				velocity.X += 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.W) && velocity.X > 0) {
				velocity.X -= (float)0.5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.D) && velocity.Y < 30) {
				velocity.Y += 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.D) && velocity.Y > 0) {
				velocity.Y -= (float)0.5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.A) && velocity.Y > -30) {
				velocity.Y -= 1;
			} else if (currentKeyboardState.IsKeyUp(Keys.A) && velocity.Y < 0) {
				velocity.Y += (float)0.5;
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
					rotation = 0.1f;
				}
				else if (keyToConsider == Keys.Left &&
					((tempRotation >= MathHelper.PiOver2 && tempRotation <= 3 * MathHelper.PiOver2) ||
					(tempRotation <= -MathHelper.PiOver2 && tempRotation >= -3 * MathHelper.Pi)))
				{
					rotation = -0.1f;
				}
				else if (keyToConsider == Keys.Right &&
					((tempRotation >= 0.0f && tempRotation <= MathHelper.PiOver2) ||
					(tempRotation <= -3 * MathHelper.PiOver2 && tempRotation >= -2 * MathHelper.Pi) ||
					(tempRotation >= 3 * MathHelper.PiOver2 && tempRotation <= 2 * MathHelper.Pi) ||
					(tempRotation <= 0.0f && tempRotation >= -MathHelper.PiOver2)))
				{
					rotation = -0.1f;
				}
				else if (keyToConsider == Keys.Right &&
					((tempRotation >= MathHelper.PiOver2 && tempRotation <= 3 * MathHelper.PiOver2) ||
					(tempRotation <= -MathHelper.PiOver2 && tempRotation >= -3 * MathHelper.Pi)))
				{
					rotation = 0.1f;
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

			/*position = new Vector2(position.X + (float)gameTime.ElapsedGameTime.TotalSeconds * velocity.X,
			    position.Y + (float)gameTime.ElapsedGameTime.TotalSeconds * velocity.Y);*/

			tempRotation = (tempRotation + rotation) % MathHelper.TwoPi;
			Matrix oldWorld = world;

			world = Matrix.Identity;
			world *= Matrix.CreateRotationZ(rotation);
			world *= oldWorld;

            position = Matrix.CreateTranslation((float)gameTime.ElapsedGameTime.TotalSeconds * velocity.X, 
                (float)gameTime.ElapsedGameTime.TotalSeconds * velocity.Y,
                0);
            world = world * position;
		}
	}
}
