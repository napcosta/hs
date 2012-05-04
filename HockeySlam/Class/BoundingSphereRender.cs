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

namespace HockeySlam
{
	class BoundingSphereRender
	{
		public struct VertexPositionColorNormal
		{
			public Vector3 Position;
			public Color Color;
			public Vector3 Normal;

			public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
			(
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
				new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
			);
		}

		static VertexBuffer vertBuffer;
		static BasicEffect effect;
		static int sphereResolution;

		/// <summary>
		/// Initializes the graphics objects for rendering the spheres. If this method isn't
		/// run manually, it will be called the first time you render a sphere.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device to use when rendering.</param>
		/// <param name="sphereResolution">The number of line segments
		///     to use for each of the three circles.</param>
		public static void InitializeGraphics(GraphicsDevice graphicsDevice, int sphereResolution)
		{
			BoundingSphereRender.sphereResolution = sphereResolution;

			//vertDecl = new VertexDeclaration(
			effect = new BasicEffect(graphicsDevice);
			effect.LightingEnabled = false;
			effect.VertexColorEnabled = false;

			VertexPositionColor[] verts = new VertexPositionColor[(sphereResolution + 1) * 3];

			int index = 0;

			float step = MathHelper.TwoPi / (float)sphereResolution;

			//create the loop on the XY plane first
			for (float a = 0f; a <= MathHelper.TwoPi; a += step)
			{
				verts[index++] = new VertexPositionColor(
					new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f),
					Color.White);
			}

