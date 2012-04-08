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
		Effect _iceEffect;
		Game _game;
		Vector3[] diffuseColor;
		ContentManager _content;
		GraphicsDevice _graphics;

		public Court(Game game, Camera camera)
			: base(game, camera)
		{
			model = game.Content.Load<Model>(@"Models\court2");
			_game = game;
			_content = _game.Content;
			_graphics = _game.GraphicsDevice;

			_iceEffect = _game.Content.Load<Effect>(@"Effects\IceEffect");
			_iceEffect.Parameters["viewportWidth"].SetValue(_graphics.Viewport.Width);
			_iceEffect.Parameters["viewportHeight"].SetValue(_graphics.Viewport.Height);


			
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}
		


		public override void Draw(GameTime gameTime)
		{
			Vector3 diffuseColor;
			diffuseColor = new Vector3(0.75f, 0.75f, 0.8f);
			base.DrawEffect(iceEffect, diffuseColor);
		}

		public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}
	}
}