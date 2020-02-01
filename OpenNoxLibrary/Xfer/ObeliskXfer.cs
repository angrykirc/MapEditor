using System;
using System.IO;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class ObeliskXfer : DefaultXfer
	{
		public int ManaPool; // 50 normally
		private byte Unused;
		
		public ObeliskXfer()
		{
			ManaPool = 50;
			Unused = 0;
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			BinaryReader rdr = new BinaryReader(mstream);
			if (ParsingRule >= 0x3D)
			{
				ManaPool = rdr.ReadInt32();
				Unused = rdr.ReadByte();
			}
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			BinaryWriter bw = new BinaryWriter(mstream);
			bw.Write(ManaPool);
			bw.Write(Unused);
		}
		
		public override short MaxVersion 
		{
			get
			{ 
				return 0x3d;
			}
		}
	}
}