			//next on the XZ plane
			for (float a = 0f; a <= MathHelper.TwoPi; a += step)
			{
				verts[index++] = new VertexPositionColor(
					new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a)),
					Color.White);
			}

			//finally on the YZ plane
			for (float a = 0f; a <= MathHelper.TwoPi; a += step)
			{
				verts[index++] = new VertexPositionColor(
					new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a)),
					Color.White);
			}

			vertBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), verts.Length, BufferUsage.None);
			vertBuffer.SetData(verts);
		}

		/// <summary>
		/// Renders a bounding sphere using different colors for each axis.
		/// </summary>
		/// <param name="sphere">The sphere to render.</param>
		/// <param name="graphicsDevice">The graphics device to use when rendering.</param>
		/// <param name="view">The current view matrix.</param>
		/// <param name="projection">The current projection matrix.</param>
		/// <param name="xyColor">The color for the XY circle.</param>
		/// <param name="xzColor">The color for the XZ circle.</param>
		/// <param name="yzColor">The color for the YZ circle.</param>
		public static void Render(
			BoundingSphere sphere,
			GraphicsDevice graphicsDevice,
			Matrix view,
			Matrix projection,
			Color xyColor,
			Color xzColor,
			Color yzColor)
		{
			if (vertBuffer == null)
				InitializeGraphics(graphicsDevice, 30);

			graphicsDevice.SetVertexBuffer(vertBuffer);

			effect.World =
				Matrix.CreateScale(sphere.Radius) *
				Matrix.CreateTranslation(sphere.Center);
			effect.View = view;
			effect.Projection = projection;
			effect.DiffuseColor = xyColor.ToVector3();

			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				//render each circle individually
				graphicsDevice.DrawPrimitives(
					  PrimitiveType.LineStrip,
					  0,
					  sphereResolution);
				pass.Apply();
				effect.DiffuseColor = xzColor.ToVector3();
				graphicsDevice.DrawPrimitives(
					  PrimitiveType.LineStrip,
					  sphereResolution + 1,
					  sphereResolution);
				pass.Apply();
				effect.DiffuseColor = yzColor.ToVector3();
				graphicsDevice.DrawPrimitives(
					  PrimitiveType.LineStrip,
					  (sphereResolution + 1) * 2,
					  sphereResolution);
				pass.Apply();

			}

		}

		public static void Render(BoundingSphere[] spheres,
		   GraphicsDevice graphicsDevice,
		   Matrix view,
		   Matrix projection,
		   Color xyColor,
			Color xzColor,
			Color yzColor)
		{
			foreach (BoundingSphere sphere in spheres)
			{
				Render(sphere, graphicsDevice, view, projection, xyColor, xzColor, yzColor);
			}
		}

		public static void Render(BoundingSphere[] spheres,
			GraphicsDevice graphicsDevice,
			Matrix view,
			Matrix projection,
			Color color)
		{
			foreach (BoundingSphere sphere in spheres)
			{
				Render(sphere, graphicsDevice, view, projection, color);
			}
		}

		/// <summary>
		/// Renders a bounding sphere using a single color for all three axis.
		/// </summary>
		/// <param name="sphere">The sphere to render.</param>
		/// <param name="graphicsDevice">The graphics device to use when rendering.</param>
		/// <param name="view">The current view matrix.</param>
		/// <param name="projection">The current projection matrix.</param>
		/// <param name="color">The color to use for rendering the circles.</param>
		public static void Render(
			BoundingSphere sphere,
			GraphicsDevice graphicsDevice,
			Matrix view,
			Matrix projection,
			Color color)
		{
			if (vertBuffer == null)
				InitializeGraphics(graphicsDevice, 30);

			graphicsDevice.SetVertexBuffer(vertBuffer);

			effect.World =
				  Matrix.CreateScale(sphere.Radius) *
				  Matrix.CreateTranslation(sphere.Center);
			effect.View = view;
			effect.Projection = projection;
			effect.DiffuseColor = color.ToVector3();

			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				//render each circle individually
				graphicsDevice.DrawPrimitives(
					  PrimitiveType.LineStrip,
					  0,
					  sphereResolution);
				graphicsDevice.DrawPrimitives(
					  PrimitiveType.LineStrip,
					  sphereResolution + 1,
					  sphereResolution);
				graphicsDevice.DrawPrimitives(
					  PrimitiveType.LineStrip,
					  (sphereResolution + 1) * 2,
					  sphereResolution);

			}

		}

		public static void Render(BoundingBox box, GraphicsDevice graphicsDevice, Matrix view, Matrix projection, Color color)
		{
			VertexPositionColorNormal[] boxVertices = new VertexPositionColorNormal[8];
			Vector3[] corners  = box.GetCorners();

			for (int i = 0; i < 8; i++) {
				boxVertices[i].Position = corners[i];
				boxVertices[i].Color = Color.White;
				boxVertices[i].Normal = Vector3.Up;
			}

			/*
			Vector3 vert0 = box.Min;
			Vector3 vert1 = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
			Vector3 vert2 = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
			Vector3 vert3 = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
			Vector3 vert4 = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
			Vector3 vert5 = box.Max;
			Vector3 vert6 = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
			Vector3 vert7 = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);

			//front face
			boxVertices[0] = new VertexPositionColorNormal();
			boxVertices[0].Position = vert0;
			boxVertices[0].Color = color;
			boxVertices[0].Normal = Vector3.Backward;
			boxVertices[1] = new VertexPositionColorNormal();
			boxVertices[1].Position = vert1;
			boxVertices[1].Color = color;
			boxVertices[1].Normal = Vector3.Backward;
			boxVertices[2] = new VertexPositionColorNormal();
			boxVertices[2].Position = vert6;
			boxVertices[2].Color = color;
			boxVertices[2].Normal = Vector3.Backward;
			boxVertices[3] = new VertexPositionColorNormal();
			boxVertices[3].Position = vert7;
			boxVertices[3].Color = color;
			boxVertices[3].Normal = Vector3.Backward;

			//left face
			boxVertices[4] = new VertexPositionColorNormal();
			boxVertices[4].Position = vert3;
			boxVertices[4].Color = color;
			boxVertices[4].Normal = Vector3.Left;
			boxVertices[5] = new VertexPositionColorNormal();
			boxVertices[5].Position = vert0;
			boxVertices[5].Color = color;
			boxVertices[5].Normal = Vector3.Left;
			boxVertices[6] = new VertexPositionColorNormal();
			boxVertices[6].Position = vert7;
			boxVertices[6].Color = color;
			boxVertices[6].Normal = Vector3.Left;
			boxVertices[7] = new VertexPositionColorNormal();
			boxVertices[7].Position = vert4;
			boxVertices[7].Color = color;
			boxVertices[7].Normal = Vector3.Left;

			//back face
			boxVertices[8] = new VertexPositionColorNormal();
			boxVertices[8].Position = vert2;
			boxVertices[8].Color = color;
			boxVertices[8].Normal = Vector3.Forward;
			boxVertices[9] = new VertexPositionColorNormal();
			boxVertices[9].Position = vert3;
			boxVertices[9].Color = color;
			boxVertices[9].Normal = Vector3.Forward;
			boxVertices[10] = new VertexPositionColorNormal();
			boxVertices[10].Position = vert4;
			boxVertices[10].Color = color;
			boxVertices[10].Normal = Vector3.Forward;
			boxVertices[11] = new VertexPositionColorNormal();
			boxVertices[11].Position = vert5;
			boxVertices[11].Color = color;
			boxVertices[11].Normal = Vector3.Forward;

			//right face
			boxVertices[12] = new VertexPositionColorNormal();
			boxVertices[12].Position = vert1;
			boxVertices[12].Color = color;
			boxVertices[12].Normal = Vector3.Right;
			boxVertices[13] = new VertexPositionColorNormal();
			boxVertices[13].Position = vert3;
			boxVertices[13].Color = color;
			boxVertices[13].Normal = Vector3.Right;
			boxVertices[14] = new VertexPositionColorNormal();
			boxVertices[14].Position = vert5;
			boxVertices[14].Color = color;
			boxVertices[14].Normal = Vector3.Right;
			boxVertices[15] = new VertexPositionColorNormal();
			boxVertices[15].Position = vert6;
			boxVertices[15].Color = color;
			boxVertices[15].Normal = Vector3.Right;*/

			short[] boxLines = new short[] {
				0, 1, 
				1, 2,   
				2, 3,  
				3, 0,  
				0, 4,  
				1, 5,  
				2, 6,  
				3, 7,  
				4, 5,  
				5, 6,  
				6, 7,  
				7, 4 
			};

			VertexBuffer buffer = new VertexBuffer(
				graphicsDevice, 
				VertexPositionColorNormal.VertexDeclaration, 
				boxVertices.Length, 
				BufferUsage.WriteOnly);

			buffer.SetData(boxVertices);

			IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), boxLines.Length, BufferUsage.WriteOnly);

			indexBuffer.SetData(boxLines);

			graphicsDevice.Indices = indexBuffer;
			graphicsDevice.SetVertexBuffer(buffer);

			effect.World = Matrix.Identity;
			effect.View = view;
			effect.Projection = projection;
			effect.DiffuseColor = color.ToVector3();

			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, boxVertices.Length, 0, boxLines.Length / 2);
			}
			
		}

	}
}
