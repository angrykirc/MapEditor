using System;
using System.IO;
using System.Text;

using OpenNoxLibrary.Files;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class AbilityRewardXfer : DefaultXfer
	{
		// The string does not contain null terminator - it is added by the game when loading the map
		// Game will cancel loading Xfer if string is longer than 128 bytes
		public string AbilityName;
		
		public AbilityRewardXfer()
		{
			AbilityName = "ABILITY_INVALID";
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			var br = new BinaryReader(mstream);
            AbilityName = Encoding.ASCII.GetString(br.ReadBytes(br.ReadByte()));
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			byte[] result = new byte[AbilityName.Length + 1];
			result[0] = (byte) AbilityName.Length;
			byte[] str = Encoding.ASCII.GetBytes(AbilityName);
			Array.Copy(str, 0, result, 1, str.Length);
			mstream.Write(result, 0, result.Length);
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
