using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace HockeySlam
{
	class Court : BaseModel
	{
		Matrix rotation = Matrix.Identity;

		public Court(Game game) : base(game)
		{
			model = game.Content.Load<Model>(@"Models\court2");
		}

        public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}

		//public override Matrix GetWorld()
		//{
			//return world * rotation;
		//}
	}
}