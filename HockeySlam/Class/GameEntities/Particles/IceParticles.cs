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
		ParticleEmitter particleEmitter;
		Player player;

		public IceParticles(Game game, ContentManager content, Player player)
			: base(game, content)
		{
			this.player = player;
		}

		protected override void InitializeSettings(ParticleSettings settings)
		{
			particleEmitter = new ParticleEmitter(this, 10, Vector3.Zero);

			settings.TextureName = "Textures/whiteParticle";

			settings.MaxParticles = 1000;

			settings.Duration = TimeSpan.FromSeconds(4);

			settings.DurationRandomness = 1.5f;

			settings.EndVelocity = 2;

			settings.EmitterVelocitySensitivity = -0.1f;

			settings.MinVerticalVelocity = 1;
			settings.MaxVerticalVelocity = 6;

			settings.Gravity = new Vector3(0, -13, 0);

			settings.MinColor = new Color(240, 240, 255);
			settings.MaxColor = new Color(240, 240, 255);

			settings.MinStartSize = 0.1f;
			settings.MaxStartSize = 0.2f;

			settings.MinEndSize = 0.2f;
			settings.MaxEndSize = 0.2f;
		}

		public override void  SpecificUpdate(GameTime gameTime)
		{
			particleEmitter.Update(gameTime, player.getPositionVector());
		}
	}
}
