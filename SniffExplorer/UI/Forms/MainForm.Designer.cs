namespace SniffExplorer.UI.Forms
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.loadSniffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._opcodeListView = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this._filterTextBox = new System.Windows.Forms.TextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._detailListView = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this._detailedPacketView = new System.Windows.Forms.PropertyGrid();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._opcodeListView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._detailListView)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadSniffToolStripMenuItem,
            this.filterToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(968, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // loadSniffToolStripMenuItem
            // 
            this.loadSniffToolStripMenuItem.Name = "loadSniffToolStripMenuItem";
            this.loadSniffToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.loadSniffToolStripMenuItem.Text = "Load sniff";
            this.loadSniffToolStripMenuItem.Click += new System.EventHandler(this.LoadSniff);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(10);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._opcodeListView);
            this.splitContainer1.Panel1.Controls.Add(this._filterTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(968, 381);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 2;
            // 
            // _opcodeListView
            // 
            this._opcodeListView.AllColumns.Add(this.olvColumn1);
            this._opcodeListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._opcodeListView.CellEditUseWholeCell = false;
            this._opcodeListView.CheckBoxes = true;
            this._opcodeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this._opcodeListView.Enabled = false;
            this._opcodeListView.FullRowSelect = true;
            this._opcodeListView.GridLines = true;
            this._opcodeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this._opcodeListView.Location = new System.Drawing.Point(3, 26);
            this._opcodeListView.Name = "_opcodeListView";
            this._opcodeListView.ShowGroups = false;
            this._opcodeListView.ShowHeaderInAllViews = false;
            this._opcodeListView.ShowImagesOnSubItems = true;
            this._opcodeListView.Size = new System.Drawing.Size(244, 349);
            this._opcodeListView.TabIndex = 1;
            this._opcodeListView.UseCompatibleStateImageBehavior = false;
            this._opcodeListView.UseFiltering = true;
            this._opcodeListView.View = System.Windows.Forms.View.Details;
            this._opcodeListView.VirtualMode = true;
            // 
            // olvColumn1
            // 
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Text = "";
            // 
            // _filterTextBox
            // 
            this._filterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._filterTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._filterTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._filterTextBox.Enabled = false;
            this._filterTextBox.Location = new System.Drawing.Point(3, 0);
            this._filterTextBox.Name = "_filterTextBox";
            this._filterTextBox.Size = new System.Drawing.Size(244, 20);
            this._filterTextBox.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._detailListView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this._detailedPacketView);
            this.splitContainer2.Size = new System.Drawing.Size(714, 381);
            this.splitContainer2.SplitterDistance = 400;
            this.splitContainer2.TabIndex = 0;
            // 
            // _detailListView
            // 
            this._detailListView.AllColumns.Add(this.olvColumn2);
            this._detailListView.AllColumns.Add(this.olvColumn3);
            this._detailListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._detailListView.CellEditUseWholeCell = false;
            this._detailListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn2,
            this.olvColumn3});
            this._detailListView.FullRowSelect = true;
            this._detailListView.GridLines = true;
            this._detailListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this._detailListView.Location = new System.Drawing.Point(3, 3);
            this._detailListView.MultiSelect = false;
            this._detailListView.Name = "_detailListView";
            this._detailListView.ShowGroups = false;
            this._detailListView.Size = new System.Drawing.Size(394, 373);
            this._detailListView.TabIndex = 0;
            this._detailListView.UseCompatibleStateImageBehavior = false;
            this._detailListView.View = System.Windows.Forms.View.Details;
            this._detailListView.VirtualMode = true;
            // 
            // olvColumn2
            // 
            this.olvColumn2.FillsFreeSpace = true;
            this.olvColumn2.Text = "Opcode";
            // 
            // olvColumn3
            // 
            this.olvColumn3.FillsFreeSpace = true;
            this.olvColumn3.Text = "Timestamp";
            // 
            // _detailedPacketView
            // 
            this._detailedPacketView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._detailedPacketView.Location = new System.Drawing.Point(3, 3);
            this._detailedPacketView.Name = "_detailedPacketView";
            this._detailedPacketView.Size = new System.Drawing.Size(304, 373);
            this._detailedPacketView.TabIndex = 0;
            this._detailedPacketView.ToolbarVisible = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 402);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(968, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.filterToolStripMenuItem.Text = "Filter";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 424);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Sniff Explorer";
            this.Load += new System.EventHandler(this.OnLoad);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._opcodeListView)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._detailListView)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem loadSniffToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox _filterTextBox;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private BrightIdeasSoftware.FastObjectListView _detailListView;
        private System.Windows.Forms.PropertyGrid _detailedPacketView;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private BrightIdeasSoftware.FastObjectListView _opcodeListView;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
    }
}

