using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HockeySlam.Class.GameEntities.Models
{
	class Atmosphere : BaseModel
	{
		Effect _effect;

		public Atmosphere(Game game, Camera camera)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>("Models/Atmosphere");
		}

		public override void LoadContent()
		{
			_effect = _game.Content.Load<Effect>("Effects/SimpleEffect");
			base.LoadContent();
		}

		public override void Draw(GameTime gameTime)
		{
			Vector3 diffuseColor;
			diffuseColor = new Vector3(1f, 1f, 1f);
			base.DrawEffect(_effect, diffuseColor);
		}
	}
}
