using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace HockeySlam.Class.GameEntities.Models
{
	class Court : BaseModel
	{
		Matrix rotation = Matrix.Identity;
		Effect effect;
		Game game;

		public Court(Game game, Camera camera) : base(game, camera)
		{
			model = game.Content.Load<Model>(@"Models\court2");
			this.game = game;
		}

		public override void LoadContent()
		{
			effect = game.Content.Load<Effect>(@"Effects\SimpleEffect");
			base.LoadContent();
		}

		public override void Draw(GameTime gameTime)
		{
			base.DrawEffect(effect);
		}

		public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}
	}
}