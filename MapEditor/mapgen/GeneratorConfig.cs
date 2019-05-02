/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 12.02.2015
 */
using System;

namespace MapEditor.mapgen
{
	/// <summary>
	/// Description of GeneratorConfig.
	/// </summary>
	public class GeneratorConfig
	{
		public enum MapPreset : byte
		{
			Crossroads = 1,
			Dungeons = 2
		}
		
		public int RandomSeed;
		public MapPreset MapType;
		public bool Allow3SideWalls;
		public bool PopulateMap;
        public bool Randomize;
        public string BASE_FLOOR;
        public string DENSE_FLOOR;
        public string PATH_FLOOR;
        public string BLEND_EDGE;
        public string WALL;
	}
}
