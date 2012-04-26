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
	enum MenuSelected {NONE, JUST_CHANGED, BLURTYPE, BLUR, TRANSPARECY};
	enum BlurType { NORMAL, GAUSSION }

	class PropertiesMenuScreen : MenuScreen
	{

		int _menuSelected;
		GameManager _gameManager;

		InputAction _upAction;
		InputAction _downAction;
		InputAction _deselectAction;

		MenuEntry _blurType;
		MenuEntry _blur;
		MenuEntry _iceTransparency;

		Ice _ice;

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

			_deselectAction = new InputAction(
				new Buttons[] { Buttons.A },
				new Keys[] { Keys.Enter },
				true);

			_ice = (Ice)_gameManager.getGameEntity("ice");
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
			else {
				_menuSelected = (int)MenuSelected.BLUR;
				_blur.Text = "Blur: " + _ice.getBlurAmount();
			}
		}

		void TransparacySelected(object sender, PlayerIndexEventArgs e)
		{
			if (_menuSelected == (int)MenuSelected.TRANSPARECY)
				_menuSelected = (int)MenuSelected.NONE;
			else {
				_menuSelected = (int)MenuSelected.TRANSPARECY;
				_iceTransparency.Text = "Ice Transparency: " + _ice.getTransparency();
			}
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

			if (_menuSelected == (int)MenuSelected.JUST_CHANGED)
				_menuSelected = (int)MenuSelected.NONE;

			switch (_menuSelected) {
				case (int)MenuSelected.BLURTYPE:
					if (_upAction.Evaluate(input, ControllingPlayer, out player))
						nextBlurType();
					else if (_downAction.Evaluate(input, ControllingPlayer, out player))
						nextBlurType();
					else if (_deselectAction.Evaluate(input, ControllingPlayer, out player))
						_menuSelected = (int)MenuSelected.JUST_CHANGED;
					break;

				case (int)MenuSelected.BLUR:
					if (_upAction.Evaluate(input, ControllingPlayer, out player))
						moreBlur();
					else if (_downAction.Evaluate(input, ControllingPlayer, out player))
						lessBlur();
					else if (_deselectAction.Evaluate(input, ControllingPlayer, out player)) {
						_blur.Text = "Blur";
						_menuSelected = (int)MenuSelected.JUST_CHANGED;
					}
					break;

				case (int)MenuSelected.TRANSPARECY:
					if (_upAction.Evaluate(input, ControllingPlayer, out player)) {
						moreTransparacy();
					}
					else if (_downAction.Evaluate(input, ControllingPlayer, out player)) {
						lessTransparacy();
					} 
					else if (_deselectAction.Evaluate(input, ControllingPlayer, out player)) {
						_iceTransparency.Text = "Ice Transparency";
						_menuSelected = (int)MenuSelected.JUST_CHANGED;
					}
					break;

				case (int)MenuSelected.NONE:
				default:
					break;
			}

			if(_menuSelected == (int)MenuSelected.NONE)
				base.HandleInput(gameTime, input);
		}

		private void lessTransparacy()
		{
			float transparency = _ice.removeTransparency();

			_iceTransparency.Text = "Ice Transparency: " + transparency;
		}

		private void moreTransparacy()
		{
			float transparency = _ice.addTransparency();

			_iceTransparency.Text = "Ice Transparency: " + transparency;
		}

		private void lessBlur()
		{
			float blur = _ice.removeBlur();

			_blur.Text = "Blur: " + blur;
		}

		private void moreBlur()
		{
			float blur = _ice.addBlur();

			_blur.Text = "Blur: " + blur;
		}

		private void nextBlurType()
		{
			int blurType = _ice.anotherBlurType();

			if (blurType == 0)
				_blurType.Text = "Normal Blur";
			else if (blurType == 1)
				_blurType.Text = "Gaussian Blur";
		}

	}
}
