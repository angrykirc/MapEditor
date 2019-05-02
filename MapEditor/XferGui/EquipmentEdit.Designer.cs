/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 30.06.2015
 */
namespace MapEditor.XferGui
{
	partial class EquipmentEdit
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
            this.buttonDone = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.durability = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.enchantment1 = new System.Windows.Forms.ComboBox();
            this.enchantment2 = new System.Windows.Forms.ComboBox();
            this.enchantment3 = new System.Windows.Forms.ComboBox();
            this.enchantment4 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ammoMin = new System.Windows.Forms.NumericUpDown();
            this.ammoMax = new System.Windows.Forms.NumericUpDown();
            this.picRender = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.picRenderOrig = new System.Windows.Forms.PictureBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRandomize = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.durability)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ammoMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ammoMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRender)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRenderOrig)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonDone
            // 
            this.buttonDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDone.Location = new System.Drawing.Point(59, 243);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(76, 32);
            this.buttonDone.TabIndex = 0;
            this.buttonDone.Text = "Save";
            this.buttonDone.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 144);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Durability:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // durability
            // 
            this.durability.Enabled = false;
            this.durability.Location = new System.Drawing.Point(32, 168);
            this.durability.Maximum = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            this.durability.Name = "durability";
            this.durability.Size = new System.Drawing.Size(104, 20);
            this.durability.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(24, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Enchantments:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // enchantment1
            // 
            this.enchantment1.FormattingEnabled = true;
            this.enchantment1.Location = new System.Drawing.Point(24, 32);
            this.enchantment1.Name = "enchantment1";
            this.enchantment1.Size = new System.Drawing.Size(121, 21);
            this.enchantment1.TabIndex = 4;
            this.enchantment1.SelectedIndexChanged += new System.EventHandler(this.enchantment1_SelectedIndexChanged);
            // 
            // enchantment2
            // 
            this.enchantment2.FormattingEnabled = true;
            this.enchantment2.Location = new System.Drawing.Point(24, 56);
            this.enchantment2.Name = "enchantment2";
            this.enchantment2.Size = new System.Drawing.Size(121, 21);
            this.enchantment2.TabIndex = 5;
            this.enchantment2.SelectedIndexChanged += new System.EventHandler(this.enchantment2_SelectedIndexChanged);
            // 
            // enchantment3
            // 
            this.enchantment3.FormattingEnabled = true;
            this.enchantment3.Location = new System.Drawing.Point(24, 80);
            this.enchantment3.Name = "enchantment3";
            this.enchantment3.Size = new System.Drawing.Size(121, 21);
            this.enchantment3.TabIndex = 6;
            this.enchantment3.SelectedIndexChanged += new System.EventHandler(this.enchantment3_SelectedIndexChanged);
            // 
            // enchantment4
            // 
            this.enchantment4.FormattingEnabled = true;
            this.enchantment4.Location = new System.Drawing.Point(24, 104);
            this.enchantment4.Name = "enchantment4";
            this.enchantment4.Size = new System.Drawing.Size(121, 21);
            this.enchantment4.TabIndex = 7;
            this.enchantment4.SelectedIndexChanged += new System.EventHandler(this.enchantment4_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(24, 186);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 23);
            this.label3.TabIndex = 8;
            this.label3.Text = "Charges/Arrows:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // ammoMin
            // 
            this.ammoMin.Enabled = false;
            this.ammoMin.Location = new System.Drawing.Point(32, 213);
            this.ammoMin.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ammoMin.Name = "ammoMin";
            this.ammoMin.Size = new System.Drawing.Size(48, 20);
            this.ammoMin.TabIndex = 9;
            // 
            // ammoMax
            // 
            this.ammoMax.Enabled = false;
            this.ammoMax.Location = new System.Drawing.Point(88, 213);
            this.ammoMax.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ammoMax.Name = "ammoMax";
            this.ammoMax.Size = new System.Drawing.Size(48, 20);
            this.ammoMax.TabIndex = 10;
            // 
            // picRender
            // 
            this.picRender.BackColor = System.Drawing.Color.Black;
            this.picRender.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picRender.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picRender.Location = new System.Drawing.Point(158, 34);
            this.picRender.Name = "picRender";
            this.picRender.Size = new System.Drawing.Size(100, 100);
            this.picRender.TabIndex = 11;
            this.picRender.TabStop = false;
            this.picRender.Click += new System.EventHandler(this.picRender_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(158, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 12;
            this.label4.Text = "Preview:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picRenderOrig
            // 
            this.picRenderOrig.BackColor = System.Drawing.Color.Black;
            this.picRenderOrig.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picRenderOrig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picRenderOrig.Location = new System.Drawing.Point(158, 133);
            this.picRenderOrig.Name = "picRenderOrig";
            this.picRenderOrig.Size = new System.Drawing.Size(100, 100);
            this.picRenderOrig.TabIndex = 13;
            this.picRenderOrig.TabStop = false;
            this.picRenderOrig.Click += new System.EventHandler(this.picRenderOrig_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(141, 243);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 32);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonRandomize
            // 
            this.buttonRandomize.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRandomize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRandomize.Location = new System.Drawing.Point(24, 125);
            this.buttonRandomize.Name = "buttonRandomize";
            this.buttonRandomize.Size = new System.Drawing.Size(121, 21);
            this.buttonRandomize.TabIndex = 15;
            this.buttonRandomize.Text = "Randomize";
            this.buttonRandomize.UseVisualStyleBackColor = true;
            this.buttonRandomize.Click += new System.EventHandler(this.buttonRandomize_Click);
            // 
            // EquipmentEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 281);
            this.Controls.Add(this.buttonRandomize);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.picRenderOrig);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.picRender);
            this.Controls.Add(this.ammoMax);
            this.Controls.Add(this.ammoMin);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.enchantment4);
            this.Controls.Add(this.enchantment3);
            this.Controls.Add(this.enchantment2);
            this.Controls.Add(this.enchantment1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.durability);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDone);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EquipmentEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Equipment";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EquipmentEdit_FormClosing);
            this.Load += new System.EventHandler(this.EquipmentEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.durability)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ammoMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ammoMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRender)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRenderOrig)).EndInit();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.NumericUpDown ammoMax;
		private System.Windows.Forms.NumericUpDown ammoMin;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox enchantment4;
		private System.Windows.Forms.ComboBox enchantment3;
		private System.Windows.Forms.ComboBox enchantment2;
		private System.Windows.Forms.ComboBox enchantment1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown durability;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.PictureBox picRender;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox picRenderOrig;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonRandomize;
    }
}
