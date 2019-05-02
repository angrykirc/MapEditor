namespace MapEditor
{
    partial class GroupDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.saveButton = new System.Windows.Forms.Button();
            this.groupBoxTypes = new System.Windows.Forms.GroupBox();
            this.waypointRadio = new System.Windows.Forms.RadioButton();
            this.wallRadio = new System.Windows.Forms.RadioButton();
            this.objectRadio = new System.Windows.Forms.RadioButton();
            this.txtGroupID = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.delButton = new System.Windows.Forms.Button();
            this.txtExtents = new System.Windows.Forms.TextBox();
            this.newButton = new System.Windows.Forms.Button();
            this.lstGroups = new System.Windows.Forms.ListBox();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.lblHelpStatus = new System.Windows.Forms.Label();
            this.groupBoxTypes.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(281, 399);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // groupBoxTypes
            // 
            this.groupBoxTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTypes.Controls.Add(this.waypointRadio);
            this.groupBoxTypes.Controls.Add(this.wallRadio);
            this.groupBoxTypes.Controls.Add(this.objectRadio);
            this.groupBoxTypes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxTypes.Location = new System.Drawing.Point(245, 5);
            this.groupBoxTypes.Name = "groupBoxTypes";
            this.groupBoxTypes.Size = new System.Drawing.Size(200, 39);
            this.groupBoxTypes.TabIndex = 3;
            this.groupBoxTypes.TabStop = false;
            this.groupBoxTypes.Text = "Type: ";
            // 
            // waypointRadio
            // 
            this.waypointRadio.AutoSize = true;
            this.waypointRadio.Location = new System.Drawing.Point(119, 14);
            this.waypointRadio.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.waypointRadio.Name = "waypointRadio";
            this.waypointRadio.Size = new System.Drawing.Size(75, 17);
            this.waypointRadio.TabIndex = 2;
            this.waypointRadio.Text = "Waypoints";
            this.waypointRadio.CheckedChanged += new System.EventHandler(this.waypointRadio_CheckedChanged);
            // 
            // wallRadio
            // 
            this.wallRadio.AutoSize = true;
            this.wallRadio.Location = new System.Drawing.Point(68, 14);
            this.wallRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.wallRadio.Name = "wallRadio";
            this.wallRadio.Size = new System.Drawing.Size(51, 17);
            this.wallRadio.TabIndex = 1;
            this.wallRadio.Text = "Walls";
            this.wallRadio.CheckedChanged += new System.EventHandler(this.wallRadio_CheckedChanged);
            // 
            // objectRadio
            // 
            this.objectRadio.AutoSize = true;
            this.objectRadio.Location = new System.Drawing.Point(7, 14);
            this.objectRadio.Name = "objectRadio";
            this.objectRadio.Size = new System.Drawing.Size(61, 17);
            this.objectRadio.TabIndex = 0;
            this.objectRadio.Text = "Objects";
            this.objectRadio.CheckedChanged += new System.EventHandler(this.objectRadio_CheckedChanged);
            // 
            // txtGroupID
            // 
            this.txtGroupID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGroupID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGroupID.Enabled = false;
            this.txtGroupID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGroupID.Location = new System.Drawing.Point(206, 22);
            this.txtGroupID.Name = "txtGroupID";
            this.txtGroupID.Size = new System.Drawing.Size(33, 22);
            this.txtGroupID.TabIndex = 5;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(363, 399);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Cancel";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // delButton
            // 
            this.delButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.delButton.Location = new System.Drawing.Point(93, 398);
            this.delButton.Name = "delButton";
            this.delButton.Size = new System.Drawing.Size(75, 23);
            this.delButton.TabIndex = 6;
            this.delButton.Text = "Delete";
            this.delButton.Click += new System.EventHandler(this.delButton_Click);
            // 
            // txtExtents
            // 
            this.txtExtents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExtents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtExtents.Font = new System.Drawing.Font("Lucida Console", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExtents.Location = new System.Drawing.Point(245, 48);
            this.txtExtents.Multiline = true;
            this.txtExtents.Name = "txtExtents";
            this.txtExtents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExtents.Size = new System.Drawing.Size(199, 336);
            this.txtExtents.TabIndex = 7;
            this.txtExtents.TextChanged += new System.EventHandler(this.txtExtents_TextChanged);
            // 
            // newButton
            // 
            this.newButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.newButton.Location = new System.Drawing.Point(12, 398);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(75, 23);
            this.newButton.TabIndex = 8;
            this.newButton.Text = "New";
            this.newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // lstGroups
            // 
            this.lstGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstGroups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstGroups.FormattingEnabled = true;
            this.lstGroups.IntegralHeight = false;
            this.lstGroups.ItemHeight = 16;
            this.lstGroups.Location = new System.Drawing.Point(19, 46);
            this.lstGroups.Name = "lstGroups";
            this.lstGroups.Size = new System.Drawing.Size(220, 338);
            this.lstGroups.TabIndex = 9;
            this.lstGroups.SelectedIndexChanged += new System.EventHandler(this.lstGroups_SelectedIndexChanged);
            // 
            // txtGroupName
            // 
            this.txtGroupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGroupName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGroupName.Location = new System.Drawing.Point(19, 22);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(188, 22);
            this.txtGroupName.TabIndex = 10;
            this.txtGroupName.TextChanged += new System.EventHandler(this.txtGroupName_TextChanged);
            this.txtGroupName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtGroupName_KeyDown);
            // 
            // lblHelpStatus
            // 
            this.lblHelpStatus.AutoSize = true;
            this.lblHelpStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHelpStatus.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.lblHelpStatus.Location = new System.Drawing.Point(17, 5);
            this.lblHelpStatus.Name = "lblHelpStatus";
            this.lblHelpStatus.Size = new System.Drawing.Size(0, 15);
            this.lblHelpStatus.TabIndex = 11;
            // 
            // GroupDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(456, 433);
            this.Controls.Add(this.txtGroupName);
            this.Controls.Add(this.newButton);
            this.Controls.Add(this.txtExtents);
            this.Controls.Add(this.delButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.txtGroupID);
            this.Controls.Add(this.groupBoxTypes);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.lstGroups);
            this.Controls.Add(this.lblHelpStatus);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximumSize = new System.Drawing.Size(800, 800);
            this.Name = "GroupDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Groups";
            this.TopMost = true;
            this.groupBoxTypes.ResumeLayout(false);
            this.groupBoxTypes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.GroupBox groupBoxTypes;
        private System.Windows.Forms.RadioButton objectRadio;
		private System.Windows.Forms.RadioButton wallRadio;
        private System.Windows.Forms.TextBox txtGroupID;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.RadioButton waypointRadio;
		private System.Windows.Forms.Button delButton;
		private System.Windows.Forms.TextBox txtExtents;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.ListBox lstGroups;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Label lblHelpStatus;
    }
}