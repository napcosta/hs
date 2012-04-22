using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using HockeySlam.Class.GameState;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameEntities.Models
{
	class Court : BaseModel, IReflectable
	{
		Matrix rotation = Matrix.Identity;
		Effect _effect;
		GameManager _gameManager;

		public Court(Game game, Camera camera, GameManager gameManager)
			: base(game, camera)
		{
			_model = game.Content.Load<Model>(@"Models\court");
			_effect = game.Content.Load<Effect>(@"Effects\SimpleEffect");
			_gameManager = gameManager;
		}

		public override void LoadContent()
		{
			base.LoadContent();
		}

		public override void Initialize()
		{
			Ice ice = (Ice)_gameManager.getGameEntity("ice");
			ice.register(this);
			base.Initialize();
		}

		public override void Draw(GameTime gameTime)
		{
			Vector3 diffuseColor;
			diffuseColor = new Vector3(0.75f, 0.75f, 0.8f);
			base.DrawEffect(_effect, diffuseColor);
		}

		public override void Update(GameTime gameTime)
		{
			//System.Console.WriteLine("updating court");
			//rotation *= Matrix.CreateRotationY(MathHelper.Pi / 180);
		}

		void IReflectable.Draw(GameTime gameTime, Camera camera)
		{
			Camera lastCamera = _camera;
			_camera = camera;
			Draw(gameTime);
			_camera = lastCamera;
		}

		void IReflectable.setClipPlane(Vector4? plane)
		{
			
		}
	}
}