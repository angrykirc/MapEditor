using System;
using System.IO;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class ToxicCloudXfer : DefaultXfer
	{
		public int Lifetime;
		
		public ToxicCloudXfer()
		{
			Lifetime = 1;
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			byte[] tmp = new byte[4];
			mstream.Read(tmp, 0, 4);
			Lifetime = BitConverter.ToInt32(tmp, 0);
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			mstream.Write(BitConverter.GetBytes(Lifetime), 0, 4);
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
