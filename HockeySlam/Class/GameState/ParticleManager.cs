using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

using HockeySlam.Interface;
using HockeySlam.Class.GameEntities;
using HockeySlam.Class.GameEntities.Particles;
using HockeySlam.Class.Particles;
using HockeySlam.Class.GameEntities.Models;

namespace HockeySlam.Class.GameState
{
	class ParticleManager : IGameEntity
	{
		Game game;
		Camera camera;
		NetworkSession networkSession;

		List<ParticleSystem> particles;

		public ParticleManager(Game game, Camera camera, NetworkSession networkSession)
		{
			this.game = game;
			this.camera = camera;
			this.networkSession = networkSession;
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
			foreach (NetworkGamer gamer in networkSession.AllGamers) {
				Player player = gamer.Tag as Player;
				particles.Add(new Trail(game, game.Content, player));
				particles.Add(new IceParticles(game, game.Content, player));
			}
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
