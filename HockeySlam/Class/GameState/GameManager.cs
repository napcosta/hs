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
using Microsoft.Xna.Framework.Net;

using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Models;
using HockeySlam.Interface;

namespace HockeySlam.Class.GameState
{
	public class GameManager
	{
		#region Fields

		Dictionary<string, IGameEntity> activeEntities = new Dictionary<string, IGameEntity>();
		Dictionary<string, IGameEntity> allEntities = new Dictionary<string, IGameEntity>();
		Game _game;
		NetworkSession _networkSession;

		#endregion

		#region Initialization

		public GameManager(Game game, NetworkSession netwokSession)
		{
			_game = game;
			_networkSession = netwokSession;
		}

		private void addEntities()
		{
			Camera camera;
			if (_networkSession != null)
				camera = new MultiplayerCamera(_game, new Vector3(85, 85, 0), Vector3.Zero, Vector3.Up);
			else
				camera = new Camera(_game, new Vector3(100, 100, 0), Vector3.Zero, Vector3.Up);
			AddEntity("camera", camera);
			AddEntity("debugManager", new DebugManager());
			AddEntity("collisionManager", new CollisionManager());
			AddEntity("court", new Court(_game, camera, this));
			//AddEntity("atmosphere", new Atmosphere(_game, camera));
			if (_networkSession != null) {
				AddEntity("multiplayerManager", new MultiplayerManager(_game, camera, this, _networkSession));
			} else {
				AddEntity("disk", new Disk(this, _game, camera, true));
				AddEntity("reactiveAgentManager", new ReactiveAgentManager(this, _game, camera));
			}
			//AddEntity("disk", new Disk(this, _game, camera));
			AddEntity("ice", new Ice(_game, camera, this));
			AddEntity("arrowManager", new ArrowManager(_game));
		}

		public void startGame()
		{
			addEntities();
			ActivateAllEntities();
			Initialize();
			LoadContent();
			CreateAllPlayers((Camera)getGameEntity("camera"));
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

		void CreateAllPlayers(Camera camera)
		{
			if (_networkSession != null) {
				foreach (NetworkGamer gamer in _networkSession.AllGamers) {
					Player newPlayer = new Player(this, _game, camera, 2, false);
					newPlayer.Initialize();
					newPlayer.LoadContent();

					gamer.Tag = newPlayer;
				}
			}
		}

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
