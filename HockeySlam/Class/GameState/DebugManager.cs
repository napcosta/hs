using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using HockeySlam.GameEntities;

namespace HockeySlam.GameState
{
	class DebugManager : IGameEntity
	{

		List<IDebugEntity> debugEntities;
		bool toggleDebug;
		bool debugKeyPressed;

		public void LoadContent() { }

		public void Initialize()
		{
			debugEntities = new List<IDebugEntity>();
			toggleDebug = false;
			debugKeyPressed = false;
		}

		public void registreDebugEntities(IDebugEntity debugEntity) 
		{
			debugEntities.Add(debugEntity);
		}

		public void Update(Microsoft.Xna.Framework.GameTime gameTime) 
		{
			KeyboardState currentKeyboardState = Keyboard.GetState();

			if (currentKeyboardState.IsKeyDown(Keys.Q) && !debugKeyPressed) {
				debugKeyPressed = true;
				toggleDebug = !toggleDebug;
			} else if (currentKeyboardState.IsKeyUp(Keys.Q) && debugKeyPressed) {
				debugKeyPressed = false;
			}
		}

		public void Draw(Microsoft.Xna.Framework.GameTime gameTime) 
		{
			if (toggleDebug) {
				foreach (IDebugEntity debugEntity in debugEntities)
					debugEntity.DrawDebug();
			}
		}
	}
}
