using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using HockeySlam.Interface;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Particles;
using HockeySlam.Class.Particles;

namespace HockeySlam.Class.GameState
{
	class ParticleManager : IGameEntity
	{
		Game game;
		Camera camera;
		GameManager gameManager;

		List<ParticleSystem> particles;

		public ParticleManager(Game game, Camera camera, GameManager gameManager)
		{
			this.game = game;
			this.camera = camera;
			this.gameManager = gameManager;
		}

		public void Initialize()
		{
			particles = new List<ParticleSystem>();

			InitializeParticles();

			foreach (ParticleSystem particle in particles)
				particle.Initialize();
		}

		private void InitializeParticles()
		{
			particles.Add(new IceParticles(game, game.Content, gameManager));
		}

		public void LoadContent()
		{
			foreach (ParticleSystem particle in particles)
				particle.LoadContent();
		}

		public void Update(GameTime gameTime)
		{
			foreach (ParticleSystem particle in particles) {
				particle.SetCamera(camera);
				particle.SpecificUpdate(gameTime);
				particle.Update(gameTime);
			}
		}

		public void Draw(GameTime gameTime)
		{
			foreach (ParticleSystem particle in particles)
				particle.Draw(gameTime);
		}
	}
}
