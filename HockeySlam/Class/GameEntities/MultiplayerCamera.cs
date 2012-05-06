using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HockeySlam.Class.GameEntities
{
	class MultiplayerCamera : Camera
	{
		public MultiplayerCamera(Game game, Vector3 pos, Vector3 target, Vector3 up) : base(game, pos, target, up) {
		}
		public void updateTargetAndPosition()
		{
			float distanceZ = Math.Max(_localPlayerPosition.Z, _diskPosition.Z) - Math.Min(_localPlayerPosition.Z, _diskPosition.Z);
			float distanceX = Math.Max(_localPlayerPosition.X, _diskPosition.X) - Math.Min(_localPlayerPosition.X, _diskPosition.X);

			_target.Z = (distanceZ / 2.0f) + Math.Min(_localPlayerPosition.Z, _diskPosition.Z);
			_target.X = (distanceX / 2.0f) + Math.Min(_localPlayerPosition.X, _diskPosition.X);
			_position.Z = _target.Z;

			_position.Y = Math.Max(distanceZ, distanceX);

			if (_position.Y < 50)
				_position.Y = 50;
			else if (_position.Y > 85)
				_position.Y = 85;

			_position.X = _target.X + _position.Y;

			//_position.X = _position.Y;
		}

		public override void Update(GameTime gameTime)
		{
			updateTargetAndPosition();
			view = Matrix.CreateLookAt(_position, _target, _up);
		}
	}
}
