using System;
using System.IO;
using System.Text;

using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	/// <summary>
	/// Description of FieldGuideXfer.
	/// </summary>
	[Serializable]
	public class FieldGuideXfer : DefaultXfer
	{
		// The string does not contain null terminator - it is added by the game when loading the map
		// Game will cancel loading Xfer if string is longer than 128 bytes
		public string MonsterThingType;
		
		public FieldGuideXfer()
		{
			MonsterThingType = "Bat";
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			BinaryReader br = new BinaryReader(mstream);
            MonsterThingType = Encoding.ASCII.GetString(br.ReadBytes(br.ReadByte()));
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			byte[] result = new byte[MonsterThingType.Length + 1];
			result[0] = (byte) MonsterThingType.Length;
			byte[] str = Encoding.ASCII.GetBytes(MonsterThingType);
			Array.Copy(str, 0, result, 1, str.Length);
			mstream.Write(result, 0, result.Length);
		}
	}
}
