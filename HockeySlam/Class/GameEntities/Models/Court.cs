using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace HockeySlam.GameEntities.Models
{
	class Court : BaseModel
	{
		Matrix rotation = Matrix.Identity;
		Effect effect;
		Game game;
		Vector3[] diffuseColor;

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
			Vector3 diffuseColor;
			diffuseColor = new Vector3(0.75f, 0.75f, 0.8f);
			base.DrawEffect(effect, diffuseColor);
		}

		public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}
	}
}