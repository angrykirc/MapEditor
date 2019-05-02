using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace MapEditor
{
	public class AboutDialog : Form
    {
		private IContainer components = null;

		public AboutDialog()
		{
			InitializeComponent();
            lblVersion.Text = string.Format("Nox Map Editor {0} by:", GetVersion());
		}

        // ** Change version number under Project > MapEditor Properties > Application (tab) > Assembly Information (button) > File Version **
        public static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblAbout = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(12, 14);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(241, 25);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Nox Map Editor x.y by:";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblAbout
            // 
            this.lblAbout.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAbout.Location = new System.Drawing.Point(12, 45);
            this.lblAbout.Name = "lblAbout";
            this.lblAbout.Size = new System.Drawing.Size(241, 142);
            this.lblAbout.TabIndex = 2;
            this.lblAbout.Text = "Eric Litak\r\nAndrew Wesie\r\nTemplar\r\nJoshua Statzer\r\nAngryKirC\r\nSuperJack\r\nProtokol" +
    "\r\nKITTY";
            this.lblAbout.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // AboutDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(265, 193);
            this.Controls.Add(this.lblAbout);
            this.Controls.Add(this.lblVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.ResumeLayout(false);

		}

        private Label lblAbout;
        private Label lblVersion;
		#endregion

	}
}
