#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using NoxShared;
using SyntaxHighlighter;
using System.Diagnostics;

#endregion

namespace MapEditor
{
    public class ScriptFunctionDialog : Form
    {
        #region Globals
        private string prevname;
        public bool hasLoaded = false;
        private int lastHeight;
        protected List<string> strings;
        protected Map.ScriptFunction sf;
        protected Map.ScriptObject sct;
        private Label lblVariables;
        private Label lblFunctionName;
        private TextBox nameBox;
        private Button cancelButton;
        private ListBox lstMethods;
        private SyntaxRichTextBox scriptBox;
        private Button okButton;
        private TextBox symbBox;
        private CheckBox chkColorSyntax;
        private TreeView scriptTree;
        private CheckBox chkShowHelp;
        private CheckBox chkColorTheme;

        public Map.ScriptObject Scripts
        {
            get
            {
                return sct;
            }
            set
            {
                sct = value;
                if (sct != null)
                {
                    treeNode1.Nodes.Clear();
                    foreach (string s in sct.SctStr)
                        treeNode1.Nodes.Add(s);

                    treeNode2.Nodes.Clear();
                    int i = 0;
                    foreach (Map.ScriptFunction sf in sct.Funcs)
                    {
                        treeNode2.Nodes.Add(string.Format("{0}: {1}", i, sf.name));
                        i++;
                    }
                    scriptTree.Nodes.Add(treeNode1);
                    scriptTree.Nodes.Add(treeNode2);
                }

                if (sct.SctStr.Count > 0 && sct.SctStr[0].StartsWith("NOXSCRIPT3.0"))
                {
                    Text += " - Nox Script 3.0";
                    rtxtDesc.Text = "This code was written with Nox Script 3.0, beware of incorrect syntax.\n\nFor best results, use File -> Export Script.";
                }
            }
        }
        public Map.ScriptFunction ScriptFunc
        {
            get
            {
                return sf;
            }
            set
            {
                sf = value;
                nameBox.Text = sf.name;

                scriptBox.m_bPaint = false;
                scriptBox.Clear();

                try
                {
                    scriptBox.Text = GetCode();
                    if (scriptBox.Settings.ColorSyntax)
                        scriptBox.ProcessBox();
                }
                catch (Exception)
                {
                    scriptBox.m_bPaint = true;
                    scriptBox.Visible = true;
                    return;
                }
                scriptBox.m_bPaint = true;
            }

        }

        public List<string> ScriptStrings
        {
            get { return strings; }
            set { strings = value; }
        }
        protected List<Map.ScriptFunction> funcs;
        public List<Map.ScriptFunction> ScriptFunctions
        {
            get { return funcs; }
            set { funcs = value; }
        }
        #endregion

