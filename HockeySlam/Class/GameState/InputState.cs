using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HockeySlam.Class.GameState
{
	// Helper for reading input from keyboard and gamepad.
	public class InputState
	{
		public const int MaxInputs = 4;

		public readonly KeyboardState[] CurrentKeyboardStates;
		public readonly GamePadState[] CurrentGamePadStates;

		public readonly KeyboardState[] LastKeyboardStates;
		public readonly GamePadState[] LastGamePadStates;

		public readonly bool[] GamePadWasConnected;

		public InputState()
		{
			CurrentKeyboardStates = new KeyboardState[MaxInputs];
			CurrentGamePadStates = new GamePadState[MaxInputs];

			LastKeyboardStates = new KeyboardState[MaxInputs];
			LastGamePadStates = new GamePadState[MaxInputs];

			GamePadWasConnected = new bool[MaxInputs];
		}

		public void Update()
		{
			for (int i = 0; i < MaxInputs; i++)
			{
				LastKeyboardStates[i] = CurrentKeyboardStates[i];
				LastGamePadStates[i] = CurrentGamePadStates[i];

				CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
				CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

				if (CurrentGamePadStates[i].IsConnected)
					GamePadWasConnected[i] = true;
			}
		}

		public bool IsKeyPressed(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;

				int i = (int)playerIndex;
				return CurrentKeyboardStates[i].IsKeyDown(key);
			}
			else
			{
				return (IsKeyPressed(key, PlayerIndex.One, out playerIndex) ||
						IsKeyPressed(key, PlayerIndex.Two, out playerIndex) ||
						IsKeyPressed(key, PlayerIndex.Three, out playerIndex) ||
						IsKeyPressed(key, PlayerIndex.Four, out playerIndex));
			}
		}

		public bool IsButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;

				int i = (int)playerIndex;

				return CurrentGamePadStates[i].IsButtonDown(button);
			}
			else
			{
				return (IsButtonPressed(button, PlayerIndex.One, out playerIndex) ||
						IsButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
						IsButtonPressed(button, PlayerIndex.Three, out playerIndex) ||
						IsButtonPressed(button, PlayerIndex.Four, out playerIndex));
			}
		}

		public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;

				int i = (int)playerIndex;

				return (CurrentKeyboardStates[i].IsKeyDown(key) &&
						LastKeyboardStates[i].IsKeyUp(key));
			}
			else
			{
				return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
						IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
						IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
						IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
			}
		}

		public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
		{
			if (controllingPlayer.HasValue)
			{
				playerIndex = controllingPlayer.Value;

				int i = (int)playerIndex;

				return (CurrentGamePadStates[i].IsButtonDown(button) &&
						LastGamePadStates[i].IsButtonUp(button));
			}
			else
			{
				return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
						IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
						IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
						IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
			}
		}
	}
}
