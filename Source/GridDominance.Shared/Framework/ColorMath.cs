﻿using Microsoft.Xna.Framework;

namespace GridDominance.Shared.Framework
{
	static class ColorMath
	{
		public static Color Darken(this Color c, float perc = 0.5f)
		{
			return new Color(
				perc*(c.R / 255f),
				perc*(c.G / 255f),
				perc*(c.B / 255f),
				c.A);
		}

		public static Color Lighten(this Color c, float perc = 0.5f)
		{
			return new Color(
				1f - perc*(1f - c.R/255f),
				1f - perc*(1f - c.G/255f),
				1f - perc*(1f - c.B/255f),
				c.A);
		}

		public static Color Blend(Color a, Color b, float perc)
		{
			var cr = (1 - perc)*a.R + perc*b.R;
			var cg = (1 - perc)*a.G + perc*b.G;
			var cb = (1 - perc)*a.B + perc*b.B;

			return new Color((int)cr, (int)cg, (int)cb);
		}
	}
}