﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoSAMFramework.Portable.BatchRenderer;
using MonoSAMFramework.Portable.GameMath;
using MonoSAMFramework.Portable.GameMath.Geometry;

namespace MonoSAMFramework.Portable.RenderHelper
{
	public static class SimpleRenderHelper
	{
		public const int CROP_CORNER_SIZE = 16;
		public const int TEX_CORNER_SIZE = 24;

		public static void DrawRoundedRect(IBatchRenderer sbatch, FRectangle bounds, Color color, float cornerScale = 1)
		{
			DrawRoundedRect(sbatch, bounds, color, true, true, true, true, cornerScale);
		}
		
		public static void DrawRoundedRect(IBatchRenderer sbatch, FRectangle bounds, Color color, bool tl, bool tr, bool bl, bool br, float cornerScale = 1)
		{
			StaticTextures.ThrowIfNotInitialized();

			if (!tl && !tr && !bl && !br)
			{
				DrawSimpleRect(sbatch, bounds, color);

				return;
			}

			#region Fill Center

			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				bounds.AsDeflated(CROP_CORNER_SIZE * cornerScale, 0).Round(),
				StaticTextures.SinglePixel.Bounds,
				color);

			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				bounds.AsDeflated(0, CROP_CORNER_SIZE * cornerScale).Round(),
				StaticTextures.SinglePixel.Bounds,
				color);

			#endregion

			var cornerBounds = new FRectangle(0, 0, TEX_CORNER_SIZE * cornerScale, TEX_CORNER_SIZE * cornerScale);

			#region Corners

			if (tl)
			{
				sbatch.Draw(
					StaticTextures.PanelCorner.Texture,
					bounds.VectorTopLeft,
					StaticTextures.PanelCorner.Bounds,
					color,
					0 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					cornerScale * StaticTextures.DEFAULT_TEXTURE_SCALE,
					SpriteEffects.None, 0);
			}
			else
			{
				sbatch.Draw(
					StaticTextures.SinglePixel.Texture,
					cornerBounds.AsTranslated(bounds.VectorTopLeft),
					StaticTextures.SinglePixel.Bounds,
					color,
					0 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					SpriteEffects.None, 0);
			}

			if (tr)
			{
				sbatch.Draw(
					StaticTextures.PanelCorner.Texture,
					bounds.VectorTopRight,
					StaticTextures.PanelCorner.Bounds,
					color,
					90 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					cornerScale * StaticTextures.DEFAULT_TEXTURE_SCALE,
					SpriteEffects.None, 0);
			}
			else
			{
				sbatch.Draw(
					StaticTextures.SinglePixel.Texture,
					cornerBounds.AsTranslated(bounds.VectorTopRight),
					StaticTextures.SinglePixel.Bounds,
					color,
					90 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					SpriteEffects.None, 0);
			}

			if (br)
			{
				sbatch.Draw(
					StaticTextures.PanelCorner.Texture,
					bounds.VectorBottomRight,
					StaticTextures.PanelCorner.Bounds,
					color,
					180 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					cornerScale * StaticTextures.DEFAULT_TEXTURE_SCALE,
					SpriteEffects.None, 0);
			}
			else
			{
				sbatch.Draw(
					StaticTextures.SinglePixel.Texture,
					cornerBounds.AsTranslated(bounds.VectorBottomRight),
					StaticTextures.SinglePixel.Bounds,
					color,
					180 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					SpriteEffects.None, 0);
			}

			if (bl)
			{
				sbatch.Draw(
					StaticTextures.PanelCorner.Texture,
					bounds.VectorBottomLeft,
					StaticTextures.PanelCorner.Bounds,
					color,
					270 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					cornerScale * StaticTextures.DEFAULT_TEXTURE_SCALE,
					SpriteEffects.None, 0);
			}
			else
			{
				sbatch.Draw(
					StaticTextures.SinglePixel.Texture,
					cornerBounds.AsTranslated(bounds.VectorBottomLeft),
					StaticTextures.SinglePixel.Bounds,
					color,
					270 * FloatMath.DegreesToRadians,
					Vector2.Zero,
					SpriteEffects.None, 0);
			}

			#endregion
		}

		public static void DrawSimpleRect(IBatchRenderer sbatch, FRectangle bounds, Color color)
		{
			StaticTextures.ThrowIfNotInitialized();

			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				bounds.Round(),
				StaticTextures.SinglePixel.Bounds,
				color);
		}

		public static void DrawSimpleRectOutline(IBatchRenderer sbatch, FRectangle bounds, float bsize, Color color)
		{
			StaticTextures.ThrowIfNotInitialized();

			int borderSize = FloatMath.Round(bsize);

			// LEFT
			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				new Rectangle(FloatMath.Round(bounds.Left), FloatMath.Ceiling(bounds.Top), borderSize, FloatMath.Floor(bounds.Height)),
				StaticTextures.SinglePixel.Bounds,
				color);

			// TOP
			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				new Rectangle(FloatMath.Ceiling(bounds.Left), FloatMath.Round(bounds.Top), FloatMath.Floor(bounds.Width), borderSize),
				StaticTextures.SinglePixel.Bounds,
				color);

			// RIGHT
			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				new Rectangle(FloatMath.Round(bounds.Right) - borderSize, FloatMath.Ceiling(bounds.Top), borderSize, FloatMath.Floor(bounds.Height)),
				StaticTextures.SinglePixel.Bounds,
				color);

			// BOTTOM
			sbatch.Draw(
				StaticTextures.SinglePixel.Texture,
				new Rectangle(FloatMath.Ceiling(bounds.Left), FloatMath.Round(bounds.Bottom) - borderSize, FloatMath.Floor(bounds.Width), borderSize),
				StaticTextures.SinglePixel.Bounds,
				color);
		}
	}
}
