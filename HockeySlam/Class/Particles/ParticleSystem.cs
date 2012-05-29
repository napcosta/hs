using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HockeySlam.Interface;
using HockeySlam.Class.GameEntities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace HockeySlam.Class.Particles
{
	public abstract class ParticleSystem : IGameEntity
	{
		#region Fields

		// Settings class controls the appearance and animation of this particle system
		ParticleSettings settings = new ParticleSettings();

		Game game;

		ContentManager content;

		Effect particleEffect;

		EffectParameter effectViewParameter;
		EffectParameter effectProjectionParameter;
		EffectParameter effectViewportScaleParameter;
		EffectParameter effectTimeParameter;

		ParticleVertex[] particles;

		DynamicVertexBuffer vertexBuffer;

		IndexBuffer indexBuffer;

		int firstActiveParticle;
		int firstNewParticle;
		int firstFreeParticle;
		int firstRetiredParticle;

		float currentTime;

		int drawCounter;

		static Random random = new Random();

		#endregion

		#region Initialization

		protected ParticleSystem(Game game, ContentManager content)
		{
			this.game = game;
			this.content = content;
		}

		public void Initialize()
		{
			InitializeSettings(settings);

			particles = new ParticleVertex[settings.MaxParticles * 4];

			for (int i = 0; i < settings.MaxParticles; i++) {
				particles[i * 4 + 0].Corner = new Short2(-1, -1);
				particles[i * 4 + 1].Corner = new Short2(1, -1);
				particles[i * 4 + 2].Corner = new Short2(1, 1);
				particles[i * 4 + 3].Corner = new Short2(-1, 1);
			}
		}

		protected abstract void InitializeSettings(ParticleSettings settings);

		public void LoadContent()
		{
			LoadParticleEffect();

			vertexBuffer = new DynamicVertexBuffer(game.GraphicsDevice, ParticleVertex.VertexDeclaration, settings.MaxParticles * 4, BufferUsage.WriteOnly);

			ushort[] indices = new ushort[settings.MaxParticles * 6];

			for (int i = 0; i < settings.MaxParticles; i++) {
				indices[i * 6 + 0] = (ushort)(i * 4 + 0);
				indices[i * 6 + 1] = (ushort)(i * 4 + 1);
				indices[i * 6 + 2] = (ushort)(i * 4 + 2);

				indices[i * 6 + 3] = (ushort)(i * 4 + 0);
				indices[i * 6 + 4] = (ushort)(i * 4 + 2);
				indices[i * 6 + 5] = (ushort)(i * 4 + 3);
			}

			indexBuffer = new IndexBuffer(game.GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);

			indexBuffer.SetData(indices);
		}

		private void LoadParticleEffect()
		{
			Effect effect = content.Load<Effect>("Effects/ParticleEffect");

			particleEffect = effect.Clone();

			EffectParameterCollection parameters = particleEffect.Parameters;

			effectViewParameter = parameters["View"];
			effectProjectionParameter = parameters["Projection"];
			effectViewportScaleParameter = parameters["ViewportScale"];
			effectTimeParameter = parameters["CurrentTime"];

			parameters["Duration"].SetValue((float)settings.Duration.TotalSeconds);
			parameters["DurationRandomness"].SetValue(settings.DurationRandomness);
			parameters["Gravity"].SetValue(settings.Gravity);
			parameters["EndVelocity"].SetValue(settings.EndVelocity);
			parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
			parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());

			parameters["RotateSpeed"].SetValue(new Vector2(settings.MinRotationSpeed, settings.MaxRotationSpeed));

			parameters["StartSize"].SetValue(new Vector2(settings.MinStartSize, settings.MaxStartSize));

			parameters["EndSize"].SetValue(new Vector2(settings.MinEndSize, settings.MaxEndSize));

			Texture2D texture = content.Load<Texture2D>(settings.TextureName);

			parameters["Texture"].SetValue(texture);
		}

		#endregion

		#region Update and Draw

		public void Update(GameTime gameTime)
		{
			if (gameTime == null)
				throw new ArgumentNullException("gameTime");

			currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

			RetiredActiveParticles();
			FreeRetiredParticles();

			if (firstActiveParticle == firstFreeParticle)
				currentTime = 0;

			if (firstRetiredParticle == firstActiveParticle)
				drawCounter = 0;
		}

		public abstract void SpecificUpdate(GameTime gameTime);

		private void RetiredActiveParticles()
		{
			float particleDuration = (float)settings.Duration.TotalSeconds;

			while (firstActiveParticle != firstNewParticle) {
				float particleAge = currentTime - particles[firstActiveParticle * 4].Time;

				if (particleAge < particleDuration)
					break;

				particles[firstActiveParticle * 4].Time = drawCounter;

				firstActiveParticle++;

				if (firstActiveParticle >= settings.MaxParticles)
					firstActiveParticle = 0;
			}
		}

		private void FreeRetiredParticles()
		{
			while (firstRetiredParticle != firstActiveParticle) {
				int age = drawCounter - (int)particles[firstRetiredParticle * 4].Time;

				if (age < 3)
					break;

				firstRetiredParticle++;

				if (firstRetiredParticle >= settings.MaxParticles)
					firstRetiredParticle = 0;
			}
		}

		public void Draw(GameTime gameTime)
		{
			GraphicsDevice device = game.GraphicsDevice;

			if (vertexBuffer.IsContentLost)
				vertexBuffer.SetData(particles);

			if (firstNewParticle != firstFreeParticle)
				AddParticlesToVertexBuffer();

			if (firstRetiredParticle != firstFreeParticle) {
				device.BlendState = settings.BlendState;
				device.DepthStencilState = DepthStencilState.DepthRead;

				effectViewportScaleParameter.SetValue(new Vector2(0.5f / device.Viewport.AspectRatio, -0.5f));

				effectTimeParameter.SetValue(currentTime);

				device.SetVertexBuffer(vertexBuffer);
				device.Indices = indexBuffer;

				foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes) {
					pass.Apply();

					if (firstActiveParticle < firstFreeParticle) {
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
													 firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4,
													 firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
					} else {
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
													 firstActiveParticle * 4, (settings.MaxParticles - firstActiveParticle) * 4,
													 firstActiveParticle * 6, (settings.MaxParticles - firstActiveParticle) * 2);

						if(firstFreeParticle > 0)
							device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, firstFreeParticle*4, 0, firstFreeParticle*2); 
					}
				}

				device.DepthStencilState = DepthStencilState.Default;
			}

			drawCounter++;
		}

		private void AddParticlesToVertexBuffer()
		{
			int stride = ParticleVertex.SizeInBytes;

			if (firstNewParticle < firstFreeParticle) {
				vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (firstFreeParticle - firstNewParticle) * 4,
									 stride, SetDataOptions.NoOverwrite);
			} else {
				vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (settings.MaxParticles - firstNewParticle) * 4,
									 stride, SetDataOptions.NoOverwrite);

				if (firstFreeParticle > 0)
					vertexBuffer.SetData(0, particles, 0, firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
			}

			firstNewParticle = firstFreeParticle;
		}

		#endregion

		#region Public Methods

		public void SetCamera(Camera camera)
		{
			effectViewParameter.SetValue(camera.view);
			effectProjectionParameter.SetValue(camera.projection);
		}

		public void AddParticle(Vector3 position, Vector3 velocity)
		{
			int nextFreeParticle = firstFreeParticle + 1;

			if (nextFreeParticle >= settings.MaxParticles)
				nextFreeParticle = 0;

			if (nextFreeParticle == firstRetiredParticle)
				return;

			bool velocityZero;

			if (velocity == Vector3.Zero)
				velocityZero = true;
			else velocityZero = false;

			velocity *= settings.EmitterVelocitySensitivity;

			//if (!velocityZero) {
				float horizontalVelocity = MathHelper.Lerp(settings.MinHorizontalVelocity, settings.MaxHorizontalVelocity, (float)random.NextDouble());

				double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

				velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
				velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);
			//}

			velocity.Y += MathHelper.Lerp(settings.MinVerticalVelocity, settings.MaxVerticalVelocity, (float)random.NextDouble());

			Color randomValues = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));

			for (int i = 0; i < 4; i++) {
				particles[firstFreeParticle * 4 + i].Position = position;
				particles[firstFreeParticle * 4 + i].Velocity = velocity;
				particles[firstFreeParticle * 4 + i].Random = randomValues;
				particles[firstFreeParticle * 4 + i].Time = currentTime;
			}

			firstFreeParticle = nextFreeParticle;
		}

		#endregion
	}
}
