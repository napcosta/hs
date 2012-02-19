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


namespace HockeySlam
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	class Player : BaseModel
	{

		Texture2D texture;
		Vector2 velocity;
        Matrix position = Matrix.Identity;

		public Player(Model model)
			: base(model)
		{
			// TODO: Construct any child components here
            velocity = Vector2.Zero;

            Matrix rot = Matrix.CreateRotationX(-MathHelper.Pi / 2);
            Matrix pos = Matrix.CreateTranslation(0, 20, 0);
            world = world *rot* pos;
		}

		/*public override void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
			spriteBatch.Begin();
			Rectangle p = new Rectangle((int)position.X, (int)position.Y, 50, 50);
			spriteBatch.Draw(texture, p, Color.White);
			//spriteBatch.Draw(texture, position, p, Color.White);
			spriteBatch.End();
			base.Draw(gameTime);
		}
         * 
		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run. This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here
			position = Vector2.Zero;
			velocity = Vector2.Zero;
			base.Initialize();
		}
*/
		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here

#if WINDOWS
			KeyboardState currentKeyboardState = Keyboard.GetState();

			if (currentKeyboardState.IsKeyDown(Keys.Down) && velocity.X < 200) {
				velocity.X += 10;
			} else if (currentKeyboardState.IsKeyUp(Keys.Down) && velocity.X > 0) {
				velocity.X -= 5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.Up) && velocity.X > -200) {
				velocity.X -= 10;
			} else if (currentKeyboardState.IsKeyUp(Keys.Up) && velocity.X < 0) {
				velocity.X += 5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.Right) && velocity.Y > -200) {
				velocity.Y -= 10;
			} else if (currentKeyboardState.IsKeyUp(Keys.Right) && velocity.Y < 0) {
				velocity.Y += 5;
			}

			if (currentKeyboardState.IsKeyDown(Keys.Left) && velocity.Y < 200) {
				velocity.Y += 10;
			} else if (currentKeyboardState.IsKeyUp(Keys.Left) && velocity.Y > 0) {
				velocity.Y -= 5;
			}

#else
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            Vector2 leftThumStick = currentGamePadState.ThumbSticks.Left;

            Vector2 maxVelocity;
            maxVelocity.X = 200 * Math.Abs(leftThumStick.Y);
            maxVelocity.Y = 200 * Math.Abs(leftThumStick.X);

            if (leftThumStick.X != 0 && velocity.Y > -maxVelocity.Y && velocity.Y < maxVelocity.Y)
            {
                velocity.Y -= leftThumStick.X * 10;
            }
            else if (velocity.Y < 0)
            {
                velocity.Y += 5;
            }
            else if (velocity.Y > 0)
            {
                velocity.Y -= 5;
            }

            if (leftThumStick.Y != 0 && velocity.X > -maxVelocity.X && velocity.X < maxVelocity.X)
            {
                velocity.X -= leftThumStick.Y * 10;
            }
            else if (velocity.X < 0)
            {
                velocity.X += 5;
            }
            else if (velocity.X > 0)
            {
                velocity.X -= 5;
            }
#endif

			/*position = new Vector2(position.X + (float)gameTime.ElapsedGameTime.TotalSeconds * velocity.X,
			    position.Y + (float)gameTime.ElapsedGameTime.TotalSeconds * velocity.Y);*/

            position = Matrix.CreateTranslation((float)gameTime.ElapsedGameTime.TotalSeconds * velocity.X, 
                0,
                (float)gameTime.ElapsedGameTime.TotalSeconds * velocity.Y);
            world = world * position;
		}
	}
}
