using System;
using System.IO;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class DoorXfer : DefaultXfer
	{
		public DOORS_DIR Direction;
		public DOORS_LOCK LockType;
		
        // 0-24
		public enum DOORS_DIR : int
		{
			South = 0,
			North = 0x10,
			East = 0x18,
			West = 0x08
		}

		public enum DOORS_LOCK : int
		{
			None = 0,
			Silver = 1,
			Gold = 2,
			Ruby = 3,
			Saphire = 4,
			Mechanism = 5
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			BinaryReader rdr = new BinaryReader(mstream);
			rdr.ReadInt32(); // current direction
			LockType = (DOORS_LOCK) rdr.ReadInt32();
			Direction = (DOORS_DIR) rdr.ReadInt32();
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			BinaryWriter bw = new BinaryWriter(mstream);
			bw.Write((int) Direction);
			bw.Write((int) LockType);
			bw.Write((int) Direction);
		}
	}
}
