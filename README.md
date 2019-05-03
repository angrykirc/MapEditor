# MapEditor
Nox Map Editor 1.10 by KITTY

Added Copy Map features:
 - Copy Area mode: select an area on the map to copy
 - Copy All: copy everything on the map
 - Paste Area: place copied area anywhere on current/new/existing map (no limit)
 - Can specify which elements get pasted
 - (credit to panic and Schizz for the idea)
 
Added Map Image tab (replacing Export Image)
 - Render the current map into preview
 - Crop option: auto crops to outer edges of map
 - Change resolution (exports actual size specified)
 - Show/Hide specific Objects/Entities
 - Compression on saved images vastly improved
 - (took some functionality from NoxMapRender, credit to IllidanS4)
 
Added Tile Paint Bucket tool (not perfect)
 - Fill in area enclosed by walls or tiles
 - Right click to bucket delete
 
Added Wall Paint Bucket tool
 - Makes walls around selected tile group
 - (most funtionality from MapGenerator, credit to AngryKirC)
 
Added 'Color Special Walls' setting

Added Window property to Wall Change mode
 - Can now make all walls windows
 - Shades both preview modes
 - Forces 'Color Special Walls' while mode is active
 
Added 'Recent' menu item: open previous maps
 - Will open new window if visible changes on current map
 
Added Quick Find tool to Shop Editor
 - Single click to set text
 - Double click to force text
 
Added 'Select All' waypoints button (replaced broken 'Enable All')
 - Can move just like Objects
 
Added menu for AngryKirC's 'Map Generator'
 - Added options to change tiles/walls/randomize/abort
 
Added mass editing feature to View Objects for: Script Name, Enchants 1-4 (+validation)

Added 'Import + Save' option: CTRL + S to reimport latest script(.obj) and save map

Added preview to Equipment Enchantments

Added 'Map Saved' indicator

Added option to 'Auto Increment' map version every save (+0.01)

Added 'Export Native Script' option: saves all Strings/Functions as .txt file

Added 'Help' menu with links

Added Quick Menu button for Settings

Added all Voices to NPC Edit and Maiden/Wounded Edit

Added icons for a few buttons

Groups Window:
 - Map Groups work again!
 - UI now a bit more intuitive
 - Extent validation on Save
 
Scripting Window:
 - UI now resizes better, moved Help text to bottom
 - Added Color Syntax toggle
 - Added Nox Script 3.0 indicators
 - Renamed Function now updates correctly in TreeView
 - Added Show Help toggle
 - Added Color Theme toggle
 - Fixed local variables clearing when editing GLOBAL
 - Added 25 missing script function descriptions
 
Mini Map tab:
 - Fixed Divide1/Divide2 for different zooms
 - Bumped Mini Map zoom to minimum of 1 and maximum of 7
 
 Can use Escape key to cancel temporary modes:
 - Picker, Bucket, Line Draw, Rect Draw, Smart Draw, Copy/Paste Area, and Wall Change
 
Other:
 - Map Info tab now has proper character limits
 - Cleaned up Settings UI
 - Undo Clipboard now holds 50 changes (up from 15)
 - Disabled waypoints and double way path colors more distinguishable
 - Only warns of closing Editor if visible changes are made
 - Moved some menu items around (File, Map, Options, Help)
 - Changed Fast Preview walls from BlackWall to MagicWallSystemUseOnly (less ambiguious)
 - Window size and maximized mode saved

File Changes:
 - Latest.log file now only saves in MapEditor.exe directory
 - BlankMap.map no longer needed in MapEditor.exe directory
 - Categories.xml file is generated if it doesn't exist
 - 'noxscript' directory + contents generated if it doesn't exist
 - 'functiondesc' directory + contents generated if it doesn't exist

Bug Fixes:
 - Weapons/Armor now default to standard Arena durability (used to crash map)
 - Fixed 'Show 3D Extents' setting not toggling
 - 'Export Script' handles errors a bit better
 - Fixed Maiden/Wounded voice bug not allowing change
 - Replaced main menu toolbar with MenuStrip, old one was limited and buggy
 - Waypoint/Object text drawing causing huge render delays; replaced TextRenderer with DrawString
 - Fixed object values(xfer) not saving on deletion -> Undo
 - Many other small bug fixes and code cleanup