        #region Windows Form Designer generated code
        private ContextMenuStrip treeMenu;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private TreeNode treeNode1 = new TreeNode("Strings");
        private TreeNode treeNode2 = new TreeNode("Functions");
        private RichTextBox rtxtDesc;
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtxtDesc = new System.Windows.Forms.RichTextBox();
            this.lblVariables = new System.Windows.Forms.Label();
            this.lblFunctionName = new System.Windows.Forms.Label();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.lstMethods = new System.Windows.Forms.ListBox();
            this.okButton = new System.Windows.Forms.Button();
            this.symbBox = new System.Windows.Forms.TextBox();
            this.chkColorSyntax = new System.Windows.Forms.CheckBox();
            this.scriptTree = new System.Windows.Forms.TreeView();
            this.chkShowHelp = new System.Windows.Forms.CheckBox();
            this.chkColorTheme = new System.Windows.Forms.CheckBox();
            this.scriptBox = new SyntaxHighlighter.SyntaxRichTextBox();
            this.treeMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeMenu
            // 
            this.treeMenu.AllowDrop = true;
            this.treeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.treeMenu.Name = "treeMenu";
            this.treeMenu.Size = new System.Drawing.Size(108, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // rtxtDesc
            // 
            this.rtxtDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtxtDesc.BackColor = System.Drawing.SystemColors.Info;
            this.rtxtDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtxtDesc.Location = new System.Drawing.Point(195, 410);
            this.rtxtDesc.Name = "rtxtDesc";
            this.rtxtDesc.ReadOnly = true;
            this.rtxtDesc.Size = new System.Drawing.Size(490, 113);
            this.rtxtDesc.TabIndex = 7;
            this.rtxtDesc.Text = "Check out Nox Script 3.0 for a more user-friendly coding experience.";
            // 
            // lblVariables
            // 
            this.lblVariables.AutoSize = true;
            this.lblVariables.BackColor = System.Drawing.Color.Transparent;
            this.lblVariables.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVariables.Location = new System.Drawing.Point(9, 11);
            this.lblVariables.Name = "lblVariables";
            this.lblVariables.Size = new System.Drawing.Size(66, 16);
            this.lblVariables.TabIndex = 8;
            this.lblVariables.Text = "Variables";
            // 
            // lblFunctionName
            // 
            this.lblFunctionName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFunctionName.AutoSize = true;
            this.lblFunctionName.BackColor = System.Drawing.Color.Transparent;
            this.lblFunctionName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFunctionName.Location = new System.Drawing.Point(482, 11);
            this.lblFunctionName.Name = "lblFunctionName";
            this.lblFunctionName.Size = new System.Drawing.Size(98, 16);
            this.lblFunctionName.TabIndex = 1;
            this.lblFunctionName.Text = "Function Name";
            // 
            // nameBox
            // 
            this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nameBox.Location = new System.Drawing.Point(488, 30);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(197, 20);
            this.nameBox.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.Location = new System.Drawing.Point(568, 529);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(115, 27);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // lstMethods
            // 
            this.lstMethods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstMethods.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstMethods.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstMethods.FormattingEnabled = true;
            this.lstMethods.IntegralHeight = false;
            this.lstMethods.ItemHeight = 16;
            this.lstMethods.Location = new System.Drawing.Point(12, 410);
            this.lstMethods.Name = "lstMethods";
            this.lstMethods.Size = new System.Drawing.Size(177, 113);
            this.lstMethods.Sorted = true;
            this.lstMethods.TabIndex = 6;
            this.lstMethods.SelectedIndexChanged += new System.EventHandler(this.listMethods_SelectedIndexChanged);
            this.lstMethods.DoubleClick += new System.EventHandler(this.listMethods_DoubleClick);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(452, 529);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(115, 27);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "Save";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // symbBox
            // 
            this.symbBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.symbBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.symbBox.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.symbBox.Location = new System.Drawing.Point(12, 30);
            this.symbBox.Multiline = true;
            this.symbBox.Name = "symbBox";
            this.symbBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.symbBox.Size = new System.Drawing.Size(470, 88);
            this.symbBox.TabIndex = 2;
            // 
            // chkColorSyntax
            // 
            this.chkColorSyntax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkColorSyntax.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkColorSyntax.Checked = true;
            this.chkColorSyntax.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkColorSyntax.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkColorSyntax.Location = new System.Drawing.Point(12, 529);
            this.chkColorSyntax.Name = "chkColorSyntax";
            this.chkColorSyntax.Size = new System.Drawing.Size(81, 27);
            this.chkColorSyntax.TabIndex = 13;
            this.chkColorSyntax.Text = "Color Syntax";
            this.chkColorSyntax.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkColorSyntax.UseVisualStyleBackColor = true;
            this.chkColorSyntax.CheckedChanged += new System.EventHandler(this.chkColorSyntax_CheckedChanged);
            // 
            // scriptTree
            // 
            this.scriptTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scriptTree.ContextMenuStrip = this.treeMenu;
            this.scriptTree.LabelEdit = true;
            this.scriptTree.Location = new System.Drawing.Point(488, 56);
            this.scriptTree.Name = "scriptTree";
            this.scriptTree.Size = new System.Drawing.Size(195, 348);
            this.scriptTree.TabIndex = 12;
            this.scriptTree.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.scriptTree_BeforeLabelEdit);
            this.scriptTree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.scriptTree_AfterLabelEdit);
            this.scriptTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.scriptTree_NodeMouseDoubleClick);
            // 
            // chkShowHelp
            // 
            this.chkShowHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowHelp.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkShowHelp.Checked = true;
            this.chkShowHelp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkShowHelp.Location = new System.Drawing.Point(93, 529);
            this.chkShowHelp.Name = "chkShowHelp";
            this.chkShowHelp.Size = new System.Drawing.Size(81, 27);
            this.chkShowHelp.TabIndex = 14;
            this.chkShowHelp.Text = "Show Help";
            this.chkShowHelp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkShowHelp.UseVisualStyleBackColor = true;
            this.chkShowHelp.CheckedChanged += new System.EventHandler(this.chkShowHelp_CheckedChanged);
            // 
            // chkColorTheme
            // 
            this.chkColorTheme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkColorTheme.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkColorTheme.Checked = true;
            this.chkColorTheme.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkColorTheme.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkColorTheme.Location = new System.Drawing.Point(174, 529);
            this.chkColorTheme.Name = "chkColorTheme";
            this.chkColorTheme.Size = new System.Drawing.Size(81, 27);
            this.chkColorTheme.TabIndex = 15;
            this.chkColorTheme.Text = "Light Theme";
            this.chkColorTheme.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkColorTheme.UseVisualStyleBackColor = true;
            this.chkColorTheme.CheckedChanged += new System.EventHandler(this.chkColorTheme_CheckedChanged);
            // 
            // scriptBox
            // 
            this.scriptBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptBox.BackColor = System.Drawing.Color.White;
            this.scriptBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scriptBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scriptBox.ForeColor = System.Drawing.Color.Black;
            this.scriptBox.HideSelection = false;
            this.scriptBox.Location = new System.Drawing.Point(12, 124);
            this.scriptBox.Name = "scriptBox";
            this.scriptBox.Size = new System.Drawing.Size(470, 280);
            this.scriptBox.TabIndex = 3;
            this.scriptBox.Text = "";
            this.scriptBox.WordWrap = false;
            this.scriptBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.scriptBox_KeyUp);
            // 
            // ScriptFunctionDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(699, 567);
            this.Controls.Add(this.chkColorTheme);
            this.Controls.Add(this.chkShowHelp);
            this.Controls.Add(this.scriptTree);
            this.Controls.Add(this.chkColorSyntax);
            this.Controls.Add(this.rtxtDesc);
            this.Controls.Add(this.symbBox);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.scriptBox);
            this.Controls.Add(this.lstMethods);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.lblFunctionName);
            this.Controls.Add(this.lblVariables);
            this.MinimumSize = new System.Drawing.Size(550, 300);
            this.Name = "ScriptFunctionDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Scripting";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScriptFunctionDialog_FormClosing);
            this.treeMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public ScriptFunctionDialog()
        {
            InitializeComponent();
            lstMethods.Items.AddRange(Enum.GetNames(typeof(methods)));
            foreach (string str in Enum.GetNames(typeof(methods)))
            {
                scriptBox.Settings.Keywords.Add(str);
            }
            string[] words = new string[]
            {
                "int", "void", "null", "not", "return", "jump", "Gvar",
                ">>", "-", "^", "|", "&", "%", "/", "*",
                "||", "&&", "!=", "<=", ">=", ">", "==",
                "not", "neg", ":", "jump", "call", "if", "=", "+"
            };

            foreach (string str in words)
            {
                scriptBox.Settings.Keywords2.Add(str);
            }
            scriptBox.Settings.ColorSyntax = true;
            scriptBox.Settings.DefaultColor = Color.Black;
            scriptBox.Settings.Comment = "//";
            scriptBox.Settings.CommentColor = Color.Green;
            scriptBox.Settings.EnableComments = true;

            scriptBox.Settings.EnableIntegers = true;
            scriptBox.Settings.EnableStrings = true;
            scriptBox.Settings.IntegerColor = Color.Blue;
            scriptBox.Settings.Keyword2Color = Color.Blue;
            scriptBox.Settings.KeywordColor = Color.SteelBlue;
            scriptBox.Settings.StringColor = Color.Red;

            scriptBox.ForeColor = scriptBox.Settings.DefaultColor;
            lastHeight = rtxtDesc.Height;
            chkColorSyntax.Checked = EditorSettings.Default.Script_ColorSyntax;
            chkShowHelp.Checked = EditorSettings.Default.Script_ShowHelp;
            chkColorTheme.Checked = EditorSettings.Default.Script_LightTheme;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                ParseFunction();
            }
            catch
            {
                MessageBox.Show("Wrong Syntax!");
                return;
            }
            Close();
        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ParseFunction()
        {
            if (sf == null)
                return;

            if (sf.name != nameBox.Text)
            {
                var index = sct.Funcs.IndexOf(sf);
                if (index >= 0)
                    treeNode2.Nodes[index].Text = index + ": " + nameBox.Text;

                sf.name = nameBox.Text;
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter wtr = new BinaryWriter(ms);
            if (scriptBox.Text.Contains("00 00 00"))
            {
                Regex bytes = new Regex("[0-9|a-f|A-F]{2}");
                foreach (Match match in bytes.Matches(scriptBox.Text))
                    wtr.Write(Convert.ToByte(match.Value, 16));
            }
            else
            {
                string s = "", scr = scriptBox.Text;
                int start, end, flags, linenum = 0;
                Dictionary<int, string> jumps = new Dictionary<int, string>();
                Dictionary<string, int> labels = new Dictionary<string, int>();

                while ((start = scr.IndexOf('"')) != -1)
                {
                    end = scr.IndexOf('"', start + 1);
                    s = scr.Substring(start + 1, end - start - 1);
                    if (!Scripts.SctStr.Contains(s))
                        Scripts.SctStr.Add(s);
                    scr = scr.Remove(start, end - start + 1).Insert(start, string.Format("*{0}", Scripts.SctStr.IndexOf(s)));
                }
                foreach (var str in scr.Split('\n'))
                {
                    linenum++;
                    flags = 0;
                    string line = str.Trim();
                    if (line.StartsWith("Gvar") && line.Contains("=")) // Set global variable
                    {
                        int line_len = line.StartsWith("GvarF") ? 5 : 4;
                        wtr.Write(2);
                        wtr.Write(1);
                        if (line.Substring(line_len + 1, 1) == " ")
                            wtr.Write(Int32.Parse(line.Substring(line_len, line.IndexOf('[') > -1 ? line.IndexOf('[') - line_len : line.IndexOf(' ') - line_len)));
                        else
                            wtr.Write(Int32.Parse(line.Substring(line_len, line.IndexOf('[') > -1 ? line.IndexOf('[') - line_len : line.IndexOf('=') - line_len)));
                        if (line.IndexOf('[') > -1)
                        {
                            try
                            {
                                ParseWord(wtr, 0, line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1));
                            }
                            catch
                            { 
                                MessageBox.Show("Wromg Syntax!");
                                return;
                            }   
                            wtr.Write(0x44);
                        }
                        switch (line[line.IndexOf("=") - 1])
                        {
                            case 'f':
                                flags = 0x17;
                                break;
                            case '+':
                                flags = (line[line.IndexOf("=") - 2]) == 'f' ? 0x1E : 0x1D;
                                break;
                            default:
                                flags = 0x16;
                                break;
                        }
                        line = line.Split('=')[1].Trim();
                    }
                    else if (line.StartsWith("var") && line.Contains("=")) // Set local variable
                    {

                        int line_len = line.StartsWith("varF") ? 4 : 3;
                        wtr.Write(2);
                        wtr.Write(0);//var0 = Ob
                        //MessageBox.Show(line.Substring(line_len+1,1));
                        if (line.Substring(line_len+1,1) == " ")
                        wtr.Write(Int32.Parse(line.Substring(line_len, line.IndexOf('[') > -1 ? line.IndexOf('[') - line_len : line.IndexOf(' ') - line_len)));
                        else
                        wtr.Write(Int32.Parse(line.Substring(line_len, line.IndexOf('[') > -1 ? line.IndexOf('[') - line_len : line.IndexOf('=') - line_len)));
                        if (line.IndexOf('[') > -1)
                        {

                            try
                            {
                                ParseWord(wtr, 0, line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1));
                            }
                            catch
                            {
                                MessageBox.Show("Wrong Syntax!");
                                return;
                            }   
                            wtr.Write(0x44);
                        }
                        switch (line[line.IndexOf("=") - 1])
                        {
                            case 'f':
                                flags = 0x17;
                                break;
                            case '+':
                                flags = (line[line.IndexOf("=") - 2]) == 'f' ? 0x1E : 0x1D;
                                break;
                            default:
                                flags = 0x16;
                                break;
                        }
                        line = line.Split('=')[1].Trim();
                    }
                    else if (line.StartsWith("if")) // If - jump statement
                    {
                        flags = 0x14;
                        if (line.StartsWith("if not"))
                        {
                            flags++;
                            line = line.Remove(0, 6).Trim();
                        }
                        else
                        {
                            line = line.Remove(0, 2).Trim();
                        }
                        s = line.Substring(line.IndexOf("jump ") + 5).Trim();
                        line = line.Remove(line.IndexOf("jump ")).Trim();
                    }
                    else if (line.Equals("return"))
                    {
                        wtr.Write(0x48);
                        line = "";
                    }
                    else if (line.StartsWith("call "))
                    {
                        wtr.Write(0x46);
                        wtr.Write(Scripts.Funcs.IndexOf(new Map.ScriptFunction(line.Substring(line.IndexOf("call ") + 5))));
                        line = "";
                    }
                    else if (line.StartsWith("jump "))
                    {
                        s = line.Substring(line.IndexOf("jump ") + 5).Trim();
                        wtr.Write(0x13);
                        jumps.Add((int)ms.Position, s);
                        wtr.Write(0);
                        line = "";
                    }
                    else if (line.StartsWith(":"))
                    {
                        labels.Add(line.Substring(1), (int)ms.Position / 4);
                        line = "";
                    }
                    while (line.Length > 0)
                    {
                        try { line = ParseWord(wtr, 0, line); }
                        catch
                        {
                            MessageBox.Show("Wrong Syntax!");
                            return;
                        }
                    }
                    if (flags != 0)
                        wtr.Write(flags);
                    if (flags == 0x14 || flags == 0x15)
                    {
                        jumps.Add((int)ms.Position, s);
                        wtr.Write(0);
                    }
                }
                try
                {
                    foreach (KeyValuePair<int, string> kv in jumps)
                    {
                        ms.Seek(kv.Key, SeekOrigin.Begin);
                        wtr.Write(labels[kv.Value]);
                    }
                }
                catch
                {
                    MessageBox.Show("Wrong Syntax!");
                    return;
                }
            }
            sf.code = ms.ToArray();
            sf.vars.Clear();
            if (symbBox.Text.Length > 0)
            {
                string[] vars;
                vars = symbBox.Text.Split('\n');
                foreach (string s in vars)
                    if (s.IndexOf('[') > -1)
                        sf.vars.Add(int.Parse(s.Remove(s.IndexOf(']'), 1).Remove(0, s.IndexOf('[') + 1).Trim()));
            }
        }
        private string ParseWord(BinaryWriter wtr, int wordi, string line)
        {
            string[] words = line.Split(' '), args;
            string word = line.Split(' ')[wordi], s, name, s2, array = "";
            float tempF = 0;
            int tempI = 0;
            s = Join(wordi, line);
            if (word.Length > 0)
            {
                switch (word)
                {
                    case "NEG":
                        s = s.Remove(s.IndexOf("NEG"), 4);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x40);
                        break;
                    case "NOT":

                        s = s.Remove(s.IndexOf("NOT"), 4);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x3F);
                        break;
                    case "==":

                        s = s.Remove(s.IndexOf("=="), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x23);
                        break;
                    case "f==":

                        s = s.Remove(s.IndexOf("f=="), 4);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x24);
                        break;
                    case "<":

                        s = s.Remove(s.IndexOf("<"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x28);
                        break;
                    case "f<":

                        s = s.Remove(s.IndexOf("f<"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x29);
                        break;
                    case ">":

                        s = s.Remove(s.IndexOf(">"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x2B);
                        break;
                    case "f>":

                        s = s.Remove(s.IndexOf("f>"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x2C);
                        break;
                    case "<=":

                        s = s.Remove(s.IndexOf("<="), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x2E);
                        break;
                    case "f<=":

                        s = s.Remove(s.IndexOf("f<="), 4);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x2F);
                        break;
                    case ">=":

                        s = s.Remove(s.IndexOf(">="), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x31);
                        break;
                    case "f>=":

                        s = s.Remove(s.IndexOf("f>="), 4);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x32);
                        break;
                    case "!=":

                        s = s.Remove(s.IndexOf("!="), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x34);
                        break;
                    case "f!=":

                        s = s.Remove(s.IndexOf("f!="), 4);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x35);
                        break;
                    case "&&":

                        s = s.Remove(s.IndexOf("&&"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x37);
                        break;
                    case "||":

                        s = s.Remove(s.IndexOf("||"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x38);
                        break;
                    case "+":

                        s = s.Remove(s.IndexOf("+"), 2);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x7);
                        break;
                    case "f+":

                        s = s.Remove(s.IndexOf("f+"), 3);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x8);
                        break;
                    case "-":

                        s = s.Remove(s.IndexOf("-"), 2);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x9);
                        break;
                    case "f-":

                        s = s.Remove(s.IndexOf("f-"), 3);

                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0xA);
                        break;
                    case "*":

                        s = s.Remove(s.IndexOf("*"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0xb);
                        break;
                    case "f*":

                        s = s.Remove(s.IndexOf("f*"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0xc);
                        break;
                    case "/":

                        s = s.Remove(s.IndexOf("/"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0xd);
                        break;
                    case "f/":

                        s = s.Remove(s.IndexOf("f/"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0xe);
                        break;
                    case "%":

                        s = s.Remove(s.IndexOf("%"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0xf);
                        break;
                    case "&":

                        s = s.Remove(s.IndexOf("&"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x10);
                        break;
                    case "|":

                        s = s.Remove(s.IndexOf("|"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x11);
                        break;
                    case "^":

                        s = s.Remove(s.IndexOf("^"), 2);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x12);
                        break;
                    case "<<":

                        s = s.Remove(s.IndexOf("<<"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x26);
                        break;
                    case ">>":

                        s = s.Remove(s.IndexOf(">>"), 3);
                        s = Join(wordi, s);
                        s = ParseWord(wtr, 0, s);
                        wtr.Write(0x27);
                        break;
                    default:
                        if (word.StartsWith("-"))
                        {
                            s = s.Remove(s.IndexOf('-'), 1);
                            s = Join(wordi, s);
                            s = ParseWord(wtr, 0, s);
                            wtr.Write(0x41);
                        }
                        else if (word.StartsWith("Gvar")) // Set global variable
                        {

                            if (word.EndsWith("]"))
                            {
                                array = word.Substring(word.IndexOf('[') + 1, word.IndexOf(']') - word.IndexOf('[') - 1);
                                wtr.Write(2);
                                word = word.Remove(word.IndexOf('['), word.IndexOf(']') - word.IndexOf('[') + 1);
                            }
                            else if (word[4] == 'F')
                            {
                                wtr.Write(1);
                                word = word.Remove(4, 1);
                            }
                            else
                                wtr.Write(0);
                            wtr.Write(1);
                            wtr.Write(Int32.Parse(word.Substring(4)));
                            if (array.Length > 0)
                            {
                                while (array.Length > 0)
                                    array = ParseWord(wtr, 0, array);
                                wtr.Write(0x44);
                            }
                            s = Join(wordi + 1, line);
                        }
                        else if (word.StartsWith("var")) // Set local variable
                        {
                            if (word.EndsWith("]"))
                            {
                                array = word.Substring(word.IndexOf('[') + 1, word.IndexOf(']') - word.IndexOf('[') - 1);
                                wtr.Write(2);
                                word = word.Remove(word.IndexOf('['), word.IndexOf(']') - word.IndexOf('[') + 1);
                            }
                            else if (word[3] == 'F')
                            {
                                wtr.Write(1);
                                word = word.Remove(3, 1);
                            }
                            else
                                wtr.Write(0);
                            wtr.Write(0);
                            wtr.Write(Int32.Parse(word.Substring(3)));
                            if (array.Length > 0)
                            {
                                while (array.Length > 0)
                                    array = ParseWord(wtr, 0, array);
                                wtr.Write(0x44);
                            }
                            s = Join(wordi + 1, line);
                        }
                        else if (word.Contains("("))
                        {
                            name = s.Remove(s.IndexOf('('));
                            int end = FindClosing(s.Substring(s.IndexOf('(') + 1));
                            if (end > 0)
                                args = s.Substring(s.IndexOf('(') + 1, end - 1).Split(',');
                            else
                                args = new string[0];
                            foreach (string arg in args)
                            {
                                s2 = arg;
                                while (s2.Length > 0)
                                    s2 = ParseWord(wtr, 0, s2);
                            }
                            wtr.Write(0x45);
                            wtr.Write((int)Enum.Parse(typeof(methods), name, true));
                            s = s.Remove(0, end + 1 + s.IndexOf('(')).Trim();
                        }
                        else if (word[0] == '*' && word[1] != ' ')
                        {
                            wtr.Write(6);
                            wtr.Write(Int32.Parse(word.Remove(0, 1)));
                            s = Join(wordi + 1, line);
                        }
                        else if (word[0] == 'f' && Single.TryParse(word.Remove(0, 1), out tempF))
                        {
                            wtr.Write(5);
                            wtr.Write(tempF);
                            s = Join(wordi + 1, line);
                        }
                        else if (Int32.TryParse(word, out tempI))
                        {
                            wtr.Write(4);
                            wtr.Write(tempI);
                            s = Join(wordi + 1, line);
                        }
                        else if (word.Length > 0)
                        {
                            return "";
                        }
                        break;
                }
            }
            return s;
        }
        private string Join(int wordi, string line)
        {
            string[] words = line.Split(' ');
            string newline = "";
            for (int i = wordi; i < words.Length; i++)
                newline += words[i] + " ";
            return newline.Trim();
        }
        private int FindClosing(string code)
        {
            int count = 0, i = 0;
            for (i = 0; i < code.Length && count > -1; i++)
            {
                if (code[i] == '(')
                    count++;
                if (code[i] == ')')
                    count--;
            }
            return i;
        }

        private void listMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                rtxtDesc.Clear();
                string m_ExePath = Process.GetCurrentProcess().MainModule.FileName;
                m_ExePath = Path.GetDirectoryName(m_ExePath);
                if (File.Exists(m_ExePath + "\\functiondescs\\" + lstMethods.SelectedItem.ToString().ToLower() + ".rtf"))
                {
                    rtxtDesc.LoadFile(m_ExePath + "\\functiondescs\\" + lstMethods.SelectedItem.ToString().ToLower() + ".rtf");
                }
                else
                    rtxtDesc.Text += "No method description found";
            }
            catch
            { }
        }
        private void listMethods_DoubleClick(object sender, EventArgs e)
        {

            string func = (string)lstMethods.SelectedItem;
            int num = 0;
            try
            {
                num = (int)Enum.Parse(typeof(numArgs), (string)lstMethods.SelectedItem);
            }
            catch
            { }
            func += "(";

            for (int i = 1; i <= num; i++)
            {
                if (i == 1)
                    func += " ";

                func += "Arg";

                if (i < num)
                    func += ", ";
                else
                    func += " ";
            }

            func += ")";
            scriptBox.SelectedText = func;
        }

        private void scriptTree_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Parent == treeNode1)
                prevname = e.Node.Text;
            else
                e.Node.EndEdit(true);
        }
        private void scriptTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            string name;
            if (e == null)
                return;
            if (e.Node.Parent == treeNode1)
            {
                if (e.Label != null && e.Label.Length > 0)
                {
                    name = e.Label;
                    Scripts.SctStr[e.Node.Index] = name;
                }
                else
                {
                    treeNode1.Nodes[e.Node.Index].Name = prevname;
                }
            }
        }
        private void scriptTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
                return;
            if (e.Node.Parent == treeNode2)
            {
                try { ParseFunction(); }
                catch { MessageBox.Show("Wrong Syntax!"); }

                symbBox.Clear();
                scriptBox.m_bPaint = false;
                ScriptFunc = Scripts.Funcs[e.Node.Index];
                scriptBox.m_bPaint = true;
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Button hok = sender as Button;
            if (scriptTree.SelectedNode != null)
            {
                if (scriptTree.SelectedNode == treeNode1)
                {
                    treeNode1.Nodes.Add("New String");
                    Scripts.SctStr.Add("New String");
                }
                if (scriptTree.SelectedNode == treeNode2)
                {
                    treeNode2.Nodes.Add(String.Format("{0}: {1}", Scripts.Funcs.Count, "New Function"));
                    Map.ScriptFunction sf = new Map.ScriptFunction();
                    sf.name = "New Function";
                    sf.code = new byte[0];
                    Scripts.Funcs.Add(sf);

                }
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scriptTree.SelectedNode != null)
            {
                if (scriptTree.SelectedNode.Parent == treeNode1)
                {
                    Scripts.SctStr.Remove(scriptTree.SelectedNode.Text);
                    treeNode1.Nodes.Remove(scriptTree.SelectedNode);
                }
                if (scriptTree.SelectedNode.Parent == treeNode2)
                {
                    Map.ScriptFunction toDelete = null;
                    toDelete = (Map.ScriptFunction)Scripts.Funcs[scriptTree.SelectedNode.Index];
                    if (toDelete != null)
                    {
                        Scripts.Funcs.Remove(toDelete);
                        treeNode2.Nodes.Clear();
                        int i = 0;
                        foreach (Map.ScriptFunction sf in Scripts.Funcs)
                        {
                            treeNode2.Nodes.Add(String.Format("{0}: {1}", i, sf.name));
                            i++;
                        }
                    }
                }
            }
        }
        
        private void scriptBox_KeyUp(object sender, KeyEventArgs e)
        {
            GenerateVars();
        }
        private void GenerateVars()
        {
            if (nameBox.Text == "GLOBAL")
                return;

            symbBox.Clear();

            List<string> vars = new List<string>();
            string s = "nene", scr = scriptBox.Text;
            int Numlength;
            int index = 0;
            string svarnum;
            foreach (var str in scr.Split('\n'))
            {
                string line = str.Trim();
                if (line.StartsWith("var") && line.Contains("="))
                {
                    s = line.Substring(line.IndexOf("var") + 3).Trim();
                    s = string.Join("", s.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                    if (s.Contains("+="))
                        Numlength = s.IndexOf("+=");
                    else if (s.Contains("-="))
                        Numlength = s.IndexOf("-=");
                    else
                        Numlength = s.IndexOf("=");
                    svarnum = s.Substring(0, Numlength);
                    s = "var" + svarnum + "[1]" + Environment.NewLine;

                    if (!vars.Contains(s))
                    {
                        vars.Add(s);
                        index++;
                    }

                    vars.Sort();
                }
            }

            string addVars = "";
            for (int i = 0; i < vars.Count; i++)
                addVars += vars[i];

            symbBox.Text = addVars;
        }

        private void chkColorSyntax_CheckedChanged(object sender, EventArgs e)
        {
            scriptBox.Settings.ColorSyntax = chkColorSyntax.Checked;

            if (!chkColorSyntax.Checked)
            {
                scriptBox.SelectAll();
                scriptBox.SelectionColor = scriptBox.Settings.DefaultColor;
                scriptBox.Select(0, 0);
            }
            else
            {
                if (nameBox.Text.Trim() != "")
                {
                    var ne = new TreeNodeMouseClickEventArgs(treeNode2.TreeView.SelectedNode, MouseButtons.Left, 1, 0, 0);
                    scriptTree_NodeMouseDoubleClick(sender, ne);
                }
                else
                    scriptBox.Text = scriptBox.Text;
            }
        }
        private void chkShowHelp_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkShowHelp.Checked)
            {
                lstMethods.Visible = false;
                rtxtDesc.Visible = false;
                lastHeight = rtxtDesc.Height;
                var moveUpBy = lastHeight + 5;

                // Unanchor the top then reduce height
                lstMethods.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                rtxtDesc.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                Size = new Size(Size.Width, Size.Height - moveUpBy);

                // Give CodeText Anchor priority
                scriptBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                scriptTree.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top;
            }
            else
            {
                var moveDownBy = lastHeight + 5;

                // Unanchor the bottom then increase height
                scriptBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                scriptTree.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                Size = new Size(Size.Width, Size.Height + moveDownBy);

                // Give HelpText Anchor priority
                lstMethods.Visible = true;
                rtxtDesc.Visible = true;
                lstMethods.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top;
                rtxtDesc.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }
        }
        private void chkColorTheme_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkColorTheme.Checked)
            {
                BackColor = Color.FromArgb(50, 50, 50);
                var bg = Color.FromArgb(35, 35, 35);
                var fg = Color.White;

                foreach (var c in Controls)
                {
                    if (c.GetType() == typeof(Label))
                        ((Label)c).ForeColor = fg;
                    else if (c.GetType() == typeof(Button))
                        ((Button)c).ForeColor = fg;
                    else if (c.GetType() == typeof(CheckBox))
                        ((CheckBox)c).ForeColor = fg;
                    else if (c.GetType() == typeof(TextBox))
                    {
                        var t = (TextBox)c;
                        t.ForeColor = fg;
                        t.BackColor = bg;
                    }
                }
                scriptBox.Settings.DefaultColor = fg;
                scriptBox.Settings.IntegerColor = Color.DeepSkyBlue;
                scriptBox.Settings.Keyword2Color = Color.DeepSkyBlue;
                scriptBox.Settings.StringColor = Color.OrangeRed;
                scriptBox.ForeColor = fg;
                scriptBox.BackColor = bg;
                scriptTree.ForeColor = fg;
                scriptTree.BackColor = bg;
                lstMethods.ForeColor = fg;
                lstMethods.BackColor = bg;
                rtxtDesc.BackColor = Color.FromArgb(200, 200, 200);
            }
            else
            {
                BackColor = default(Color);
                var bg = Color.FromArgb(35, 35, 35);
                var fg = Color.White;

                foreach (var c in Controls)
                {
                    if (c.GetType() == typeof(Label))
                        ((Label)c).ForeColor = default(Color);
                    else if (c.GetType() == typeof(Button))
                        ((Button)c).ForeColor = default(Color);
                    else if (c.GetType() == typeof(CheckBox))
                        ((CheckBox)c).ForeColor = default(Color);
                    else if (c.GetType() == typeof(TextBox))
                    {
                        var t = (TextBox)c;
                        t.ForeColor = default(Color);
                        t.BackColor = default(Color);
                    }
                }
                scriptBox.Settings.DefaultColor = default(Color);
                scriptBox.Settings.IntegerColor = Color.Blue;
                scriptBox.Settings.Keyword2Color = Color.Blue;
                scriptBox.Settings.StringColor = Color.Red;
                scriptBox.ForeColor = default(Color);
                scriptBox.BackColor = default(Color);
                scriptTree.ForeColor = default(Color);
                scriptTree.BackColor = default(Color);
                lstMethods.ForeColor = default(Color);
                lstMethods.BackColor = default(Color);
                rtxtDesc.BackColor = SystemColors.Info;
            }
            // Refresh colored syntax
            if (chkColorSyntax.Checked)
            {
                if (nameBox.Text.Trim() != "")
                {
                    var ne = new TreeNodeMouseClickEventArgs(treeNode2.TreeView.SelectedNode, MouseButtons.Left, 1, 0, 0);
                    scriptTree_NodeMouseDoubleClick(sender, ne);
                }
                else
                    scriptBox.Text = scriptBox.Text;
            }
        }
        private void ScriptFunctionDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            EditorSettings.Default.Script_ColorSyntax = chkColorSyntax.Checked;
            EditorSettings.Default.Script_ShowHelp = chkShowHelp.Checked;
            EditorSettings.Default.Script_LightTheme = chkColorTheme.Checked;
            EditorSettings.Default.Save();
        }

        private string GetCode()
        {
            Stack<string> args = new Stack<string>();
            List<int> jumps = new List<int>();

            int index = 0;
            while (index < sf.vars.Count)
            {
                symbBox.Text += string.Format("var{0}[{1}]\r\n", index, sf.vars[index]);
                index++;
            }
            index = 0;

            string human = "";
            string equals;
            int funccode, opcode, global;
            MemoryStream ms = new MemoryStream(sf.code);
            BinaryReader rdr = new BinaryReader(ms);

            while (ms.Position < ms.Length)
            {
                if (jumps.Contains((int)ms.Position / 4))
                {
                    index = human.Length;
                    foreach (string s in args.ToArray())
                        human = human.Insert(index, s + "\r\n");
                    human += string.Format(":{0}\r\n", ms.Position / 4);
                    args.Clear();
                }
                opcode = rdr.ReadInt32();
                switch (opcode)
                {
                    case 0:
                    case 3:
                        global = rdr.ReadInt32();
                        Debug.Assert(global == 0 || global == 1);
                        equals = global == 0 ? "var" : "Gvar";
                        args.Push(equals + rdr.ReadInt32().ToString());
                        break;
                    case 1:
                        global = rdr.ReadInt32();
                        Debug.Assert(global == 0 || global == 1);
                        equals = global == 0 ? "varF" : "GvarF";
                        args.Push(equals + rdr.ReadInt32().ToString());
                        break;
                    case 2:
                        global = rdr.ReadInt32();
                        Debug.Assert(global == 0 || global == 1);
                        equals = global == 0 ? "var" : "Gvar";
                        args.Push(equals + rdr.ReadInt32().ToString());
                        break;
                    case 4:
                        args.Push(rdr.ReadInt32().ToString());
                        break;
                    case 5:
                        System.Globalization.NumberFormatInfo nfi_float = new System.Globalization.NumberFormatInfo();
                        nfi_float.NumberDecimalSeparator = ".";
                        args.Push("f" + rdr.ReadSingle().ToString(nfi_float));
                        break;
                    case 6:
                        args.Push('"' + (string)Scripts.SctStr[rdr.ReadInt32()] + '"');
                        break;
                    case 7:
                        equals = args.Pop();
                        args.Push(args.Pop() + " + " + equals);
                        break;
                    case 8:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f+ " + equals);
                        break;
                    case 9:
                        equals = args.Pop();
                        args.Push(args.Pop() + " - " + equals);
                        break;
                    case 0xa:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f- " + equals);
                        break;
                    case 0xb:
                        equals = args.Pop();
                        args.Push(args.Pop() + " * " + equals);
                        break;
                    case 0xc:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f* " + equals);
                        break;
                    case 0xd:
                        equals = args.Pop();
                        args.Push(args.Pop() + " / " + equals);
                        break;
                    case 0xe:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f/ " + equals);
                        break;
                    case 0xf:
                        equals = args.Pop();
                        args.Push(args.Pop() + " % " + equals);
                        break;
                    case 0x10:
                        equals = args.Pop();
                        args.Push(args.Pop() + " & " + equals);
                        break;
                    case 0x11:
                        equals = args.Pop();
                        args.Push(args.Pop() + " | " + equals);
                        break;
                    case 0x12:
                        equals = args.Pop();
                        args.Push(args.Pop() + " ^ " + equals);
                        break;
                    case 0x26:
                        equals = args.Pop();
                        args.Push(args.Pop() + " << " + equals);
                        break;
                    case 0x27:
                        equals = args.Pop();
                        args.Push(args.Pop() + " >> " + equals);
                        break;
                    case 0x13:
                        opcode = rdr.ReadInt32();
                        if (!jumps.Contains(opcode))
                            jumps.Add(opcode);
                        args.Push("jump " + opcode.ToString());
                        break;
                    case 0x14:
                        opcode = rdr.ReadInt32();
                        if (!jumps.Contains(opcode))
                            jumps.Add(opcode);
                        args.Push("if " + args.Pop() + " jump " + opcode.ToString());
                        break;
                    case 0x15:
                        opcode = rdr.ReadInt32();
                        if (!jumps.Contains(opcode))
                            jumps.Add(opcode);
                        args.Push("if not " + args.Pop() + " jump " + opcode.ToString());
                        break;
                    case 0x23:
                        equals = args.Pop();
                        args.Push(args.Pop() + " == " + equals);
                        break;
                    case 0x24:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f== " + equals);
                        break;
                    case 0x28:
                        equals = args.Pop();
                        args.Push(args.Pop() + " < " + equals);
                        break;
                    case 0x29:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f< " + equals);
                        break;
                    case 0x2B:
                        equals = args.Pop();
                        args.Push(args.Pop() + " > " + equals);
                        break;
                    case 0x2C:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f> " + equals);
                        break;
                    case 0x2E:
                        equals = args.Pop();
                        args.Push(args.Pop() + " <= " + equals);
                        break;
                    case 0x2F:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f<= " + equals);
                        break;
                    case 0x31:
                        equals = args.Pop();
                        args.Push(args.Pop() + " >= " + equals);
                        break;
                    case 0x32:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f>= " + equals);
                        break;
                    case 0x34:
                        equals = args.Pop();
                        args.Push(args.Pop() + " != " + equals);
                        break;
                    case 0x35:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f!= " + equals);
                        break;
                    case 0x37:
                        equals = args.Pop();
                        args.Push(args.Pop() + " && " + equals);
                        break;
                    case 0x38:
                        equals = args.Pop();
                        args.Push(args.Pop() + " || " + equals);
                        break;
                    case 0x3f:
                        args.Push("NOT " + args.Pop());
                        break;
                    case 0x40:
                        args.Push("NEG " + args.Pop());
                        break;
                    case 0x16:
                        equals = args.Pop();
                        args.Push(args.Pop() + " = " + equals);
                        break;
                    case 0x17:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f= " + equals);
                        break;
                    case 0x19:
                        equals = args.Pop();
                        args.Push(args.Pop() + " *= " + equals);
                        break;
                    case 0x1A:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f*= " + equals);
                        break;
                    case 0x1B:
                        equals = args.Pop();
                        args.Push(args.Pop() + " /= " + equals);
                        break;
                    case 0x1C:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f/= " + equals);
                        break;
                    case 0x1D:
                        equals = args.Pop();
                        args.Push(args.Pop() + " += " + equals);
                        break;
                    case 0x1E:
                        equals = args.Pop();
                        args.Push(args.Pop() + " f+= " + equals);
                        break;
                    case 0x41:
                        args.Push("-" + args.Pop());
                        break;
                    case 0x44:
                        equals = args.Pop();
                        args.Push(args.Pop() + "[" + equals + "]");
                        break;
                    case 0x45:
                        funccode = rdr.ReadInt32();
                        if (Enum.IsDefined(typeof(methods), funccode))
                        {
                            equals = Enum.GetName(typeof(methods), funccode) + "(";
                            index = equals.Length;
                            for (int i = 0; i < (int)Enum.Parse(typeof(numArgs), Enum.GetName(typeof(methods), funccode)); i++)
                                equals = equals.Insert(index, (i == 0) ? args.Pop() : args.Pop() + ",");
                            equals += ')';
                            args.Push(equals);
                        }
                        else
                            Debug.WriteLine(string.Format("Funccode:{0:x2} does not exist!", funccode));
                        break;
                    case 0x46:
                        args.Push("call " + Scripts.Funcs[rdr.ReadInt32()].name);
                        break;
                    case 0x48:
                        args.Push("return");
                        break;
                    case 0x42:
                        equals = args.Pop();
                        args.Push(args.Pop() + "[" + equals + "]");
                        break;
                    default:
                        Debug.WriteLine(string.Format("Unknown opcode:{0:x2}", opcode), "ScriptFunctionDialog");
                        break;
                }
            }
            index = human.Length;
            foreach (string s in args.ToArray())
                human = human.Insert(index, s + "\r\n");

            return human;
        }
        public string ExportNativeScript()
        {
            var result = "// #########  STRINGS  #########" + Environment.NewLine;
            foreach (var s in Scripts.SctStr)
                result += s + Environment.NewLine;
            result += Environment.NewLine + "// #########  FUNCTIONS  #########" + Environment.NewLine;
            foreach (var f in Scripts.Funcs)
            {
                sf = f;
                var vars = "";
                int i = 0;
                foreach (var v in f.vars)
                {
                    vars += "var" + i + "[" + v + "] ";
                    i++;
                }
                result += vars + Environment.NewLine +
                    f + "(" + ((f.args > 0) ? f.args + " args" : "") + ")" + Environment.NewLine +
                    GetCode() + Environment.NewLine;
            }

            return result;
        }
    }
}