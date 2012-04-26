using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class.Screens
{
	enum MenuSelected {NONE, BLURTYPE, BLUR, TRANSPARECY};
	enum BlurType { NORMAL, GAUSSION }

	class PropertiesMenuScreen : MenuScreen
	{

		int _menuSelected;
		GameManager _gameManager;

		InputAction _upAction;
		InputAction _downAction;

		MenuEntry _blurType;
		MenuEntry _blur;
		MenuEntry _iceTransparency;

		public PropertiesMenuScreen(GameManager gameManager)
			: base("Properties")
		{
			_blurType = new MenuEntry("Normal Blur");
			_blur = new MenuEntry("Blur");
			_iceTransparency = new MenuEntry("Ice Transparecy");
			MenuEntry exit = new MenuEntry("Exit");

			_blurType.Selected += BlurTypeSelected;
			_blur.Selected += BlurSelected;
			_iceTransparency.Selected += TransparacySelected;
			exit.Selected += OnCancel;

			MenuEntries.Add(_blurType);
			MenuEntries.Add(_blur);
			MenuEntries.Add(_iceTransparency);
			MenuEntries.Add(exit);

			_menuSelected = (int)MenuSelected.NONE;
			_gameManager = gameManager;

			_upAction = new InputAction(
				new Buttons[] { Buttons.DPadUp },
				new Keys[] { Keys.Right },
				true);
			_downAction = new InputAction(
				new Buttons[] { Buttons.DPadDown },
				new Keys[] { Keys.Left },
				true);
		}

		void BlurTypeSelected(object sender, PlayerIndexEventArgs e)
		{
			if (_menuSelected == (int)MenuSelected.BLURTYPE)
				_menuSelected = (int)MenuSelected.NONE;
			else _menuSelected = (int)MenuSelected.BLURTYPE;
		}

		void BlurSelected(object sender, PlayerIndexEventArgs e)
		{
			if (_menuSelected == (int)MenuSelected.BLUR)
				_menuSelected = (int)MenuSelected.NONE;
			else _menuSelected = (int)MenuSelected.BLUR;
		}

		void TransparacySelected(object sender, PlayerIndexEventArgs e)
		{
			if (_menuSelected == (int)MenuSelected.TRANSPARECY)
				_menuSelected = (int)MenuSelected.NONE;
			else _menuSelected = (int)MenuSelected.TRANSPARECY;
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		public override void HandleInput(GameTime gameTime, InputState input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			PlayerIndex player;

			switch (_menuSelected) {
				case (int)MenuSelected.BLURTYPE:
					if (_upAction.Evaluate(input, ControllingPlayer, out player)) {
						nextBlurType();
					} else if (_downAction.Evaluate(input, ControllingPlayer, out player)) {
						nextBlurType();
					}
					break;

				case (int)MenuSelected.BLUR:
					if (_upAction.Evaluate(input, ControllingPlayer, out player)) {
						moreBlur();
					} else if (_downAction.Evaluate(input, ControllingPlayer, out player)) {
						lessBlur();
					}
					break;

				case (int)MenuSelected.TRANSPARECY:
					if (_upAction.Evaluate(input, ControllingPlayer, out player)) {
						moreTransparacy();
					} else if (_downAction.Evaluate(input, ControllingPlayer, out player)) {
						lessTransparacy();
					}
					break;
			}

			if(_menuSelected == (int)MenuSelected.NONE)
				base.HandleInput(gameTime, input);
		}

		private void lessTransparacy()
		{
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.removeTransparency();
		}

		private void moreTransparacy()
		{
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.addTransparency();
		}

		private void lessBlur()
		{
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.removeBlur();
		}

		private void moreBlur()
		{
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.addBlur();
		}

		private void nextBlurType()
		{
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.anotherBlurType();
		}

	}
}
