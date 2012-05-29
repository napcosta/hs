using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HockeySlam.Class.Particles
{
	class ParticleEmitter
	{
		#region Fields

		ParticleSystem particleSystem;
		float timeBetweenParticles;
		Vector3 previousPosition;
		float timeLeftOver;

		#endregion

		public ParticleEmitter(ParticleSystem particleSystem, float particlesPerSecond, Vector3 initialPosition)
		{
			this.particleSystem = particleSystem;

			timeBetweenParticles = 1.0f / particlesPerSecond;

			previousPosition = initialPosition;
		}

		public void Update(GameTime gameTime, Vector3 newPosition)
		{
			if (gameTime == null)
				throw new ArgumentNullException("Particle Emitter -> gameTime");

			float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

			if(elapsedTime > 0 && newPosition != previousPosition) {

				Vector3 velocity = (newPosition - previousPosition) / elapsedTime;

				float timeToSpend = timeLeftOver + elapsedTime;

				float currentTime = -timeLeftOver;

				while (timeToSpend > timeBetweenParticles) {
					currentTime += timeBetweenParticles;
					timeToSpend -= timeBetweenParticles;

					float mu = currentTime / elapsedTime;

					Vector3 position = Vector3.Lerp(previousPosition, newPosition, mu);

					particleSystem.AddParticle(newPosition, velocity);
				}

				timeLeftOver = timeToSpend;
			}

			previousPosition = newPosition;
		}
	}
}
