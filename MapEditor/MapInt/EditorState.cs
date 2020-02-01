using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor.MapInt
{
    /// <summary>
    /// Encapsulates all user-input data that is related to altering and removing map elements, as well as current editor mode
    /// </summary>
    public class EditorState
    {
        // Singleton
        public static EditorState Instance = new EditorState();

        protected EditorState()
        {
            CurrentMode = EditMode.OBJECT_SELECT;
        }

        /// <summary>
        /// Current editor mode, dictates whenether features for specific map elements are visible/accessible
        /// </summary>
        public EditMode CurrentMode;

        /// <summary>
        /// Wall prototype built by WallMakeTab UI
        /// </summary>
        public EditableNoxMap.Wall Wall;
    }

    public enum EditMode
    {
        WALL_PLACE,
        WALL_BRUSH,
        WALL_CHANGE, // WallProperties
        FLOOR_PLACE,
        FLOOR_BRUSH,
        EDGE_PLACE,
        OBJECT_PLACE,
        OBJECT_SELECT,
        WAYPOINT_PLACE,
        WAYPOINT_SELECT,
        WAYPOINT_CONNECT,
        POLYGON_RESHAPE
    };
}
