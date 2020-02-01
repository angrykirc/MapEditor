using System;
using System.IO;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class SpellPagePedestalXfer : DefaultXfer
	{
		public int SpellId;
		
		public SpellPagePedestalXfer()
		{
			SpellId = 1;
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			byte[] tmp = new byte[4];
			mstream.Read(tmp, 0, 4);
			SpellId = BitConverter.ToInt32(tmp, 0);
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			mstream.Write(BitConverter.GetBytes(SpellId), 0, 4);
		}
	}
}
