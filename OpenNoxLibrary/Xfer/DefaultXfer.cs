/*
 * NoxShared
 * Пользователь: AngryKirC
 * Дата: 29.06.2015
 */
using System;
using System.IO;
using System.Collections.Generic;

using OpenNoxLibrary.Files;

namespace OpenNoxLibrary.Xfer
{
	/// <summary>
	/// Default container for a transferred map object data.
	/// </summary>
	[Serializable]
	public class DefaultXfer : ICloneable
	{
		/// <summary>
		/// Reads an object's extra data from specfied Stream
		/// </summary>
		public virtual bool FromStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			return false;
		}
		
		/// <summary>
		/// Writes an object's extra data to specfied Stream
		/// </summary>
		public virtual void WriteToStream(Stream mstream, short ParsingRule, ThingDb.Thing thing)
		{
			;
		}
		
		/// <summary>
		/// This value will be used for writing Xfer data upon map saving (ReadRule1)
		/// </summary>
		public virtual short MaxVersion
		{
			get
			{
				return 0x3c; // 60
			}
		}
		
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}

	public static class ObjectXferProvider
	{
		private static Dictionary<string, Type> Providers;
		
		/// <summary>
		/// Returns ObjectDataXfer implementation for specified Nox type
		/// </summary>
		public static DefaultXfer Get(string xferName)
		{
			if (xferName != null) // null = DefaultXfer
			{
				if (Providers.ContainsKey(xferName))
				{
					return (DefaultXfer) Activator.CreateInstance(Providers[xferName]);
				}
			}
			// Not found, assume DefaultXfer
			return new DefaultXfer();
		}
		
		static ObjectXferProvider()
		{
			Providers = new Dictionary<string, Type>();
			/* -- Here goes list of known Xfer providers -- */
			// DefaultXfer...
			Providers.Add("SpellPagePedestalXfer", typeof(SpellPagePedestalXfer));
			Providers.Add("SpellRewardXfer", typeof(SpellRewardXfer));
			Providers.Add("AbilityRewardXfer", typeof(AbilityRewardXfer));
			Providers.Add("FieldGuideXfer", typeof(FieldGuideXfer));
			Providers.Add("ReadableXfer", typeof(ReadableXfer));
			Providers.Add("ExitXfer", typeof(ExitXfer));
			Providers.Add("DoorXfer", typeof(DoorXfer));
			Providers.Add("TriggerXfer", typeof(TriggerXfer));
			Providers.Add("MonsterXfer", typeof(MonsterXfer));
			Providers.Add("HoleXfer", typeof(HoleXfer));
			Providers.Add("TransporterXfer", typeof(TransporterXfer));
			Providers.Add("ElevatorXfer", typeof(ElevatorXfer));
			Providers.Add("ElevatorShaftXfer", typeof(ElevatorXfer));
			Providers.Add("MoverXfer", typeof(MoverXfer));
			Providers.Add("GlyphXfer", typeof(GlyphXfer));
			Providers.Add("InvisibleLightXfer", typeof(InvisibleLightXfer));
			Providers.Add("SentryXfer", typeof(SentryXfer));
			Providers.Add("WeaponXfer", typeof(WeaponXfer));
			Providers.Add("ArmorXfer", typeof(ArmorXfer));
			Providers.Add("TeamXfer", typeof(TeamXfer));
			Providers.Add("GoldXfer", typeof(GoldXfer));
			Providers.Add("AmmoXfer", typeof(AmmoXfer));
			Providers.Add("NPCXfer", typeof(NPCXfer));
			Providers.Add("ObeliskXfer", typeof(ObeliskXfer));
			Providers.Add("ToxicCloudXfer", typeof(ToxicCloudXfer));
			Providers.Add("MonsterGeneratorXfer", typeof(MonsterGeneratorXfer));
			Providers.Add("RewardMarkerXfer", typeof(RewardMarkerXfer));
		}
	}
}
