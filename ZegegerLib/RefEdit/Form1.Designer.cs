namespace RefEdit
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.refList = new System.Windows.Forms.ListBox();
            this.valList = new System.Windows.Forms.ListBox();
            this.refName = new System.Windows.Forms.TextBox();
            this.refAdd = new System.Windows.Forms.Button();
            this.refDelete = new System.Windows.Forms.Button();
            this.valName = new System.Windows.Forms.TextBox();
            this.valType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.valIsArray = new System.Windows.Forms.CheckBox();
            this.valData = new System.Windows.Forms.TextBox();
            this.valAdd = new System.Windows.Forms.Button();
            this.valDelete = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.StatusText = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.validateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label5 = new System.Windows.Forms.Label();
            this.valRef = new System.Windows.Forms.ComboBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.valLblData = new System.Windows.Forms.Label();
            this.valRefData = new System.Windows.Forms.ComboBox();
            this.valNew = new System.Windows.Forms.Button();
            this.XMLText = new System.Windows.Forms.RichTextBox();
            this.EditXML = new System.Windows.Forms.Button();
            this.RevertXML = new System.Windows.Forms.Button();
            this.DoneXML = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // refList
            // 
            this.refList.Enabled = false;
            this.refList.FormattingEnabled = true;
            this.refList.Location = new System.Drawing.Point(616, 53);
            this.refList.Name = "refList";
            this.refList.Size = new System.Drawing.Size(219, 121);
            this.refList.TabIndex = 0;
            this.refList.SelectedIndexChanged += new System.EventHandler(this.refList_SelectedIndexChanged);
            // 
            // valList
            // 
            this.valList.Enabled = false;
            this.valList.FormattingEnabled = true;
            this.valList.Location = new System.Drawing.Point(616, 201);
            this.valList.Name = "valList";
            this.valList.Size = new System.Drawing.Size(405, 134);
            this.valList.TabIndex = 1;
            this.valList.SelectedValueChanged += new System.EventHandler(this.valList_SelectedValueChanged);
            // 
            // refName
            // 
            this.refName.Enabled = false;
            this.refName.Location = new System.Drawing.Point(851, 53);
            this.refName.Name = "refName";
            this.refName.Size = new System.Drawing.Size(170, 20);
            this.refName.TabIndex = 2;
            // 
            // refAdd
            // 
            this.refAdd.Enabled = false;
            this.refAdd.Location = new System.Drawing.Point(946, 79);
            this.refAdd.Name = "refAdd";
            this.refAdd.Size = new System.Drawing.Size(75, 23);
            this.refAdd.TabIndex = 3;
            this.refAdd.Text = "Add";
            this.refAdd.UseVisualStyleBackColor = true;
            this.refAdd.Click += new System.EventHandler(this.refAdd_Click);
            // 
            // refDelete
            // 
            this.refDelete.Enabled = false;
            this.refDelete.Location = new System.Drawing.Point(851, 151);
            this.refDelete.Name = "refDelete";
            this.refDelete.Size = new System.Drawing.Size(75, 23);
            this.refDelete.TabIndex = 4;
            this.refDelete.Text = "Delete";
            this.refDelete.UseVisualStyleBackColor = true;
            this.refDelete.Click += new System.EventHandler(this.refDelete_Click);
            // 
            // valName
            // 
            this.valName.Enabled = false;
            this.valName.Location = new System.Drawing.Point(704, 376);
            this.valName.Name = "valName";
            this.valName.Size = new System.Drawing.Size(317, 20);
            this.valName.TabIndex = 6;
            // 
            // valType
            // 
            this.valType.Enabled = false;
            this.valType.FormattingEnabled = true;
            this.valType.Items.AddRange(new object[] {
            "String",
            "Int",
            "Float",
            "Bool",
            "XML",
            "Hex",
            "Alias"});
            this.valType.Location = new System.Drawing.Point(704, 402);
            this.valType.Name = "valType";
            this.valType.Size = new System.Drawing.Size(317, 21);
            this.valType.TabIndex = 7;
            this.valType.SelectedValueChanged += new System.EventHandler(this.valType_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(616, 379);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(616, 405);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Type";
            // 
            // valIsArray
            // 
            this.valIsArray.AutoSize = true;
            this.valIsArray.Enabled = false;
            this.valIsArray.Location = new System.Drawing.Point(704, 456);
            this.valIsArray.Name = "valIsArray";
            this.valIsArray.Size = new System.Drawing.Size(61, 17);
            this.valIsArray.TabIndex = 10;
            this.valIsArray.Text = "Is Array";
            this.valIsArray.UseVisualStyleBackColor = true;
            // 
            // valData
            // 
            this.valData.Enabled = false;
            this.valData.Location = new System.Drawing.Point(704, 479);
            this.valData.Multiline = true;
            this.valData.Name = "valData";
            this.valData.Size = new System.Drawing.Size(317, 64);
            this.valData.TabIndex = 11;
            // 
            // valAdd
            // 
            this.valAdd.Enabled = false;
            this.valAdd.Location = new System.Drawing.Point(946, 549);
            this.valAdd.Name = "valAdd";
            this.valAdd.Size = new System.Drawing.Size(75, 23);
            this.valAdd.TabIndex = 12;
            this.valAdd.Text = "Add/Update";
            this.valAdd.UseVisualStyleBackColor = true;
            this.valAdd.Click += new System.EventHandler(this.valAdd_Click);
            // 
            // valDelete
            // 
            this.valDelete.Enabled = false;
            this.valDelete.Location = new System.Drawing.Point(946, 341);
            this.valDelete.Name = "valDelete";
            this.valDelete.Size = new System.Drawing.Size(75, 23);
            this.valDelete.TabIndex = 13;
            this.valDelete.Text = "Delete";
            this.valDelete.UseVisualStyleBackColor = true;
            this.valDelete.Click += new System.EventHandler(this.valDelete_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(616, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "References";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(616, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Values";
            // 
            // StatusText
            // 
            this.StatusText.Location = new System.Drawing.Point(12, 554);
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(598, 18);
            this.StatusText.TabIndex = 16;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1047, 24);
            this.menuStrip1.TabIndex = 17;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator2,
            this.validateToolStripMenuItem,
            this.toolStripSeparator1,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // validateToolStripMenuItem
            // 
            this.validateToolStripMenuItem.Name = "validateToolStripMenuItem";
            this.validateToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.validateToolStripMenuItem.Text = "Validate";
            this.validateToolStripMenuItem.Click += new System.EventHandler(this.validateToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(616, 432);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Alias Reference";
            // 
            // valRef
            // 
            this.valRef.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.valRef.Enabled = false;
            this.valRef.FormattingEnabled = true;
            this.valRef.Location = new System.Drawing.Point(704, 429);
            this.valRef.Name = "valRef";
            this.valRef.Size = new System.Drawing.Size(317, 21);
            this.valRef.TabIndex = 20;
            this.valRef.SelectedValueChanged += new System.EventHandler(this.valRef_SelectedValueChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "XML files|*.xml";
            // 
            // valLblData
            // 
            this.valLblData.AutoSize = true;
            this.valLblData.Location = new System.Drawing.Point(620, 482);
            this.valLblData.Name = "valLblData";
            this.valLblData.Size = new System.Drawing.Size(34, 13);
            this.valLblData.TabIndex = 21;
            this.valLblData.Text = "Value";
            // 
            // valRefData
            // 
            this.valRefData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.valRefData.Enabled = false;
            this.valRefData.FormattingEnabled = true;
            this.valRefData.Location = new System.Drawing.Point(704, 479);
            this.valRefData.Name = "valRefData";
            this.valRefData.Size = new System.Drawing.Size(317, 21);
            this.valRefData.TabIndex = 22;
            this.valRefData.Visible = false;
            // 
            // valNew
            // 
            this.valNew.Enabled = false;
            this.valNew.Location = new System.Drawing.Point(865, 341);
            this.valNew.Name = "valNew";
            this.valNew.Size = new System.Drawing.Size(75, 23);
            this.valNew.TabIndex = 23;
            this.valNew.Text = "New";
            this.valNew.UseVisualStyleBackColor = true;
            this.valNew.Click += new System.EventHandler(this.valNew_Click);
            // 
            // XMLText
            // 
            this.XMLText.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.XMLText.Location = new System.Drawing.Point(12, 27);
            this.XMLText.Name = "XMLText";
            this.XMLText.ReadOnly = true;
            this.XMLText.Size = new System.Drawing.Size(598, 490);
            this.XMLText.TabIndex = 24;
            this.XMLText.Text = "";
            // 
            // EditXML
            // 
            this.EditXML.Location = new System.Drawing.Point(535, 523);
            this.EditXML.Name = "EditXML";
            this.EditXML.Size = new System.Drawing.Size(75, 23);
            this.EditXML.TabIndex = 25;
            this.EditXML.Text = "Edit";
            this.EditXML.UseVisualStyleBackColor = true;
            this.EditXML.Visible = false;
            this.EditXML.Click += new System.EventHandler(this.EditXML_Click);
            // 
            // RevertXML
            // 
            this.RevertXML.Location = new System.Drawing.Point(454, 523);
            this.RevertXML.Name = "RevertXML";
            this.RevertXML.Size = new System.Drawing.Size(75, 23);
            this.RevertXML.TabIndex = 26;
            this.RevertXML.Text = "Revert";
            this.RevertXML.UseVisualStyleBackColor = true;
            this.RevertXML.Visible = false;
            this.RevertXML.Click += new System.EventHandler(this.RevertXML_Click);
            // 
            // DoneXML
            // 
            this.DoneXML.Location = new System.Drawing.Point(535, 523);
            this.DoneXML.Name = "DoneXML";
            this.DoneXML.Size = new System.Drawing.Size(75, 23);
            this.DoneXML.TabIndex = 27;
            this.DoneXML.Text = "Done";
            this.DoneXML.UseVisualStyleBackColor = true;
            this.DoneXML.Visible = false;
            this.DoneXML.Click += new System.EventHandler(this.DoneXML_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "xml";
            this.saveFileDialog1.Filter = "XML files|*.xml";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1047, 584);
            this.Controls.Add(this.DoneXML);
            this.Controls.Add(this.RevertXML);
            this.Controls.Add(this.EditXML);
            this.Controls.Add(this.XMLText);
            this.Controls.Add(this.valNew);
            this.Controls.Add(this.valRefData);
            this.Controls.Add(this.valLblData);
            this.Controls.Add(this.valRef);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.StatusText);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.valDelete);
            this.Controls.Add(this.valAdd);
            this.Controls.Add(this.valData);
            this.Controls.Add(this.valIsArray);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.valType);
            this.Controls.Add(this.valName);
            this.Controls.Add(this.refDelete);
            this.Controls.Add(this.refAdd);
            this.Controls.Add(this.refName);
            this.Controls.Add(this.valList);
            this.Controls.Add(this.refList);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Ref Edit";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox refList;
        private System.Windows.Forms.ListBox valList;
        private System.Windows.Forms.TextBox refName;
        private System.Windows.Forms.Button refAdd;
        private System.Windows.Forms.Button refDelete;
        private System.Windows.Forms.TextBox valName;
        private System.Windows.Forms.ComboBox valType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox valIsArray;
        private System.Windows.Forms.TextBox valData;
        private System.Windows.Forms.Button valAdd;
        private System.Windows.Forms.Button valDelete;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label StatusText;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox valRef;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label valLblData;
        private System.Windows.Forms.ComboBox valRefData;
        private System.Windows.Forms.Button valNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.RichTextBox XMLText;
        private System.Windows.Forms.Button EditXML;
        private System.Windows.Forms.Button RevertXML;
        private System.Windows.Forms.Button DoneXML;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem validateToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

