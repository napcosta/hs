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
using HockeySlam.Screens;


namespace HockeySlam.GameState
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class ScreenManager : Microsoft.Xna.Framework.DrawableGameComponent
	{

		#region Fields

		List<GameScreen> screens = new List<GameScreen>();
		List<GameScreen> tempScreenList = new List<GameScreen>();

		InputState input = new InputState();

		SpriteBatch spriteBatch;
		SpriteFont font;
		Texture2D blankTexture;

		bool isInitialized;

		#endregion

		#region Properties

		public SpriteBatch SpriteBatch
		{
			get { return spriteBatch; }
		}

		public SpriteFont Font
		{
			get { return font; }
		}

		public Texture2D BlankTexture
		{
			get { return blankTexture; }
		}

		#endregion

		#region Initialization

		public ScreenManager(Game game)
			: base(game)
		{
			// TODO: Construct any child components here
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here

			base.Initialize();

			isInitialized = true;
		}

		protected override void LoadContent()
		{
			ContentManager content = Game.Content;

			spriteBatch = new SpriteBatch(GraphicsDevice);
			font = content.Load<SpriteFont>("Fonts/GameFont");
			blankTexture = content.Load<Texture2D>("Screens/blank");

			foreach (GameScreen screen in screens)
			{
				screen.Activate(false);
			}

			base.LoadContent();
		}

		#endregion

		#region Update & Draw
		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			input.Update();

			// Make  acopy of teh master screen list to avoid confusion if
			// the process of updating on screen adds or removes others.
			tempScreenList.Clear();

			foreach (GameScreen screen in screens)
				tempScreenList.Add(screen);

			bool otherScreenHasFocus = !Game.IsActive;
			bool coveredByOtherScreen = false;

			while (tempScreenList.Count > 0)
			{
				// Pop teh topmost screen off teh waiting list.
				GameScreen screen = tempScreenList[tempScreenList.Count - 1];

				tempScreenList.RemoveAt(tempScreenList.Count - 1);

				//Update the screen
				screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

				if (screen.ScreenState == ScreenState.TransitionOn ||
					screen.ScreenState == ScreenState.Active)
				{
					if (!otherScreenHasFocus)
					{
						screen.HandleInput(gameTime, input);
						otherScreenHasFocus = true;
					}

					if (!screen.IsPopup)
						coveredByOtherScreen = true;
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			foreach (GameScreen screen in screens)
			{
				if (screen.ScreenState == ScreenState.Hidden)
					continue;

				screen.Draw(gameTime);
			}
			base.Draw(gameTime);
		}

		#endregion

		#region Public Methods

		// Add screens to screenManager
		public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
		{
			screen.ControllingPlayer = controllingPlayer;
			screen.ScreenManager = this;
			screen.IsExiting = false;

			if (isInitialized)
			{
				screen.Activate(false);
			}

			screens.Add(screen);
		}

		// Remove screen from sceneManager
		public void RemoveScreen(GameScreen screen)
		{
			if (isInitialized)
			{
				screen.Unload();
			}

			screens.Remove(screen);
			tempScreenList.Remove(screen);
		}

		// Get all the screens
		public GameScreen[] GetScreens()
		{
			return screens.ToArray();
		}

		// Function used for the fade effect
		public void FadeBackBufferToBlack(float alpha)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(blankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha);
			spriteBatch.End();
		}

		#endregion
	}
}
