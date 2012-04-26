﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HockeySlam.Interface;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class.GameState
{
	class ArrowManager : IGameEntity
	{
		Dictionary<Player, Vector2> _playerOffScreen;
		Game _game;

		Texture2D _arrow;

		public ArrowManager(Game game)
		{
			_game = game;
		}

		public void Update(GameTime gameTime){}

		public void Draw(GameTime gameTime)
		{
			SpriteBatch spriteBatch = new SpriteBatch(_game.GraphicsDevice);

			spriteBatch.Begin();
			foreach (KeyValuePair<Player, Vector2> pair in _playerOffScreen) {
				Rectangle src = new Rectangle((int)pair.Value.X, (int)pair.Value.Y, 30, 30);
				spriteBatch.Draw(_arrow, src, null, Color.White, -MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0);
			}
			spriteBatch.End();
		}

		public void Initialize()
		{
			_playerOffScreen = new Dictionary<Player, Vector2>();
		}

		public void LoadContent()
		{
			_arrow = _game.Content.Load<Texture2D>("Textures/Arrow");
		}

		public void updatePosition(Player player, Vector2 position)
		{
			if(_playerOffScreen.ContainsKey(player))
				_playerOffScreen[player] = position;
			else _playerOffScreen.Add(player, position);
		}

		public void unregister(Player player)
		{
			if(_playerOffScreen.ContainsKey(player))
				_playerOffScreen.Remove(player);
		}
	}
}