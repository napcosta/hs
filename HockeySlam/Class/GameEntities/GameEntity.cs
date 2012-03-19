using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;

namespace HockeySlam.Class.GameEntities
{
	public interface GameEntity
	{
		void Update(GameTime gameTime);
		void Draw(GameTime gameTime);
		void Initialize();
		void LoadContent();

	}
}
