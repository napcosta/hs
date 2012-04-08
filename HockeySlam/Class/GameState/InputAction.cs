using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HockeySlam.GameState
{
	// Defines an action that is designed by some set of buttons and/or keys.
	class InputAction
	{
		private readonly Buttons[] buttons;
		private readonly Keys[] keys;
		private readonly bool newPressOnly;

		private delegate bool ButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex player);
		private delegate bool KeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex player);

		public InputAction(Buttons[] buttons, Keys[] keys, bool newPressOnly)
		{
			this.buttons = buttons != null ? buttons.Clone() as Buttons[] : new Buttons[0];
			this.keys = keys != null ? keys.Clone() as Keys[] : new Keys[0];

			this.newPressOnly = newPressOnly;
		}

		public bool Evaluate(InputState state, PlayerIndex? controllingPlayer, out PlayerIndex player)
		{
			ButtonPress buttonTest;
			KeyPress keyTest;

			if (newPressOnly)
			{
				buttonTest = state.IsNewButtonPress;
				keyTest = state.IsNewKeyPress;
			}
			else
			{
				buttonTest = state.IsButtonPressed;
				keyTest = state.IsKeyPressed;
			}

			foreach (Buttons button in buttons)
			{
				if (buttonTest(button, controllingPlayer, out player))
					return true;
			}

			foreach (Keys key in keys)
			{
				if (keyTest(key, controllingPlayer, out player))
					return true;
			}

			player = PlayerIndex.One;
			return false;
		}
	}
}
