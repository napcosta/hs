using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class
{
	class GameManager
	{
		#region Fields

		Dictionary<string, GameEntity> activeEntities = new Dictionary<string, GameEntity>();
		Dictionary<string, GameEntity> allEntities = new Dictionary<string, GameEntity>();
		Camera camera;

		#endregion

		#region Initialization

		public GameManager(Game game)
		{
			camera = new Camera(game, new Vector3(85, 85, 0), Vector3.Zero, new Vector3(0, 1, 0));
			AddEntity("camera1", camera);
			AddEntity("court", new Court(game, camera));
			AddEntity("player1", new Player(game, camera));
			ActivateAllEntities();
			Initialize();
			LoadContent();
		}

		public void Initialize()
		{
			foreach (KeyValuePair<string, GameEntity> pair in allEntities)
				pair.Value.Initialize();
		}

		protected void LoadContent()
		{
			foreach (KeyValuePair<string, GameEntity> pair in allEntities)
				pair.Value.LoadContent();
		}

		#endregion

		#region Methods

		protected void AddEntity(string name, GameEntity entity)
		{
			allEntities.Add(name, entity);
		}

		protected void ActivateAllEntities()
		{
			activeEntities.Clear();

			foreach (KeyValuePair<string, GameEntity> pair in allEntities)
				activeEntities.Add(pair.Key, pair.Value);
		}

		protected void ActivateEntity(string name)
		{
			if (!activeEntities.ContainsKey(name))
				activeEntities.Add(name, allEntities[name]);
		}

		protected void DeactivateEntity(string name)
		{
			if (activeEntities.ContainsKey(name))
				activeEntities.Remove(name);
		}

		#endregion

		#region Update & Draw

		public void Update(GameTime gameTime)
		{
			foreach (KeyValuePair<string, GameEntity> pair in activeEntities)
				pair.Value.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			foreach (KeyValuePair<string, GameEntity> pair in activeEntities)
				pair.Value.Draw(gameTime);
		}

		#endregion
	}
}
