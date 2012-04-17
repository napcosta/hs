using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace HockeySlam.Class.GameEntities.Models
{
	class Court : BaseModel
	{
		Matrix rotation = Matrix.Identity;
		Effect _effect;

		public Court(Game game, Camera camera)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\court2");

			_effect = game.Content.Load<Effect>(@"Effects\SimpleEffect");
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}
		


		public override void Draw(GameTime gameTime)
		{
			Vector3 diffuseColor;
			diffuseColor = new Vector3(0.75f, 0.75f, 0.8f);
			base.DrawEffect(_effect, diffuseColor);
		}

		public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}
	}
}