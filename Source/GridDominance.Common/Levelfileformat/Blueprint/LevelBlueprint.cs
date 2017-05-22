﻿using System;
using System.Collections.Generic;
using System.IO;

namespace GridDominance.Levelfileformat.Blueprint
{
	public sealed class LevelBlueprint
	{
		public const int KI_TYPE_RAYTRACE    = 10; 
		public const int KI_TYPE_PRECALC     = 11;
		public const int KI_TYPE_PRESIMULATE = 12;

		public const byte SERIALIZE_ID_CANNON      = 0x01; 
		public const byte SERIALIZE_ID_NAME        = 0x02; 
		public const byte SERIALIZE_ID_DESCRIPTION = 0x03; 
		public const byte SERIALIZE_ID_GUID        = 0x04;
		public const byte SERIALIZE_ID_VOIDWALL    = 0x05;
		public const byte SERIALIZE_ID_VOIDCIRCLE  = 0x06;
		public const byte SERIALIZE_ID_GLASSBLOCK  = 0x07;
		public const byte SERIALIZE_ID_KITYPE      = 0x08;
		public const byte SERIALIZE_ID_BLACKHOLE   = 0x09;
		public const byte SERIALIZE_ID_EOF         = 0xFF;

		public readonly List<CannonBlueprint>     BlueprintCannons     = new List<CannonBlueprint>();
		public readonly List<VoidWallBlueprint>   BlueprintVoidWalls   = new List<VoidWallBlueprint>();
		public readonly List<VoidCircleBlueprint> BlueprintVoidCircles = new List<VoidCircleBlueprint>();
		public readonly List<GlassBlockBlueprint> BlueprintGlassBlocks = new List<GlassBlockBlueprint>();
		public readonly List<BlackHoleBlueprint>  BlueprintBlackHoles  = new List<BlackHoleBlueprint>();

		public Guid UniqueID { get; set; } = Guid.Empty;
		public string Name { get; set; } = "";
		public string FullName { get; set; } = "";
		public byte KIType { get; set; } = KI_TYPE_RAYTRACE;

		public LevelBlueprint()
		{
			//
		}
		
		public void BinarySerialize(BinaryWriter bw)
		{
			bw.Write(SERIALIZE_ID_NAME);
			bw.Write(Name);

			bw.Write(SERIALIZE_ID_DESCRIPTION);
			bw.Write(FullName);

			bw.Write(SERIALIZE_ID_GUID);
			bw.Write(UniqueID.ToByteArray());

			bw.Write(SERIALIZE_ID_KITYPE);
			bw.Write(KIType);

			for (int i = 0; i < BlueprintCannons.Count;     i++) BlueprintCannons[i].Serialize(bw);
			for (int i = 0; i < BlueprintVoidWalls.Count;   i++) BlueprintVoidWalls[i].Serialize(bw);
			for (int i = 0; i < BlueprintVoidCircles.Count; i++) BlueprintVoidCircles[i].Serialize(bw);
			for (int i = 0; i < BlueprintGlassBlocks.Count; i++) BlueprintGlassBlocks[i].Serialize(bw);
			for (int i = 0; i < BlueprintBlackHoles.Count;  i++) BlueprintBlackHoles[i].Serialize(bw);

			bw.Write(SERIALIZE_ID_EOF);

			bw.Write((byte)0xB1);
			bw.Write((byte)0x6B);
			bw.Write((byte)0x00);
			bw.Write((byte)0xB5);
		}

		public void BinaryDeserialize(BinaryReader br)
		{
			byte[] id = new byte[1];
			while (br.Read(id, 0, 1) > 0)
			{
				switch (id[0])
				{
					case SERIALIZE_ID_CANNON:
						BlueprintCannons.Add(CannonBlueprint.Deserialize(br));
						break;

					case SERIALIZE_ID_VOIDWALL:
						BlueprintVoidWalls.Add(VoidWallBlueprint.Deserialize(br));
						break;

					case SERIALIZE_ID_VOIDCIRCLE:
						BlueprintVoidCircles.Add(VoidCircleBlueprint.Deserialize(br));
						break;

					case SERIALIZE_ID_GLASSBLOCK:
						BlueprintGlassBlocks.Add(GlassBlockBlueprint.Deserialize(br));
						break;

					case SERIALIZE_ID_BLACKHOLE:
						BlueprintBlackHoles.Add(BlackHoleBlueprint.Deserialize(br));
						break;

					case SERIALIZE_ID_NAME:
						Name = br.ReadString();
						break;

					case SERIALIZE_ID_KITYPE:
						KIType = br.ReadByte();
						break;

					case SERIALIZE_ID_DESCRIPTION:
						FullName = br.ReadString();
						break;

					case SERIALIZE_ID_GUID:
						UniqueID = new Guid(br.ReadBytes(16));
						break;

					case SERIALIZE_ID_EOF:
					{
						if (string.IsNullOrWhiteSpace(Name)) throw new Exception("Level needs a valid name");
						if (UniqueID == Guid.Empty) throw new Exception("Level needs a valid UUID");

						if (br.ReadByte() != 0xB1) throw new Exception("Missing footer byte 1");
						if (br.ReadByte() != 0x6B) throw new Exception("Missing footer byte 2");
						if (br.ReadByte() != 0x00) throw new Exception("Missing footer byte 3");
						if (br.ReadByte() != 0xB5) throw new Exception("Missing footer byte 4");

						return;
					}

					default:
					{
						throw new Exception("Unknown binary ID:" + id[0]);
					}
				}
			}

			throw new Exception("Unexpected binary file end");
		}
		
		public static bool IsIncludeMatch(string a, string b)
		{
			const StringComparison icic = StringComparison.CurrentCultureIgnoreCase;

			if (string.Equals(a, b, icic)) return true;

			if (a.LastIndexOf('.') > 0) a = a.Substring(0, a.LastIndexOf('.'));
			if (b.LastIndexOf('.') > 0) b = b.Substring(0, b.LastIndexOf('.'));

			return string.Equals(a, b, icic);
		}

		public void ValidOrThrow()
		{
			if (string.IsNullOrWhiteSpace(Name))
				throw new Exception("Level needs a valid name");

			if (UniqueID == Guid.Empty)
				throw new Exception("Level needs a valid UUID");
		}
	}
}