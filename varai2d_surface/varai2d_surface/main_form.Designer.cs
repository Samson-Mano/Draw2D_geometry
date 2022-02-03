
namespace varai2d_surface
{
    partial class main_form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main_form));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusStripLabel_zoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripLabel_scale = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripLabel_tooltip = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_error = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_new = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_open = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_save = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_export = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_rawdatatxt = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_screenshotbmp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_close = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_divider1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_select = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_addLine = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_addCircle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_addarc1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_addarc2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_addbezier = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_bezier3pt = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_bezier4pt = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_bezier5pt = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_scale_to_fit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_divider2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_duplicate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_translate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_rotate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_mirror = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_divider3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_intersect = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_splitline = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_divider4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_addsurface = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_deletesurface = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_options = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_divider5 = new System.Windows.Forms.ToolStripSeparator();
            this.main_pic = new System.Windows.Forms.Panel();
            this.mt_pic = new System.Windows.Forms.PictureBox();
            this.txt_keyboard_inpt = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.main_pic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mt_pic)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripLabel_zoom,
            this.statusStripLabel_scale,
            this.statusStripLabel_tooltip,
            this.toolStripStatusLabel_error});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 467);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1088, 26);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusStripLabel_zoom
            // 
            this.statusStripLabel_zoom.Name = "statusStripLabel_zoom";
            this.statusStripLabel_zoom.Size = new System.Drawing.Size(92, 20);
            this.statusStripLabel_zoom.Text = "Zoom: 100%";
            // 
            // statusStripLabel_scale
            // 
            this.statusStripLabel_scale.Name = "statusStripLabel_scale";
            this.statusStripLabel_scale.Size = new System.Drawing.Size(70, 20);
            this.statusStripLabel_scale.Text = "Scale: 1.0";
            // 
            // statusStripLabel_tooltip
            // 
            this.statusStripLabel_tooltip.Name = "statusStripLabel_tooltip";
            this.statusStripLabel_tooltip.Size = new System.Drawing.Size(241, 20);
            this.statusStripLabel_tooltip.Text = "File -> New (to start a new project)";
            // 
            // toolStripStatusLabel_error
            // 
            this.toolStripStatusLabel_error.Name = "toolStripStatusLabel_error";
            this.toolStripStatusLabel_error.Size = new System.Drawing.Size(40, 20);
            this.toolStripStatusLabel_error.Text = "[x, y]";
            this.toolStripStatusLabel_error.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_file,
            this.toolStripMenuItem_divider1,
            this.toolStripMenuItem_select,
            this.toolStripMenuItem_addLine,
            this.toolStripMenuItem_addCircle,
            this.toolStripMenuItem_addarc1,
            this.toolStripMenuItem_addarc2,
            this.toolStripMenuItem_addbezier,
            this.toolStripMenuItem_scale_to_fit,
            this.toolStripMenuItem_divider2,
            this.toolStripMenuItem_delete,
            this.toolStripMenuItem_duplicate,
            this.toolStripMenuItem_translate,
            this.toolStripMenuItem_rotate,
            this.toolStripMenuItem_mirror,
            this.toolStripMenuItem_divider3,
            this.toolStripMenuItem_intersect,
            this.toolStripMenuItem_splitline,
            this.toolStripMenuItem_divider4,
            this.toolStripMenuItem_addsurface,
            this.toolStripMenuItem_deletesurface,
            this.toolStripMenuItem_options,
            this.toolStripMenuItem_divider5});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.menuStrip1.ShowItemToolTips = true;
            this.menuStrip1.Size = new System.Drawing.Size(1088, 60);
            this.menuStrip1.Stretch = false;
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem_file
            // 
            this.toolStripMenuItem_file.AutoSize = false;
            this.toolStripMenuItem_file.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStripMenuItem_file.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_new,
            this.toolStripMenuItem_open,
            this.toolStripMenuItem_save,
            this.toolStripMenuItem_export,
            this.toolStripMenuItem_close});
            this.toolStripMenuItem_file.Image = global::varai2d_surface.Properties.Resources.menupic_file;
            this.toolStripMenuItem_file.Name = "toolStripMenuItem_file";
            this.toolStripMenuItem_file.Padding = new System.Windows.Forms.Padding(0);
            this.toolStripMenuItem_file.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_file.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.toolStripMenuItem_file.ToolTipText = "File Menu";
            // 
            // toolStripMenuItem_new
            // 
            this.toolStripMenuItem_new.Image = global::varai2d_surface.Properties.Resources.NewM;
            this.toolStripMenuItem_new.Name = "toolStripMenuItem_new";
            this.toolStripMenuItem_new.Size = new System.Drawing.Size(135, 26);
            this.toolStripMenuItem_new.Text = "New";
            this.toolStripMenuItem_new.Click += new System.EventHandler(this.toolStripMenuItem_new_ItemClicked);
            // 
            // toolStripMenuItem_open
            // 
            this.toolStripMenuItem_open.Image = global::varai2d_surface.Properties.Resources.OpenM;
            this.toolStripMenuItem_open.Name = "toolStripMenuItem_open";
            this.toolStripMenuItem_open.Size = new System.Drawing.Size(135, 26);
            this.toolStripMenuItem_open.Text = "Open";
            this.toolStripMenuItem_open.Click += new System.EventHandler(this.toolStripMenuItem_open_ItemClicked);
            // 
            // toolStripMenuItem_save
            // 
            this.toolStripMenuItem_save.Image = global::varai2d_surface.Properties.Resources.saveM;
            this.toolStripMenuItem_save.Name = "toolStripMenuItem_save";
            this.toolStripMenuItem_save.Size = new System.Drawing.Size(135, 26);
            this.toolStripMenuItem_save.Text = "Save";
            this.toolStripMenuItem_save.Click += new System.EventHandler(this.toolStripMenuItem_save_ItemClicked);
            // 
            // toolStripMenuItem_export
            // 
            this.toolStripMenuItem_export.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_rawdatatxt,
            this.toolStripMenuItem_screenshotbmp});
            this.toolStripMenuItem_export.Image = global::varai2d_surface.Properties.Resources.export;
            this.toolStripMenuItem_export.Name = "toolStripMenuItem_export";
            this.toolStripMenuItem_export.Size = new System.Drawing.Size(135, 26);
            this.toolStripMenuItem_export.Text = "Export";
            // 
            // toolStripMenuItem_rawdatatxt
            // 
            this.toolStripMenuItem_rawdatatxt.Name = "toolStripMenuItem_rawdatatxt";
            this.toolStripMenuItem_rawdatatxt.Size = new System.Drawing.Size(194, 26);
            this.toolStripMenuItem_rawdatatxt.Text = "Raw data (*.txt)";
            this.toolStripMenuItem_rawdatatxt.Click += new System.EventHandler(this.toolStripMenuItem_rawdatatxt_ItemClicked);
            // 
            // toolStripMenuItem_screenshotbmp
            // 
            this.toolStripMenuItem_screenshotbmp.Name = "toolStripMenuItem_screenshotbmp";
            this.toolStripMenuItem_screenshotbmp.Size = new System.Drawing.Size(194, 26);
            this.toolStripMenuItem_screenshotbmp.Text = "Picture (*.png)";
            this.toolStripMenuItem_screenshotbmp.Click += new System.EventHandler(this.toolStripMenuItem_screenshotbmp_ItemClicked);
            // 
            // toolStripMenuItem_close
            // 
            this.toolStripMenuItem_close.Image = global::varai2d_surface.Properties.Resources.exitM;
            this.toolStripMenuItem_close.Name = "toolStripMenuItem_close";
            this.toolStripMenuItem_close.Size = new System.Drawing.Size(135, 26);
            this.toolStripMenuItem_close.Text = "Close";
            this.toolStripMenuItem_close.Click += new System.EventHandler(this.toolStripMenuItem_close_ItemClicked);
            // 
            // toolStripMenuItem_divider1
            // 
            this.toolStripMenuItem_divider1.AutoSize = false;
            this.toolStripMenuItem_divider1.Name = "toolStripMenuItem_divider1";
            this.toolStripMenuItem_divider1.Size = new System.Drawing.Size(22, 60);
            // 
            // toolStripMenuItem_select
            // 
            this.toolStripMenuItem_select.AutoSize = false;
            this.toolStripMenuItem_select.Image = global::varai2d_surface.Properties.Resources.menupic_select;
            this.toolStripMenuItem_select.Name = "toolStripMenuItem_select";
            this.toolStripMenuItem_select.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_select.ToolTipText = "Select member";
            this.toolStripMenuItem_select.Click += new System.EventHandler(this.toolStripMenuItem_select_ItemClicked);
            // 
            // toolStripMenuItem_addLine
            // 
            this.toolStripMenuItem_addLine.AutoSize = false;
            this.toolStripMenuItem_addLine.Image = global::varai2d_surface.Properties.Resources.menupic_addline;
            this.toolStripMenuItem_addLine.Name = "toolStripMenuItem_addLine";
            this.toolStripMenuItem_addLine.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_addLine.ToolTipText = "Add Line (two points)";
            this.toolStripMenuItem_addLine.Click += new System.EventHandler(this.toolStripMenuItem_addLine_ItemClicked);
            // 
            // toolStripMenuItem_addCircle
            // 
            this.toolStripMenuItem_addCircle.AutoSize = false;
            this.toolStripMenuItem_addCircle.Image = global::varai2d_surface.Properties.Resources.menupic_addcircle;
            this.toolStripMenuItem_addCircle.Name = "toolStripMenuItem_addCircle";
            this.toolStripMenuItem_addCircle.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_addCircle.ToolTipText = "Add Circle (center + radius)";
            this.toolStripMenuItem_addCircle.Click += new System.EventHandler(this.toolStripMenuItem_addCircle_ItemClicked);
            // 
            // toolStripMenuItem_addarc1
            // 
            this.toolStripMenuItem_addarc1.AutoSize = false;
            this.toolStripMenuItem_addarc1.Image = global::varai2d_surface.Properties.Resources.menupic_addarc1;
            this.toolStripMenuItem_addarc1.Name = "toolStripMenuItem_addarc1";
            this.toolStripMenuItem_addarc1.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_addarc1.ToolTipText = "Add Arc (chord length + radius)";
            this.toolStripMenuItem_addarc1.Click += new System.EventHandler(this.toolStripMenuItem_addarc1_ItemClicked);
            // 
            // toolStripMenuItem_addarc2
            // 
            this.toolStripMenuItem_addarc2.AutoSize = false;
            this.toolStripMenuItem_addarc2.Image = global::varai2d_surface.Properties.Resources.menupic_addarc2;
            this.toolStripMenuItem_addarc2.Name = "toolStripMenuItem_addarc2";
            this.toolStripMenuItem_addarc2.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_addarc2.ToolTipText = "Add Arc (radius + sector angle)";
            this.toolStripMenuItem_addarc2.Click += new System.EventHandler(this.toolStripMenuItem_addarc2_ItemClicked);
            // 
            // toolStripMenuItem_addbezier
            // 
            this.toolStripMenuItem_addbezier.AutoSize = false;
            this.toolStripMenuItem_addbezier.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_bezier3pt,
            this.toolStripMenuItem_bezier4pt,
            this.toolStripMenuItem_bezier5pt});
            this.toolStripMenuItem_addbezier.Image = global::varai2d_surface.Properties.Resources.menupic_addbezier;
            this.toolStripMenuItem_addbezier.Name = "toolStripMenuItem_addbezier";
            this.toolStripMenuItem_addbezier.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_addbezier.ToolTipText = "Add Bezier (n - points)";
            this.toolStripMenuItem_addbezier.Click += new System.EventHandler(this.toolStripMenuItem_addbezier_ItemClicked);
            // 
            // toolStripMenuItem_bezier3pt
            // 
            this.toolStripMenuItem_bezier3pt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem_bezier3pt.Name = "toolStripMenuItem_bezier3pt";
            this.toolStripMenuItem_bezier3pt.Size = new System.Drawing.Size(182, 26);
            this.toolStripMenuItem_bezier3pt.Text = "3 Point Bezier";
            this.toolStripMenuItem_bezier3pt.Click += new System.EventHandler(this.toolStripMenuItem_3ptbezier_ItemClicked);
            // 
            // toolStripMenuItem_bezier4pt
            // 
            this.toolStripMenuItem_bezier4pt.Checked = true;
            this.toolStripMenuItem_bezier4pt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem_bezier4pt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem_bezier4pt.Name = "toolStripMenuItem_bezier4pt";
            this.toolStripMenuItem_bezier4pt.Size = new System.Drawing.Size(182, 26);
            this.toolStripMenuItem_bezier4pt.Text = "4 Point Bezier";
            this.toolStripMenuItem_bezier4pt.Click += new System.EventHandler(this.toolStripMenuItem_4ptbezier_ItemClicked);
            // 
            // toolStripMenuItem_bezier5pt
            // 
            this.toolStripMenuItem_bezier5pt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem_bezier5pt.Name = "toolStripMenuItem_bezier5pt";
            this.toolStripMenuItem_bezier5pt.Size = new System.Drawing.Size(182, 26);
            this.toolStripMenuItem_bezier5pt.Text = "5 Point Bezier";
            this.toolStripMenuItem_bezier5pt.Click += new System.EventHandler(this.toolStripMenuItem_5ptbezier_ItemClicked);
            // 
            // toolStripMenuItem_scale_to_fit
            // 
            this.toolStripMenuItem_scale_to_fit.AutoSize = false;
            this.toolStripMenuItem_scale_to_fit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMenuItem_scale_to_fit.Image = global::varai2d_surface.Properties.Resources.scale_to_fit;
            this.toolStripMenuItem_scale_to_fit.Name = "toolStripMenuItem_scale_to_fit";
            this.toolStripMenuItem_scale_to_fit.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_scale_to_fit.Click += new System.EventHandler(this.toolStripMenuItem_scale_to_fit_ItemClicked);
            // 
            // toolStripMenuItem_divider2
            // 
            this.toolStripMenuItem_divider2.AutoSize = false;
            this.toolStripMenuItem_divider2.Name = "toolStripMenuItem_divider2";
            this.toolStripMenuItem_divider2.Size = new System.Drawing.Size(22, 60);
            // 
            // toolStripMenuItem_delete
            // 
            this.toolStripMenuItem_delete.AutoSize = false;
            this.toolStripMenuItem_delete.Image = global::varai2d_surface.Properties.Resources.menupic_delete;
            this.toolStripMenuItem_delete.Name = "toolStripMenuItem_delete";
            this.toolStripMenuItem_delete.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_delete.ToolTipText = "Delete member";
            this.toolStripMenuItem_delete.Click += new System.EventHandler(this.toolStripMenuItem_delete_ItemClicked);
            // 
            // toolStripMenuItem_duplicate
            // 
            this.toolStripMenuItem_duplicate.AutoSize = false;
            this.toolStripMenuItem_duplicate.Image = global::varai2d_surface.Properties.Resources.duplicate;
            this.toolStripMenuItem_duplicate.Name = "toolStripMenuItem_duplicate";
            this.toolStripMenuItem_duplicate.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_duplicate.ToolTipText = "Duplicate while transform";
            this.toolStripMenuItem_duplicate.Click += new System.EventHandler(this.toolStripMenuItem_duplicate_ItemClicked);
            // 
            // toolStripMenuItem_translate
            // 
            this.toolStripMenuItem_translate.AutoSize = false;
            this.toolStripMenuItem_translate.Image = global::varai2d_surface.Properties.Resources.menupic_translate;
            this.toolStripMenuItem_translate.Name = "toolStripMenuItem_translate";
            this.toolStripMenuItem_translate.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_translate.ToolTipText = "Translate transform";
            this.toolStripMenuItem_translate.Click += new System.EventHandler(this.toolStripMenuItem_translate_ItemClicked);
            // 
            // toolStripMenuItem_rotate
            // 
            this.toolStripMenuItem_rotate.AutoSize = false;
            this.toolStripMenuItem_rotate.Image = global::varai2d_surface.Properties.Resources.menupic_rotate;
            this.toolStripMenuItem_rotate.Name = "toolStripMenuItem_rotate";
            this.toolStripMenuItem_rotate.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_rotate.ToolTipText = "Rotate transform";
            this.toolStripMenuItem_rotate.Click += new System.EventHandler(this.toolStripMenuItem_rotate_ItemClicked);
            // 
            // toolStripMenuItem_mirror
            // 
            this.toolStripMenuItem_mirror.AutoSize = false;
            this.toolStripMenuItem_mirror.Image = global::varai2d_surface.Properties.Resources.menupic_mirror;
            this.toolStripMenuItem_mirror.Name = "toolStripMenuItem_mirror";
            this.toolStripMenuItem_mirror.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_mirror.ToolTipText = "Mirror transform";
            this.toolStripMenuItem_mirror.Click += new System.EventHandler(this.toolStripMenuItem_mirror_ItemClicked);
            // 
            // toolStripMenuItem_divider3
            // 
            this.toolStripMenuItem_divider3.AutoSize = false;
            this.toolStripMenuItem_divider3.Name = "toolStripMenuItem_divider3";
            this.toolStripMenuItem_divider3.Size = new System.Drawing.Size(22, 60);
            // 
            // toolStripMenuItem_intersect
            // 
            this.toolStripMenuItem_intersect.AutoSize = false;
            this.toolStripMenuItem_intersect.Image = global::varai2d_surface.Properties.Resources.menupic_intersect;
            this.toolStripMenuItem_intersect.Name = "toolStripMenuItem_intersect";
            this.toolStripMenuItem_intersect.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_intersect.ToolTipText = "Split member at intersection";
            this.toolStripMenuItem_intersect.Click += new System.EventHandler(this.toolStripMenuItem_intersect_ItemClicked);
            // 
            // toolStripMenuItem_splitline
            // 
            this.toolStripMenuItem_splitline.AutoSize = false;
            this.toolStripMenuItem_splitline.Image = global::varai2d_surface.Properties.Resources.menupic_splitline;
            this.toolStripMenuItem_splitline.Name = "toolStripMenuItem_splitline";
            this.toolStripMenuItem_splitline.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_splitline.ToolTipText = "Split member in-between";
            this.toolStripMenuItem_splitline.Click += new System.EventHandler(this.toolStripMenuItem_splitline_ItemClicked);
            // 
            // toolStripMenuItem_divider4
            // 
            this.toolStripMenuItem_divider4.AutoSize = false;
            this.toolStripMenuItem_divider4.Name = "toolStripMenuItem_divider4";
            this.toolStripMenuItem_divider4.Size = new System.Drawing.Size(22, 60);
            // 
            // toolStripMenuItem_addsurface
            // 
            this.toolStripMenuItem_addsurface.AutoSize = false;
            this.toolStripMenuItem_addsurface.Image = global::varai2d_surface.Properties.Resources.menupic_addsurface;
            this.toolStripMenuItem_addsurface.Name = "toolStripMenuItem_addsurface";
            this.toolStripMenuItem_addsurface.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_addsurface.ToolTipText = "Add surface (click on closed curve)";
            this.toolStripMenuItem_addsurface.Click += new System.EventHandler(this.toolStripMenuItem_addsurface_ItemClicked);
            // 
            // toolStripMenuItem_deletesurface
            // 
            this.toolStripMenuItem_deletesurface.AutoSize = false;
            this.toolStripMenuItem_deletesurface.Image = global::varai2d_surface.Properties.Resources.menupic_deletesurface;
            this.toolStripMenuItem_deletesurface.Name = "toolStripMenuItem_deletesurface";
            this.toolStripMenuItem_deletesurface.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_deletesurface.ToolTipText = "Delete surface";
            this.toolStripMenuItem_deletesurface.Click += new System.EventHandler(this.toolStripMenuItem_deletesurface_ItemClicked);
            // 
            // toolStripMenuItem_options
            // 
            this.toolStripMenuItem_options.AutoSize = false;
            this.toolStripMenuItem_options.Image = global::varai2d_surface.Properties.Resources.menupic_options;
            this.toolStripMenuItem_options.Name = "toolStripMenuItem_options";
            this.toolStripMenuItem_options.Size = new System.Drawing.Size(54, 60);
            this.toolStripMenuItem_options.ToolTipText = "Options";
            this.toolStripMenuItem_options.Click += new System.EventHandler(this.toolStripMenuItem_options_ItemClicked);
            // 
            // toolStripMenuItem_divider5
            // 
            this.toolStripMenuItem_divider5.AutoSize = false;
            this.toolStripMenuItem_divider5.Name = "toolStripMenuItem_divider5";
            this.toolStripMenuItem_divider5.Size = new System.Drawing.Size(22, 60);
            // 
            // main_pic
            // 
            this.main_pic.BackColor = System.Drawing.Color.White;
            this.main_pic.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.main_pic.Controls.Add(this.mt_pic);
            this.main_pic.Location = new System.Drawing.Point(28, 130);
            this.main_pic.Name = "main_pic";
            this.main_pic.Size = new System.Drawing.Size(135, 118);
            this.main_pic.TabIndex = 2;
            this.main_pic.KeyUp += new System.Windows.Forms.KeyEventHandler(this.main_pic_KeyUp);
            this.main_pic.KeyDown += new System.Windows.Forms.KeyEventHandler(this.main_pic_KeyDown);
            this.main_pic.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.main_pic_KeyPress);
            this.main_pic.SizeChanged += new System.EventHandler(this.main_pic_SizeChanged);
            this.main_pic.Paint += new System.Windows.Forms.PaintEventHandler(this.main_pic_Paint);
            this.main_pic.MouseClick += new System.Windows.Forms.MouseEventHandler(this.main_pic_MouseClick);
            this.main_pic.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.main_pic_MouseDoubleClick);
            this.main_pic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.main_pic_MouseDown);
            this.main_pic.MouseEnter += new System.EventHandler(this.main_pic_MouseEnter);
            this.main_pic.MouseMove += new System.Windows.Forms.MouseEventHandler(this.main_pic_MouseMove);
            this.main_pic.MouseUp += new System.Windows.Forms.MouseEventHandler(this.main_pic_MouseUp);
            this.main_pic.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.main_pic_MouseWheel);
            // 
            // mt_pic
            // 
            this.mt_pic.BackColor = System.Drawing.Color.Transparent;
            this.mt_pic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mt_pic.Enabled = false;
            this.mt_pic.Location = new System.Drawing.Point(0, 0);
            this.mt_pic.Name = "mt_pic";
            this.mt_pic.Size = new System.Drawing.Size(131, 114);
            this.mt_pic.TabIndex = 0;
            this.mt_pic.TabStop = false;
            // 
            // txt_keyboard_inpt
            // 
            this.txt_keyboard_inpt.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txt_keyboard_inpt.Location = new System.Drawing.Point(443, 87);
            this.txt_keyboard_inpt.Name = "txt_keyboard_inpt";
            this.txt_keyboard_inpt.Size = new System.Drawing.Size(125, 30);
            this.txt_keyboard_inpt.TabIndex = 3;
            this.txt_keyboard_inpt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_keyboard_inpt_KeyDown);
            // 
            // main_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1088, 493);
            this.Controls.Add(this.txt_keyboard_inpt);
            this.Controls.Add(this.main_pic);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(540, 540);
            this.Name = "main_form";
            this.Text = "Varai2D ---- Developed by Samson Mano <https://https://sites.google.com/site/sams" +
    "oninfinite/>";
            this.Load += new System.EventHandler(this.main_form_Load);
            this.SizeChanged += new System.EventHandler(this.main_form_SizeChanged);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.main_pic.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mt_pic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_file;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem_divider1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_addLine;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_addCircle;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_addarc1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_addarc2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_addbezier;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_select;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem_divider2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_delete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_duplicate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_translate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_rotate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_mirror;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_intersect;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem_divider3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_addsurface;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_deletesurface;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem_divider4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_options;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem_divider5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_new;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_open;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_save;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_close;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_splitline;
        private System.Windows.Forms.Panel main_pic;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel_tooltip;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel_zoom;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel_scale;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_error;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_bezier3pt;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_bezier4pt;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_bezier5pt;
        private System.Windows.Forms.TextBox txt_keyboard_inpt;
        public System.Windows.Forms.PictureBox mt_pic;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_scale_to_fit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_export;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_rawdatatxt;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_screenshotbmp;
    }
}