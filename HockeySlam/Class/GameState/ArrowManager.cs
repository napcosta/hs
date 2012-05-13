using System;
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
		Vector2  _localPlayerPos;

		Texture2D _arrow;
		float _rotation;
		public ArrowManager(Game game)
		{
			_game = game;
			_rotation = MathHelper.PiOver2;
		}

		public void Update(GameTime gameTime){}

		public void Draw(GameTime gameTime)
		{
			BlendState lastBlend = _game.GraphicsDevice.BlendState;
			DepthStencilState lastDepth = _game.GraphicsDevice.DepthStencilState;

			SpriteBatch spriteBatch = new SpriteBatch(_game.GraphicsDevice);

			spriteBatch.Begin();
			foreach (KeyValuePair<Player, Vector2> pair in _playerOffScreen) {
				Rectangle src = new Rectangle((int)pair.Value.X, (int)pair.Value.Y, 30, 30);
				spriteBatch.Draw(_arrow, src, null, Color.White, -_rotation, Vector2.Zero, SpriteEffects.None, 0);
			}

			Rectangle src2 = new Rectangle((int)_localPlayerPos.X+15, (int)_localPlayerPos.Y-50, 30, 30);
			spriteBatch.Draw(_arrow, src2, null, Color.White, MathHelper.Pi, Vector2.Zero, SpriteEffects.None, 0);

			spriteBatch.End();

			_game.GraphicsDevice.BlendState = lastBlend;
			_game.GraphicsDevice.DepthStencilState = lastDepth;
		}


		public void Initialize()
		{
			_playerOffScreen = new Dictionary<Player, Vector2>();
		}

		public void LoadContent()
		{
			_arrow = _game.Content.Load<Texture2D>("Textures/Arrow");
		}

		public void updatePosition(Player player, Vector2 position,float rotation)
		{
			_rotation = rotation;
			if(_playerOffScreen.ContainsKey(player))
				_playerOffScreen[player] = position;
			else _playerOffScreen.Add(player, position);
		}

		public void unregister(Player player)
		{
			if(_playerOffScreen.ContainsKey(player))
				_playerOffScreen.Remove(player);
		}

		public void setLocalPlayerPos(Vector2 localPlayerPos)
		{
			_localPlayerPos = localPlayerPos;
		}
	}
}
