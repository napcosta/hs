using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using HockeySlam.GameState;

namespace HockeySlam.Screens
{
	public enum ScreenState
	{
		TransitionOn,
		Active,
		TransitionOff,
		Hidden,
	}

	public abstract class GameScreen
	{

		#region Properties

		public bool IsPopup
		{
			get { return isPopup; }
			protected set { isPopup = value; }
		}

		bool isPopup = false;

		// Idicates how long teh screen takes to transition on
		// when it is activated.
		public TimeSpan TransitionOnTime
		{
			get { return transitionOnTime; }
			protected set { transitionOnTime = value; }
		}

		public TimeSpan transitionOnTime = TimeSpan.Zero;

		// Time of transition when screen is deactivated
		public TimeSpan TransitionOffTime
		{
			get { return transitionOffTime; }
			protected set { transitionOffTime = value; }
		}

		TimeSpan transitionOffTime = TimeSpan.Zero;

		// Gets the current position of the screen transition, ranging
		// from zero (fully active, no transition) to one (transitioned
		// fully off to nothing).
		public float TransitionPosition
		{
			get { return transitionPosition; }
			protected set { transitionPosition = value; }
		}

		float transitionPosition = 1;

		// Gets the current alpha of the screen transition, raging
		// 1 (fully active, no transition) to 0 (transitioned fully
		// off to nothing)

		public float TransitionAlpha
		{
			get { return 1f - TransitionPosition; }
		}

		// Gets the current screen transition state
		public ScreenState ScreenState
		{
			get { return screenState; }
			protected set { screenState = value; }
		}

		ScreenState screenState = ScreenState.TransitionOn;

		public bool IsExiting
		{
			get { return isExiting; }
			protected internal set { isExiting = value; }
		}

		bool isExiting = false;

		// Check whether this screen is active and can respond to user input
		public bool IsActive
		{
			get
			{
				return !otherScreenHasFocus &&
					(screenState == ScreenState.TransitionOn ||
					 screenState == ScreenState.Active);
			}
		}

		bool otherScreenHasFocus;

		// Gets the manager that this screen belongs to.
		public ScreenManager ScreenManager
		{
			get { return screenManager; }
			internal set { screenManager = value; }
		}

		ScreenManager screenManager;

		// Gets the index of the player who's controlling the screen
		// or null if it is accepting input from any player.
		public PlayerIndex? ControllingPlayer
		{
			get { return controllingPlayer; }
			internal set { controllingPlayer = value; }
		}

		PlayerIndex? controllingPlayer;

		public bool IsSerializable
		{
			get { return isSerializable; }
			protected set { isSerializable = value; }
		}

		bool isSerializable = true;

		#endregion

		#region Abstract Methods

		// Activates the screen
		public virtual void Activate(bool instancePreserved) { }

		// Deactivates the screen
		public virtual void Deactivate() { }

		// Unload content for the screen
		public virtual void Unload() { }

		// Only called when the screen is active
		// public virtual void HandleInput(GameTime gameTime, InputState input) { }

		// Draw call
		public virtual void Draw(GameTime gameTime) { }

		public virtual void HandleInput(GameTime gameTime, InputState input) { }

		#endregion

		#region Update

		public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			this.otherScreenHasFocus = otherScreenHasFocus;

			if(isExiting)
			{
				// Change state because the screen is exiting
				screenState = ScreenState.TransitionOff;

				if (!UpdateTransition(gameTime, transitionOffTime, 1))
				{
					ScreenManager.RemoveScreen(this);
				}
			}
			else if (coveredByOtherScreen)
			{
				if (UpdateTransition(gameTime, transitionOffTime, 1))
				{
					screenState = ScreenState.TransitionOff;
				}
				else
				{
					screenState = ScreenState.Hidden;
				}
			}
			else
			{
				if (UpdateTransition(gameTime, transitionOnTime, -1))
				{
					screenState = ScreenState.TransitionOn;
				}
				else
				{
					screenState = ScreenState.Active;
				}
			}
		}

		#endregion

		#region Other Methods

		bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
		{
			// How much should we move by?
			float transitionDelta;

			if (time == TimeSpan.Zero)
				transitionDelta = 1;
			else
				transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

			transitionPosition += transitionDelta * direction;

			// Did we reach the end of transition
			if (((direction < 0) && (transitionPosition <= 0)) ||
				((direction > 0) && (transitionPosition >= 1)))
			{
				transitionPosition = MathHelper.Clamp(transitionPosition, 0, 1);
				return false;
			}

			return true;
		}

		public void ExitScreen()
		{
			if (TransitionOffTime == TimeSpan.Zero)
			{
				ScreenManager.RemoveScreen(this);
			}
			else
			{
				isExiting = true;
			}
		}

		#endregion
	}
}
