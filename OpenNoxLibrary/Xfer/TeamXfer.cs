using System;
using System.IO;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class TeamXfer : DefaultXfer
	{
		// these never have C null terminators
		public string[] Enchantments;
		
		public TeamXfer()
		{
			// One of these is normally MaterialTeam$
			Enchantments = new string[4] { "", "" , "", "" };
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			NoxBinaryReader br = new NoxBinaryReader(mstream);
			// 4 enchantments
			for (int i = 0; i < 4; i++) Enchantments[i] = br.ReadString();
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			NoxBinaryWriter bw = new NoxBinaryWriter(mstream, CryptApi.NoxCryptFormat.NONE);
			
			for (int i = 0; i < 4; i++) bw.Write(Enchantments[i]);
		}
	}
}
