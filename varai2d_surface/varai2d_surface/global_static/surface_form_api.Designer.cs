
namespace varai2d_surface.global_static
{
    partial class surface_form_api
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_help = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_add = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView_surfacedata = new System.Windows.Forms.DataGridView();
            this.Column_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_end_pts_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_lineids = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_arcid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_bezierIds = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_nested = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_area = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_surfacedata)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_help});
            this.statusStrip1.Location = new System.Drawing.Point(0, 327);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(882, 26);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_help
            // 
            this.toolStripStatusLabel_help.Name = "toolStripStatusLabel_help";
            this.toolStripStatusLabel_help.Size = new System.Drawing.Size(499, 20);
            this.toolStripStatusLabel_help.Text = "Select (Add Surface or Delete Surface option) and Left click on the surface";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_add,
            this.toolStripMenuItem_delete});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(882, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem_add
            // 
            this.toolStripMenuItem_add.Name = "toolStripMenuItem_add";
            this.toolStripMenuItem_add.Size = new System.Drawing.Size(104, 24);
            this.toolStripMenuItem_add.Text = "Add Surface";
            this.toolStripMenuItem_add.Click += new System.EventHandler(this.toolStripMenuItem_add_ItemClicked);
            // 
            // toolStripMenuItem_delete
            // 
            this.toolStripMenuItem_delete.Name = "toolStripMenuItem_delete";
            this.toolStripMenuItem_delete.Size = new System.Drawing.Size(120, 24);
            this.toolStripMenuItem_delete.Text = "Delete Surface";
            this.toolStripMenuItem_delete.Click += new System.EventHandler(this.toolStripMenuItem_delete_ItemClicked);
            // 
            // dataGridView_surfacedata
            // 
            this.dataGridView_surfacedata.AllowUserToAddRows = false;
            this.dataGridView_surfacedata.AllowUserToDeleteRows = false;
            this.dataGridView_surfacedata.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView_surfacedata.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_surfacedata.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_id,
            this.Column_end_pts_id,
            this.Column_lineids,
            this.Column_arcid,
            this.Column_bezierIds,
            this.Column_nested,
            this.Column_area});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.NullValue = null;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_surfacedata.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_surfacedata.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_surfacedata.Location = new System.Drawing.Point(0, 28);
            this.dataGridView_surfacedata.Name = "dataGridView_surfacedata";
            this.dataGridView_surfacedata.ReadOnly = true;
            this.dataGridView_surfacedata.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridView_surfacedata.RowTemplate.Height = 29;
            this.dataGridView_surfacedata.Size = new System.Drawing.Size(882, 299);
            this.dataGridView_surfacedata.TabIndex = 2;
            // 
            // Column_id
            // 
            this.Column_id.HeaderText = "Surface ID";
            this.Column_id.MinimumWidth = 6;
            this.Column_id.Name = "Column_id";
            this.Column_id.ReadOnly = true;
            this.Column_id.Width = 80;
            // 
            // Column_end_pts_id
            // 
            this.Column_end_pts_id.HeaderText = "End Pts IDs";
            this.Column_end_pts_id.MinimumWidth = 6;
            this.Column_end_pts_id.Name = "Column_end_pts_id";
            this.Column_end_pts_id.ReadOnly = true;
            this.Column_end_pts_id.Width = 125;
            // 
            // Column_lineids
            // 
            this.Column_lineids.HeaderText = "Line IDs";
            this.Column_lineids.MinimumWidth = 6;
            this.Column_lineids.Name = "Column_lineids";
            this.Column_lineids.ReadOnly = true;
            this.Column_lineids.Width = 125;
            // 
            // Column_arcid
            // 
            this.Column_arcid.HeaderText = "Arc IDs";
            this.Column_arcid.MinimumWidth = 6;
            this.Column_arcid.Name = "Column_arcid";
            this.Column_arcid.ReadOnly = true;
            this.Column_arcid.Width = 125;
            // 
            // Column_bezierIds
            // 
            this.Column_bezierIds.HeaderText = "Bezier IDs";
            this.Column_bezierIds.MinimumWidth = 6;
            this.Column_bezierIds.Name = "Column_bezierIds";
            this.Column_bezierIds.ReadOnly = true;
            this.Column_bezierIds.Width = 125;
            // 
            // Column_nested
            // 
            this.Column_nested.HeaderText = "Nested member IDs";
            this.Column_nested.MinimumWidth = 6;
            this.Column_nested.Name = "Column_nested";
            this.Column_nested.ReadOnly = true;
            this.Column_nested.Width = 125;
            // 
            // Column_area
            // 
            this.Column_area.HeaderText = "Surface Area";
            this.Column_area.MinimumWidth = 6;
            this.Column_area.Name = "Column_area";
            this.Column_area.ReadOnly = true;
            this.Column_area.Width = 125;
            // 
            // surface_form_api
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 353);
            this.Controls.Add(this.dataGridView_surfacedata);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "surface_form_api";
            this.Text = "Surface Creation API";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.surface_form_api_FormClosing);
            this.Load += new System.EventHandler(this.surface_form_api_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_surfacedata)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_add;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_delete;
        private System.Windows.Forms.DataGridView dataGridView_surfacedata;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_help;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_end_pts_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_lineids;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_arcid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_bezierIds;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_nested;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_area;
    }
}