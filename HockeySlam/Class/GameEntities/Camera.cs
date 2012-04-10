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
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities
{

	public class Camera : IGameEntity
	{
		Vector3 _position;
		Vector3 _target;
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
			view = Matrix.CreateLookAt(pos, target, up);

			projection = Matrix.CreatePerspectiveFieldOfView(
			    MathHelper.PiOver4,
			    (float)game.Window.ClientBounds.Width /
			    (float)game.Window.ClientBounds.Height,
			    1, 3000);
		}

		public Vector3 getPosition()
		{
			return _position;
		}

		public Vector3 getTarget()
		{
			return _target;
		}
		public void Update(GameTime gameTime) { }
		public void Draw(GameTime gameTime) { }
		public void Initialize() { }
		public void LoadContent() { }
	}
}