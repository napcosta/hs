using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameState
{
	class MultiplayerManager : GamerServicesComponent, IGameEntity
	{
		public MultiplayerManager(Game game)
			: base(game)
		{ }

		public void Draw(GameTime gameTime)
		{ }

		public void LoadContent()
		{ }
	}
}
