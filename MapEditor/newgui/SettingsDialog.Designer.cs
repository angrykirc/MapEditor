/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 15.04.2015
 */
namespace MapEditor.newgui
{
	partial class SettingsDialog
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.checkBoxExt3d = new System.Windows.Forms.CheckBox();
            this.checkBoxScriptNames = new System.Windows.Forms.CheckBox();
            this.checkBoxThingNames = new System.Windows.Forms.CheckBox();
            this.checkBoxTexEdges = new System.Windows.Forms.CheckBox();
            this.checkBoxTileGrid = new System.Windows.Forms.CheckBox();
            this.checkBoxObjects = new System.Windows.Forms.CheckBox();
            this.checkBoxTiles = new System.Windows.Forms.CheckBox();
            this.checkBoxPolygons = new System.Windows.Forms.CheckBox();
            this.checkBoxWaypoints = new System.Windows.Forms.CheckBox();
            this.checkBoxWalls = new System.Windows.Forms.CheckBox();
            this.groupBoxMapfile = new System.Windows.Forms.GroupBox();
            this.checkBoxAllowOver = new System.Windows.Forms.CheckBox();
            this.checkBoxProtect = new System.Windows.Forms.CheckBox();
            this.checkBoxSaveNXZ = new System.Windows.Forms.CheckBox();
            this.checkBoxSaveScripts = new System.Windows.Forms.CheckBox();
            this.buttonDone = new System.Windows.Forms.Button();
            this.checkBoxComplexPrev = new System.Windows.Forms.CheckBox();
            this.checkBoxObjFacing = new System.Windows.Forms.CheckBox();
            this.checkBoxObjectLabels = new System.Windows.Forms.CheckBox();
            this.groupBoxObjLbl = new System.Windows.Forms.GroupBox();
            this.checkBoxLabelTeams = new System.Windows.Forms.CheckBox();
            this.groupBoxElems = new System.Windows.Forms.GroupBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxMinimap = new System.Windows.Forms.GroupBox();
            this.checkBoxMinimapFade = new System.Windows.Forms.CheckBox();
            this.checkBoxMinimapHide = new System.Windows.Forms.CheckBox();
            this.checkBoxTeleports = new System.Windows.Forms.CheckBox();
            this.groupBoxMapView = new System.Windows.Forms.GroupBox();
            this.checkBoxMinimapShow = new System.Windows.Forms.CheckBox();
            this.checkBoxColorWalls = new System.Windows.Forms.CheckBox();
            this.groupBoxMapfile.SuspendLayout();
            this.groupBoxObjLbl.SuspendLayout();
            this.groupBoxElems.SuspendLayout();
            this.groupBoxMinimap.SuspendLayout();
            this.groupBoxMapView.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxExt3d
            // 
            this.checkBoxExt3d.AutoSize = true;
            this.checkBoxExt3d.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxExt3d.Location = new System.Drawing.Point(15, 25);
            this.checkBoxExt3d.Name = "checkBoxExt3d";
            this.checkBoxExt3d.Size = new System.Drawing.Size(133, 17);
            this.checkBoxExt3d.TabIndex = 10;
            this.checkBoxExt3d.Text = "3D Object Extents (F8)";
            this.checkBoxExt3d.UseVisualStyleBackColor = true;
            // 
            // checkBoxScriptNames
            // 
            this.checkBoxScriptNames.AutoSize = true;
            this.checkBoxScriptNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxScriptNames.Location = new System.Drawing.Point(18, 44);
            this.checkBoxScriptNames.Name = "checkBoxScriptNames";
            this.checkBoxScriptNames.Size = new System.Drawing.Size(97, 17);
            this.checkBoxScriptNames.TabIndex = 9;
            this.checkBoxScriptNames.Text = "Custom Names";
            this.checkBoxScriptNames.UseVisualStyleBackColor = true;
            // 
            // checkBoxThingNames
            // 
            this.checkBoxThingNames.AutoSize = true;
            this.checkBoxThingNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxThingNames.Location = new System.Drawing.Point(18, 24);
            this.checkBoxThingNames.Name = "checkBoxThingNames";
            this.checkBoxThingNames.Size = new System.Drawing.Size(85, 17);
            this.checkBoxThingNames.TabIndex = 8;
            this.checkBoxThingNames.Text = "Thing Types";
            this.checkBoxThingNames.UseVisualStyleBackColor = true;
            // 
            // checkBoxTexEdges
            // 
            this.checkBoxTexEdges.AutoSize = true;
            this.checkBoxTexEdges.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxTexEdges.Location = new System.Drawing.Point(15, 65);
            this.checkBoxTexEdges.Name = "checkBoxTexEdges";
            this.checkBoxTexEdges.Size = new System.Drawing.Size(146, 17);
            this.checkBoxTexEdges.TabIndex = 7;
            this.checkBoxTexEdges.Text = "Render Edges in Preview";
            this.checkBoxTexEdges.UseVisualStyleBackColor = true;
            // 
            // checkBoxTileGrid
            // 
            this.checkBoxTileGrid.AutoSize = true;
            this.checkBoxTileGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxTileGrid.Location = new System.Drawing.Point(16, 125);
            this.checkBoxTileGrid.Name = "checkBoxTileGrid";
            this.checkBoxTileGrid.Size = new System.Drawing.Size(66, 17);
            this.checkBoxTileGrid.TabIndex = 1;
            this.checkBoxTileGrid.Text = "Grid (F5)";
            this.checkBoxTileGrid.UseVisualStyleBackColor = true;
            // 
            // checkBoxObjects
            // 
            this.checkBoxObjects.AutoSize = true;
            this.checkBoxObjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxObjects.Location = new System.Drawing.Point(16, 105);
            this.checkBoxObjects.Name = "checkBoxObjects";
            this.checkBoxObjects.Size = new System.Drawing.Size(83, 17);
            this.checkBoxObjects.TabIndex = 6;
            this.checkBoxObjects.Text = "Objects (F7)";
            this.checkBoxObjects.UseVisualStyleBackColor = true;
            // 
            // checkBoxTiles
            // 
            this.checkBoxTiles.AutoSize = true;
            this.checkBoxTiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxTiles.Location = new System.Drawing.Point(16, 85);
            this.checkBoxTiles.Name = "checkBoxTiles";
            this.checkBoxTiles.Size = new System.Drawing.Size(48, 17);
            this.checkBoxTiles.TabIndex = 5;
            this.checkBoxTiles.Text = "Tiles";
            this.checkBoxTiles.UseVisualStyleBackColor = true;
            // 
            // checkBoxPolygons
            // 
            this.checkBoxPolygons.AutoSize = true;
            this.checkBoxPolygons.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxPolygons.Location = new System.Drawing.Point(16, 65);
            this.checkBoxPolygons.Name = "checkBoxPolygons";
            this.checkBoxPolygons.Size = new System.Drawing.Size(69, 17);
            this.checkBoxPolygons.TabIndex = 4;
            this.checkBoxPolygons.Text = "Polygons";
            this.checkBoxPolygons.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaypoints
            // 
            this.checkBoxWaypoints.AutoSize = true;
            this.checkBoxWaypoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxWaypoints.Location = new System.Drawing.Point(16, 45);
            this.checkBoxWaypoints.Name = "checkBoxWaypoints";
            this.checkBoxWaypoints.Size = new System.Drawing.Size(103, 17);
            this.checkBoxWaypoints.TabIndex = 3;
            this.checkBoxWaypoints.Text = "Waypoints (F10)";
            this.checkBoxWaypoints.UseVisualStyleBackColor = true;
            // 
            // checkBoxWalls
            // 
            this.checkBoxWalls.AutoSize = true;
            this.checkBoxWalls.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxWalls.Location = new System.Drawing.Point(16, 25);
            this.checkBoxWalls.Name = "checkBoxWalls";
            this.checkBoxWalls.Size = new System.Drawing.Size(73, 17);
            this.checkBoxWalls.TabIndex = 2;
            this.checkBoxWalls.Text = "Walls (F6)";
            this.checkBoxWalls.UseVisualStyleBackColor = true;
            // 
            // groupBoxMapfile
            // 
            this.groupBoxMapfile.Controls.Add(this.checkBoxAllowOver);
            this.groupBoxMapfile.Controls.Add(this.checkBoxProtect);
            this.groupBoxMapfile.Controls.Add(this.checkBoxSaveNXZ);
            this.groupBoxMapfile.Controls.Add(this.checkBoxSaveScripts);
            this.groupBoxMapfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxMapfile.Location = new System.Drawing.Point(19, 275);
            this.groupBoxMapfile.Name = "groupBoxMapfile";
            this.groupBoxMapfile.Size = new System.Drawing.Size(191, 112);
            this.groupBoxMapfile.TabIndex = 1;
            this.groupBoxMapfile.TabStop = false;
            this.groupBoxMapfile.Text = "General";
            // 
            // checkBoxAllowOver
            // 
            this.checkBoxAllowOver.AutoSize = true;
            this.checkBoxAllowOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxAllowOver.Location = new System.Drawing.Point(15, 85);
            this.checkBoxAllowOver.Name = "checkBoxAllowOver";
            this.checkBoxAllowOver.Size = new System.Drawing.Size(127, 17);
            this.checkBoxAllowOver.TabIndex = 0;
            this.checkBoxAllowOver.Text = "Auto Override Ts/Ws";
            this.checkBoxAllowOver.UseVisualStyleBackColor = true;
            // 
            // checkBoxProtect
            // 
            this.checkBoxProtect.AutoSize = true;
            this.checkBoxProtect.Enabled = false;
            this.checkBoxProtect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxProtect.Location = new System.Drawing.Point(15, 65);
            this.checkBoxProtect.Name = "checkBoxProtect";
            this.checkBoxProtect.Size = new System.Drawing.Size(84, 17);
            this.checkBoxProtect.TabIndex = 13;
            this.checkBoxProtect.Text = "Protect Map";
            this.checkBoxProtect.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveNXZ
            // 
            this.checkBoxSaveNXZ.AutoSize = true;
            this.checkBoxSaveNXZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSaveNXZ.Location = new System.Drawing.Point(15, 25);
            this.checkBoxSaveNXZ.Name = "checkBoxSaveNXZ";
            this.checkBoxSaveNXZ.Size = new System.Drawing.Size(81, 17);
            this.checkBoxSaveNXZ.TabIndex = 11;
            this.checkBoxSaveNXZ.Text = "Make .NXZ";
            this.checkBoxSaveNXZ.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveScripts
            // 
            this.checkBoxSaveScripts.AutoSize = true;
            this.checkBoxSaveScripts.Enabled = false;
            this.checkBoxSaveScripts.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSaveScripts.Location = new System.Drawing.Point(15, 45);
            this.checkBoxSaveScripts.Name = "checkBoxSaveScripts";
            this.checkBoxSaveScripts.Size = new System.Drawing.Size(84, 17);
            this.checkBoxSaveScripts.TabIndex = 12;
            this.checkBoxSaveScripts.Text = "Save scripts";
            this.checkBoxSaveScripts.UseVisualStyleBackColor = true;
            // 
            // buttonDone
            // 
            this.buttonDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDone.Location = new System.Drawing.Point(244, 297);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(80, 35);
            this.buttonDone.TabIndex = 0;
            this.buttonDone.Text = "Apply";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.ButtonKClick);
            // 
            // checkBoxComplexPrev
            // 
            this.checkBoxComplexPrev.AutoSize = true;
            this.checkBoxComplexPrev.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxComplexPrev.Location = new System.Drawing.Point(15, 85);
            this.checkBoxComplexPrev.Name = "checkBoxComplexPrev";
            this.checkBoxComplexPrev.Size = new System.Drawing.Size(157, 17);
            this.checkBoxComplexPrev.TabIndex = 12;
            this.checkBoxComplexPrev.Text = "Color NPC/Items in Preview";
            this.checkBoxComplexPrev.UseVisualStyleBackColor = true;
            // 
            // checkBoxObjFacing
            // 
            this.checkBoxObjFacing.AutoSize = true;
            this.checkBoxObjFacing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxObjFacing.Location = new System.Drawing.Point(15, 45);
            this.checkBoxObjFacing.Name = "checkBoxObjFacing";
            this.checkBoxObjFacing.Size = new System.Drawing.Size(122, 17);
            this.checkBoxObjFacing.TabIndex = 11;
            this.checkBoxObjFacing.Text = "Show Object Facing";
            this.checkBoxObjFacing.UseVisualStyleBackColor = true;
            // 
            // checkBoxObjectLabels
            // 
            this.checkBoxObjectLabels.AutoSize = true;
            this.checkBoxObjectLabels.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxObjectLabels.Location = new System.Drawing.Point(10, 179);
            this.checkBoxObjectLabels.Name = "checkBoxObjectLabels";
            this.checkBoxObjectLabels.Size = new System.Drawing.Size(15, 14);
            this.checkBoxObjectLabels.TabIndex = 3;
            this.checkBoxObjectLabels.UseVisualStyleBackColor = true;
            this.checkBoxObjectLabels.CheckedChanged += new System.EventHandler(this.CheckBoxEnTextCheckedChanged);
            // 
            // groupBoxObjLbl
            // 
            this.groupBoxObjLbl.Controls.Add(this.checkBoxLabelTeams);
            this.groupBoxObjLbl.Controls.Add(this.checkBoxThingNames);
            this.groupBoxObjLbl.Controls.Add(this.checkBoxScriptNames);
            this.groupBoxObjLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxObjLbl.Location = new System.Drawing.Point(19, 177);
            this.groupBoxObjLbl.Name = "groupBoxObjLbl";
            this.groupBoxObjLbl.Size = new System.Drawing.Size(191, 92);
            this.groupBoxObjLbl.TabIndex = 2;
            this.groupBoxObjLbl.TabStop = false;
            this.groupBoxObjLbl.Text = "Object Labels";
            // 
            // checkBoxLabelTeams
            // 
            this.checkBoxLabelTeams.AutoSize = true;
            this.checkBoxLabelTeams.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxLabelTeams.Location = new System.Drawing.Point(18, 64);
            this.checkBoxLabelTeams.Name = "checkBoxLabelTeams";
            this.checkBoxLabelTeams.Size = new System.Drawing.Size(97, 17);
            this.checkBoxLabelTeams.TabIndex = 10;
            this.checkBoxLabelTeams.Text = "Existing Teams";
            this.checkBoxLabelTeams.UseVisualStyleBackColor = true;
            // 
            // groupBoxElems
            // 
            this.groupBoxElems.Controls.Add(this.checkBoxWalls);
            this.groupBoxElems.Controls.Add(this.checkBoxWaypoints);
            this.groupBoxElems.Controls.Add(this.checkBoxPolygons);
            this.groupBoxElems.Controls.Add(this.checkBoxTiles);
            this.groupBoxElems.Controls.Add(this.checkBoxTileGrid);
            this.groupBoxElems.Controls.Add(this.checkBoxObjects);
            this.groupBoxElems.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxElems.Location = new System.Drawing.Point(216, 12);
            this.groupBoxElems.Name = "groupBoxElems";
            this.groupBoxElems.Size = new System.Drawing.Size(145, 159);
            this.groupBoxElems.TabIndex = 1;
            this.groupBoxElems.TabStop = false;
            this.groupBoxElems.Text = "Draw Elements";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(244, 334);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 35);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // groupBoxMinimap
            // 
            this.groupBoxMinimap.Controls.Add(this.checkBoxMinimapFade);
            this.groupBoxMinimap.Controls.Add(this.checkBoxMinimapHide);
            this.groupBoxMinimap.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxMinimap.Location = new System.Drawing.Point(216, 177);
            this.groupBoxMinimap.Name = "groupBoxMinimap";
            this.groupBoxMinimap.Size = new System.Drawing.Size(145, 92);
            this.groupBoxMinimap.TabIndex = 11;
            this.groupBoxMinimap.TabStop = false;
            this.groupBoxMinimap.Text = "Minimap";
            // 
            // checkBoxMinimapFade
            // 
            this.checkBoxMinimapFade.AutoSize = true;
            this.checkBoxMinimapFade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxMinimapFade.Location = new System.Drawing.Point(16, 45);
            this.checkBoxMinimapFade.Name = "checkBoxMinimapFade";
            this.checkBoxMinimapFade.Size = new System.Drawing.Size(75, 17);
            this.checkBoxMinimapFade.TabIndex = 10;
            this.checkBoxMinimapFade.Text = "Auto Fade";
            this.checkBoxMinimapFade.UseVisualStyleBackColor = true;
            // 
            // checkBoxMinimapHide
            // 
            this.checkBoxMinimapHide.AutoSize = true;
            this.checkBoxMinimapHide.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxMinimapHide.Location = new System.Drawing.Point(16, 25);
            this.checkBoxMinimapHide.Name = "checkBoxMinimapHide";
            this.checkBoxMinimapHide.Size = new System.Drawing.Size(73, 17);
            this.checkBoxMinimapHide.TabIndex = 9;
            this.checkBoxMinimapHide.Text = "Auto Hide";
            this.checkBoxMinimapHide.UseVisualStyleBackColor = true;
            // 
            // checkBoxTeleports
            // 
            this.checkBoxTeleports.AutoSize = true;
            this.checkBoxTeleports.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxTeleports.Location = new System.Drawing.Point(15, 125);
            this.checkBoxTeleports.Name = "checkBoxTeleports";
            this.checkBoxTeleports.Size = new System.Drawing.Size(146, 17);
            this.checkBoxTeleports.TabIndex = 13;
            this.checkBoxTeleports.Text = "Show Teleport Ways (F9)";
            this.checkBoxTeleports.UseVisualStyleBackColor = true;
            // 
            // groupBoxMapView
            // 
            this.groupBoxMapView.Controls.Add(this.checkBoxColorWalls);
            this.groupBoxMapView.Controls.Add(this.checkBoxTeleports);
            this.groupBoxMapView.Controls.Add(this.checkBoxExt3d);
            this.groupBoxMapView.Controls.Add(this.checkBoxTexEdges);
            this.groupBoxMapView.Controls.Add(this.checkBoxComplexPrev);
            this.groupBoxMapView.Controls.Add(this.checkBoxObjFacing);
            this.groupBoxMapView.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxMapView.Location = new System.Drawing.Point(19, 12);
            this.groupBoxMapView.Name = "groupBoxMapView";
            this.groupBoxMapView.Size = new System.Drawing.Size(191, 159);
            this.groupBoxMapView.TabIndex = 4;
            this.groupBoxMapView.TabStop = false;
            this.groupBoxMapView.Text = "Map View";
            // 
            // checkBoxMinimapShow
            // 
            this.checkBoxMinimapShow.AutoSize = true;
            this.checkBoxMinimapShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxMinimapShow.Location = new System.Drawing.Point(207, 180);
            this.checkBoxMinimapShow.Name = "checkBoxMinimapShow";
            this.checkBoxMinimapShow.Size = new System.Drawing.Size(15, 14);
            this.checkBoxMinimapShow.TabIndex = 12;
            this.checkBoxMinimapShow.UseVisualStyleBackColor = true;
            this.checkBoxMinimapShow.CheckedChanged += new System.EventHandler(this.checkBoxMinimapShow_CheckedChanged);
            // 
            // checkBoxColorWalls
            // 
            this.checkBoxColorWalls.AutoSize = true;
            this.checkBoxColorWalls.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxColorWalls.Location = new System.Drawing.Point(15, 105);
            this.checkBoxColorWalls.Name = "checkBoxColorWalls";
            this.checkBoxColorWalls.Size = new System.Drawing.Size(117, 17);
            this.checkBoxColorWalls.TabIndex = 14;
            this.checkBoxColorWalls.Text = "Color Special Walls";
            this.checkBoxColorWalls.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 401);
            this.Controls.Add(this.checkBoxMinimapShow);
            this.Controls.Add(this.checkBoxObjectLabels);
            this.Controls.Add(this.groupBoxMinimap);
            this.Controls.Add(this.groupBoxMapView);
            this.Controls.Add(this.groupBoxObjLbl);
            this.Controls.Add(this.groupBoxMapfile);
            this.Controls.Add(this.groupBoxElems);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonDone);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Map Editor Settings";
            this.groupBoxMapfile.ResumeLayout(false);
            this.groupBoxMapfile.PerformLayout();
            this.groupBoxObjLbl.ResumeLayout(false);
            this.groupBoxObjLbl.PerformLayout();
            this.groupBoxElems.ResumeLayout(false);
            this.groupBoxElems.PerformLayout();
            this.groupBoxMinimap.ResumeLayout(false);
            this.groupBoxMinimap.PerformLayout();
            this.groupBoxMapView.ResumeLayout(false);
            this.groupBoxMapView.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.CheckBox checkBoxProtect;
		private System.Windows.Forms.CheckBox checkBoxExt3d;
		private System.Windows.Forms.Button buttonDone;
		private System.Windows.Forms.CheckBox checkBoxSaveScripts;
		private System.Windows.Forms.CheckBox checkBoxSaveNXZ;
		private System.Windows.Forms.GroupBox groupBoxMapfile;
		private System.Windows.Forms.CheckBox checkBoxThingNames;
		private System.Windows.Forms.CheckBox checkBoxScriptNames;
		private System.Windows.Forms.CheckBox checkBoxTexEdges;
		private System.Windows.Forms.CheckBox checkBoxObjects;
		private System.Windows.Forms.CheckBox checkBoxWaypoints;
		private System.Windows.Forms.CheckBox checkBoxPolygons;
		private System.Windows.Forms.CheckBox checkBoxTiles;
		private System.Windows.Forms.CheckBox checkBoxWalls;
		private System.Windows.Forms.CheckBox checkBoxTileGrid;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.GroupBox groupBoxElems;
		private System.Windows.Forms.CheckBox checkBoxObjectLabels;
		private System.Windows.Forms.GroupBox groupBoxObjLbl;
        private System.Windows.Forms.CheckBox checkBoxObjFacing;
		private System.Windows.Forms.CheckBox checkBoxLabelTeams;
        private System.Windows.Forms.CheckBox checkBoxAllowOver;
		private System.Windows.Forms.CheckBox checkBoxComplexPrev;
        private System.Windows.Forms.GroupBox groupBoxMinimap;
        private System.Windows.Forms.CheckBox checkBoxMinimapFade;
        private System.Windows.Forms.CheckBox checkBoxMinimapHide;
        private System.Windows.Forms.CheckBox checkBoxTeleports;
        private System.Windows.Forms.GroupBox groupBoxMapView;
        private System.Windows.Forms.CheckBox checkBoxMinimapShow;
        private System.Windows.Forms.CheckBox checkBoxColorWalls;
    }
}
