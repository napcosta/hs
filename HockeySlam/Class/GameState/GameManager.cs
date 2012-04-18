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
using HockeySlam.Interface;
namespace HockeySlam.Class.GameState
{
	class GameManager
	{
		#region Fields

		Dictionary<string, IGameEntity> activeEntities = new Dictionary<string, IGameEntity>();
		Dictionary<string, IGameEntity> allEntities = new Dictionary<string, IGameEntity>();
		Game _game;
		

		#endregion

		#region Initialization

		public GameManager(Game game)
		{
			_game = game;
		}

		public void startGame()
		{
			ActivateAllEntities();
			Initialize();
			LoadContent();
		}

		public void Initialize()
		{
			foreach (KeyValuePair<string, IGameEntity> pair in allEntities)
				pair.Value.Initialize();
		}

		protected void LoadContent()
		{
			foreach (KeyValuePair<string, IGameEntity> pair in allEntities)
				pair.Value.LoadContent();
		}

		#endregion

		#region Methods

		public void AddEntity(string name, IGameEntity entity)
		{
			allEntities.Add(name, entity);
		}

		public IGameEntity getGameEntity(string name)
		{
			if (!allEntities.ContainsKey(name))
				return null;

			return allEntities[name];
		}

		protected void ActivateAllEntities()
		{
			activeEntities.Clear();

			foreach (KeyValuePair<string, IGameEntity> pair in allEntities)
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
			foreach (KeyValuePair<string, IGameEntity> pair in activeEntities)
				pair.Value.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			BlendState lastBlend = _game.GraphicsDevice.BlendState;
			DepthStencilState lastDepth = _game.GraphicsDevice.DepthStencilState;

			_game.GraphicsDevice.BlendState = BlendState.Opaque;
			_game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			Ice ice = (Ice)getGameEntity("ice");
			ice.preDraw(gameTime);

			foreach (KeyValuePair<string, IGameEntity> pair in activeEntities)
				pair.Value.Draw(gameTime);

			_game.GraphicsDevice.BlendState = lastBlend;
			_game.GraphicsDevice.DepthStencilState = lastDepth;

			/************/

		}

		#endregion
	}
}
