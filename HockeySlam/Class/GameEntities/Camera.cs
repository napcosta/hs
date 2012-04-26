using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using HockeySlam.Class.GameState;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities
{

	public class Camera : IGameEntity
	{
		Vector3 _position;
		Vector3 _target;
		Vector3 _up;

		Vector3 _diskPosition;
		Vector3 _localPlayerPosition;
		Game _game;
		//Camera matrices
		public Matrix view
		{
			get;
			protected set;
		}
		public Matrix projection
		{
			get;
			protected set;
		}

		public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
		{
			_position = pos;
			_target = target;
			_up = up;
			_game = game;
			_localPlayerPosition = target;
			_diskPosition = target;

			view = Matrix.CreateLookAt(pos, target, up);

			projection = Matrix.CreatePerspectiveFieldOfView(
			    MathHelper.PiOver4,
			    (float)game.Window.ClientBounds.Width /
			    (float)game.Window.ClientBounds.Height,
			    1, 300);

		}

		public Vector3 getPosition()
		{
			return _position;
		}

		public Vector3 getTarget()
		{
			return _target;
		}

		public float abs(float num)
		{
			if (num > 0)
				return num;
			else if (num < 0)
				return -num;
			else
				return 0;
		}

		public void updateTargetAndPosition()
		{
			float distanceZ = Math.Max(_localPlayerPosition.Z, _diskPosition.Z) - Math.Min(_localPlayerPosition.Z, _diskPosition.Z);
			float distanceX = Math.Max(_localPlayerPosition.X, _diskPosition.X) - Math.Min(_localPlayerPosition.X, _diskPosition.X);

			_target.Z = (distanceZ / 2.0f) + Math.Min(_localPlayerPosition.Z, _diskPosition.Z);
			_target.X = (distanceX / 2.0f) + Math.Min(_localPlayerPosition.X, _diskPosition.X);
			_position.Z = _target.Z;

			_position.Y = Math.Max(distanceZ,distanceX);

			if (_position.Y < 50)
				_position.Y = 50;
			else if (_position.Y > 85)
				_position.Y = 85;

			_position.X = _target.X + _position.Y;

			//_position.X = _position.Y;
		}

		public void Update(GameTime gameTime) 
		{
		//	updateTargetAndPosition();
			view = Matrix.CreateLookAt(_position, _target, _up);
		}

		public void Draw(GameTime gameTime) { }
		public void Initialize() { }
		public void LoadContent() { }

		public void updateDiskPosition(Vector3 diskPosition)
		{
			_diskPosition = diskPosition;
		}

		public void updateLocalPlayerPosition(Vector3 localPlayerPosition)
		{
			_localPlayerPosition = localPlayerPosition;
		}
	}
}