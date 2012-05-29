using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using HockeySlam.Class.Particles;
using HockeySlam.Class.GameState;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class.GameEntities.Particles
{
	class IceParticles : ParticleSystem
	{
		GameManager gameManager;
		Dictionary<Player, ParticleEmitter> playerParticles;

		public IceParticles(Game game, ContentManager content, GameManager gameManager)
			: base(game, content)
		{
			this.gameManager = gameManager;
		}

		protected override void InitializeSettings(ParticleSettings settings)
		{
			playerParticles = new Dictionary<Player, ParticleEmitter>();

			MultiplayerManager multiplayerManager = (MultiplayerManager)gameManager.getGameEntity("multiplayerManager");
			if (multiplayerManager != null) {
				List<Player> players = multiplayerManager.GetPlayers();

				foreach (Player player in players) {
					if (player != null)
						playerParticles.Add(player, new ParticleEmitter(this, 50, Vector3.Zero));
				}
			}

			settings.TextureName = "Textures/whiteParticle";

			settings.MaxParticles = 1000;

			settings.Duration = TimeSpan.FromSeconds(4);

			settings.DurationRandomness = 1.5f;

			settings.EndVelocity = 2;

			settings.EmitterVelocitySensitivity = -0.1f;

			settings.MinVerticalVelocity = 1;
			settings.MaxVerticalVelocity = 6;

			settings.Gravity = new Vector3(0, -13, 0);

			settings.MinColor = Color.LightBlue;

			settings.MinStartSize = 0.1f;
			settings.MaxStartSize = 0.2f;

			settings.MinEndSize = 0.3f;
			settings.MaxEndSize = 0.4f;
		}

		public override void  SpecificUpdate(GameTime gameTime)
		{
			MultiplayerManager multiplayerManager = (MultiplayerManager)gameManager.getGameEntity("multiplayerManager");
			if (multiplayerManager != null) {
				List<Player> players = multiplayerManager.GetPlayers();

				foreach (Player player in players)
					if(!playerParticles.ContainsKey(player) && player != null)
						playerParticles.Add(player, new ParticleEmitter(this, 50, Vector3.Zero));
			}

			foreach (KeyValuePair<Player, ParticleEmitter> player in playerParticles) {
				player.Value.Update(gameTime, player.Key.getPositionVector());
			}
		}
	}
}
