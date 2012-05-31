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
	class Trail : ParticleSystem
	{
		ParticleEmitter particleEmitter;
		Player player;

		public Trail(Game game, ContentManager content, Player player)
			: base(game, content)
		{
			this.player = player;
		}

		protected override void InitializeSettings(ParticleSettings settings)
		{
			particleEmitter =  new ParticleEmitter(this, 70, Vector3.Zero);

			settings.TextureName = "Textures/trace";

			settings.MaxParticles = 1000;

			settings.Duration = TimeSpan.FromSeconds(15);

			settings.DurationRandomness = 1.5f;

			settings.EndVelocity = 0;

			settings.EmitterVelocitySensitivity = 0;

			settings.MinVerticalVelocity = 0;
			settings.MaxVerticalVelocity = 0;

			settings.Gravity = new Vector3(0, 0, 0);

			settings.MinColor = new Color(220, 220, 255);
			settings.MaxColor = new Color(220, 220, 255);

			settings.MinStartSize = 0.15f;
			settings.MaxStartSize = 0.15f;

			settings.MinEndSize = 0.15f;
			settings.MaxEndSize = 0.15f;
		}

		public override void SpecificUpdate(GameTime gameTime)
		{
			Vector3 pos = player.getPositionVector();
			pos.Y = 1;
			particleEmitter.Update(gameTime, pos);
		}
	}
}
