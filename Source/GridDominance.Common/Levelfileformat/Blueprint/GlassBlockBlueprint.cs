﻿using System.IO;

namespace GridDominance.Levelfileformat.Blueprint
{
	public sealed class GlassBlockBlueprint
	{
		public const float DEFAULT_WIDTH = 24; // TILE_WIDTH * 0.333f

		public const float REFRACTION_INDEX = 1.5f;

		public readonly float X; // center
		public readonly float Y;
		public readonly float Width;
		public readonly float Height;
		public readonly float Rotation; // in degree

		public GlassBlockBlueprint(float x, float y, float w, float h, float r)
		{
			X = x;
			Y = y;
			Width = w;
			Height = h;
			Rotation = r;
		}

		public void Serialize(BinaryWriter bw)
		{
			bw.Write(LevelBlueprint.SERIALIZE_ID_GLASSBLOCK);
			bw.Write(X);
			bw.Write(Y);
			bw.Write(Width);
			bw.Write(Height);
			bw.Write(Rotation);
		}

		public static GlassBlockBlueprint Deserialize(BinaryReader br)
		{
			var x = br.ReadSingle();
			var y = br.ReadSingle();
			var w = br.ReadSingle();
			var h = br.ReadSingle();
			var r = br.ReadSingle();

			return new GlassBlockBlueprint(x, y, w, h, r);
		}
	}
}
