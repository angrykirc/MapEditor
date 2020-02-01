using System;
using System.IO;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Xfer
{
	[Serializable]
	public class TriggerXfer : DefaultXfer
	{
		public int SizeX;
		public int SizeY;
		public Color24 EdgeColor;
		public Color24 BackColor;
		public int UnkInt1;
		public string ScriptOnPressed;
		public string ScriptOnReleased;
		public string ScriptOnCollided;
		public int AllowedObjClass;
		public int IgnoredObjClass;
		public byte AllowedTeamID; // разрешить ТОЛЬКО этой команде
		public byte IgnoredTeamID; // разрешить ВСЕМ КРОМЕ этой команды
		public byte TriggerState;
		public byte Unk7;
		public int UnkInt2;
		
		public TriggerXfer()
		{
			ScriptOnPressed = "";
			ScriptOnReleased = "";
			ScriptOnCollided = "";
			SizeX = 1;
			SizeY = 1;
			EdgeColor = new Color24(90, 90, 90);
			BackColor = new Color24(10, 10, 10);
			AllowedObjClass = 6;
		}
		
		public override bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			NoxBinaryReader br = new NoxBinaryReader(mstream);
			// collisionbox
			SizeX = br.ReadInt32();
			SizeY = br.ReadInt32();
			if (SizeX > 60) SizeX = 60;
			if (SizeY > 60) SizeY = 60;
			// цвета заливки и граней для PressurePlate
			EdgeColor = br.ReadColor();
			BackColor = br.ReadColor();
			UnkInt1 = br.ReadInt32();
			// обработчики событий
			ScriptOnPressed = br.ReadScriptEventString();
			ScriptOnReleased = br.ReadScriptEventString();
			ScriptOnCollided = br.ReadScriptEventString();
			// кто может активировать
			AllowedObjClass = br.ReadInt32();
			IgnoredObjClass = br.ReadInt32();
			AllowedTeamID = br.ReadByte();
			IgnoredTeamID = br.ReadByte();
			if (ParsingRule >= 61)
			{
				TriggerState = br.ReadByte();
				Unk7 = br.ReadByte();
				UnkInt2 = br.ReadInt32();
			}
			return true;
		}
		
		public override void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			NoxBinaryWriter bw = new NoxBinaryWriter(mstream, CryptApi.NoxCryptFormat.NONE);
			
			bw.Write(SizeX);
			bw.Write(SizeY);
			bw.WriteColor(EdgeColor);
			bw.WriteColor(BackColor);
			bw.Write(UnkInt1);
			bw.WriteScriptEvent(ScriptOnPressed);
			bw.WriteScriptEvent(ScriptOnReleased);
			bw.WriteScriptEvent(ScriptOnCollided);
			bw.Write(AllowedObjClass);
			bw.Write(IgnoredObjClass);
			bw.Write(AllowedTeamID);
			bw.Write(IgnoredTeamID);
			bw.Write(TriggerState);
			bw.Write(Unk7);
			bw.Write(UnkInt2);
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
