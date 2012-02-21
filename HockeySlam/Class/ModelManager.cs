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
	public class ModelManager : DrawableGameComponent
	{
		// List of models
		List<BaseModel> models = new List<BaseModel>();

		public ModelManager(Game game)
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
		}

		protected override void LoadContent()
		{
			//Add models to list
			models.Add(new Court(
			//    Game.Content.Load<Model>(@"Models\p1_wedge"))
			    Game.Content.Load<Model>(@"Models\court2"))
			    );
            models.Add(new Player(Game.Content.Load<Model>(@"Models\player")));

			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// Loop through all models and call Update
			for (int i = 0; i < models.Count; ++i) {
				models[i].Update(gameTime);
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			// Loop through and draw each model
			foreach (BaseModel bm in models) {
				bm.Draw(((Game1)Game).camera);
			}

			base.Draw(gameTime);
		}
	}
}
