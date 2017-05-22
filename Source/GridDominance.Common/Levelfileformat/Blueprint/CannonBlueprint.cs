﻿using System;
using System.IO;

namespace GridDominance.Levelfileformat.Blueprint
{
	public sealed class CannonBlueprint
	{
		public readonly float X; // center
		public readonly float Y;
		public readonly float Diameter;
		public readonly int Player;
		public readonly float Rotation; // in degree
		public readonly int CannonID;

		public BulletPathBlueprint[] PrecalculatedPaths;

		public CannonBlueprint(float x, float y, float d, int p, float rot, int cid, BulletPathBlueprint[] bp)
		{
			X = x;
			Y = y;
			Diameter = d;
			Player = p;
			CannonID = cid;
			PrecalculatedPaths = bp;

			if (rot < 0)
				Rotation = (float) (Math.Atan2(320 - Y, 512 - X) / Math.PI * 180);
			else
				Rotation = rot;
		}

		public CannonBlueprint(float x, float y, float d, int p, float rot, int cid)
		{
			X = x;
			Y = y;
			Diameter = d;
			Player = p;
			CannonID = cid;
			PrecalculatedPaths = new BulletPathBlueprint[0];

			if (rot < 0)
				Rotation = (float)(Math.Atan2(320 - Y, 512 - X) / Math.PI * 180);
			else
				Rotation = rot;
		}

		public void Serialize(BinaryWriter bw)
		{
			bw.Write(LevelBlueprint.SERIALIZE_ID_CANNON);
			bw.Write(CannonID);
			bw.Write(Player);
			bw.Write(X);
			bw.Write(Y);
			bw.Write(Diameter);
			bw.Write(Rotation);

			bw.Write((short)PrecalculatedPaths.Length);
			foreach (var path in PrecalculatedPaths) path.Serialize(bw);
		}

		public static CannonBlueprint Deserialize(BinaryReader br)
		{
			var i = br.ReadInt32();
			var p = br.ReadInt32();
			var x = br.ReadSingle();
			var y = br.ReadSingle();
			var d = br.ReadSingle();
			var a = br.ReadSingle();

			var pathCount = br.ReadInt16();
			var b = new BulletPathBlueprint[pathCount];
			for (int j = 0; j < pathCount; j++) b[j] = BulletPathBlueprint.Deserialize(br);

			return new CannonBlueprint(x, y, d, p, a, i, b);
		}
	}
}