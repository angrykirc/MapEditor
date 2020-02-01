﻿using System;
using System.IO;
using System.Text;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class ReadableXfer : DefaultXfer
	{
		public string Text;
		
		public ReadableXfer()
		{
			Text = "_aTest2.map:ShopkeeperDialog";
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			Text = "";
			BinaryReader br = new BinaryReader(mstream);
			int len = br.ReadInt32();
			Text = Encoding.ASCII.GetString(br.ReadBytes(len));
			Text = Text.TrimEnd('\0');
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			BinaryWriter bw = new BinaryWriter(mstream);
			if (!Text.EndsWith("\0")) Text += '\0';
			bw.Write((int) Text.Length);
			bw.Write(Encoding.ASCII.GetBytes(Text));
		}
	}
}