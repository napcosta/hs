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
		public Atmosphere(Game game, Camera camera)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>("Models/Atmosphere");
		}
	}
}
