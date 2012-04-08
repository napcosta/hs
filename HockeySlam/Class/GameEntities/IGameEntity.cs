using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;

namespace HockeySlam.GameEntities
{
	public interface IGameEntity
	{
		void Update(GameTime gameTime);
		void Draw(GameTime gameTime);
		void Initialize();
		void LoadContent();

	}
}
