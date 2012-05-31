using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using HockeySlam.Class.GameEntities;
using HockeySlam.Interface;
namespace HockeySlam.Class.GameState
{
	class DebugManager : IGameEntity
	{

		List<IDebugEntity> debugEntities;
		bool toggleDebug;
		bool debugKeyPressed;

		public DebugManager()
		{
			debugEntities = new List<IDebugEntity>();
		}

		public void LoadContent() { }

		public void Initialize()
		{
			toggleDebug = false;
			debugKeyPressed = false;
		}

		public void registerDebugEntities(IDebugEntity debugEntity) 
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
