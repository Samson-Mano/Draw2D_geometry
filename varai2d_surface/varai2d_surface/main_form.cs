using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using varai2d_surface.Geometry_class;
using varai2d_surface.Geometry_class.geometry_store;
using varai2d_surface.global_static;
using varai2d_surface.Drawing_area;
using varai2d_surface.export_control;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace varai2d_surface
{
    public partial class main_form : Form
    {
        private readonly object drawLock = new object();
        private workarea_control wkc_obj = new workarea_control();
        surface_form_api surf_frm;

        #region "Tool bar events"
        #region "File menu events"
        private void toolStripMenuItem_new_ItemClicked(object sender, EventArgs e)
        {
            // File -> New
            DialogResult dlg_rslt;
            if (wkc_obj.geom_obj.all_end_pts.Count != 0)
            {
                dlg_rslt = MessageBox.Show("Do you want to start new model? Unsaved items will be discarded", "Samson Mano", MessageBoxButtons.YesNoCancel);

                if (dlg_rslt == DialogResult.No)
                {
                    toolStripMenuItem_save_ItemClicked(sender, e);
                    return;
                }
                else if (dlg_rslt == DialogResult.Cancel)
                {
                    return;
                }
            }


            // No user action performed start new project
            // geom = new Geometry_class.geom_class();
            wkc_obj = new workarea_control();

            // update main_pic backcolor
            // main_pic.Location = new Point(5, menuStrip1.Height + 5);
            // main_pic.Size = new Size(this.Width - 30, this.Height - menuStrip1.Height -statusStrip1.Height - 20);
            main_pic.Dock = DockStyle.Fill;
            main_pic.Visible = true;

            // update tool bars
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            update_toolbar_checked_state = "0,1,0,0,0,0,0,0,0,0";
            set_statusstrip_label = toolbarstate.get_status_tooltip(1);

            // Set the mainpic details
            gvariables.mainpic_size = new SizeF(main_pic.Width, main_pic.Height);
            gvariables.mainpic_center = new PointF(main_pic.Width * 0.5f, main_pic.Height * 0.5f);
            gvariables.zoom_factor = 1.01f;
            statusStripLabel_zoom.Text = "Zoom: " + (gfunctions.RoundOff((int)(gvariables.zoom_factor * 100))).ToString() + "%";
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_open_ItemClicked(object sender, EventArgs e)
        {
            // File -> Open
            DialogResult dlg_rslt;
            if (wkc_obj.geom_obj.all_end_pts.Count != 0)
            {
                dlg_rslt = MessageBox.Show("Do you want to open new model? Unsaved items will be discarded", "Samson Mano", MessageBoxButtons.YesNoCancel);

                if (dlg_rslt == DialogResult.No)
                {
                    toolStripMenuItem_save_ItemClicked(sender, e);
                    return;
                }
                else if (dlg_rslt == DialogResult.Cancel)
                {
                    return;
                }
            }

            OpenFileDialog ow = new OpenFileDialog();
            ow.DefaultExt = "*.2ds";
            ow.Filter = "Samson Mano's Varai2D - Drawing Object Files (*.2ds)|*.2ds";
            ow.ShowDialog();

            if (File.Exists(ow.FileName) == true)
            {
                List<object> trobject = new List<object>();
                Stream gsf = File.OpenRead(ow.FileName);
                BinaryFormatter desrlizer = new BinaryFormatter();

                try
                {
#pragma warning disable SYSLIB0011
                    trobject = (List<object>)desrlizer.Deserialize(gsf);
#pragma warning restore SYSLIB0011

                    this.wkc_obj = new workarea_control();
                    this.wkc_obj = (workarea_control)trobject[0];
                    gvariables.zoom_factor = (double)trobject[1];
                    gvariables.scale_factor = (double)trobject[2];
                    gvariables.mainpic_center = (PointF)trobject[3];

                    // special case to handle [NonSerialized] System.Drawing.Region data in the surface_store
                    foreach (surface_store surf in this.wkc_obj.geom_obj.all_surfaces)
                    {
                        surf.set_nonserialized_surface_region();
                    }


                    statusStripLabel_scale.Text = "Scale: " + gvariables.scale_factor.ToString("F3");
                    statusStripLabel_zoom.Text = "Zoom: " + (gfunctions.RoundOff((int)(gvariables.zoom_factor * 100))).ToString() + "%";
                    mt_pic.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Sorry!!!!! Unable to Open.. File Reading Error" + ex.Message, "Samson Mano", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void toolStripMenuItem_save_ItemClicked(object sender, EventArgs e)
        {
            // File -> Save
            if (wkc_obj.geom_obj.all_end_pts.Count != 0)
            {
                SaveFileDialog sw = new SaveFileDialog();
                sw.DefaultExt = "*.2ds";
                sw.Filter = "Samson Mano's Varai2D - Drawing Object Files (*.2ds)|*.2ds";
                sw.FileName = "Surface";
                DialogResult = sw.ShowDialog();

                try
                {
                    // Add to objects
                    List<object> trobject = new List<object>();
                    trobject.Add(this.wkc_obj);
                    trobject.Add(gvariables.zoom_factor);
                    trobject.Add(gvariables.scale_factor);
                    trobject.Add(gvariables.mainpic_center);

                    Stream psf = File.Create(sw.FileName);
                    BinaryFormatter srlizer = new BinaryFormatter();

#pragma warning disable SYSLIB0011
                    srlizer.Serialize(psf, trobject);
#pragma warning restore SYSLIB0011

                    psf.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Sorry!!!!! Unable to Save.. File Writing Error" + ex.Message, "Samson Mano", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripMenuItem_rawdatatxt_ItemClicked(object sender, EventArgs e)
        {
            // File -> Export -> Raw data
            if (wkc_obj.geom_obj.all_end_pts.Count != 0)
            {
                SaveFileDialog sw = new SaveFileDialog();
                sw.DefaultExt = "*.txt";
                sw.Filter = "Samson Mano's Varai2D - txt Files (*.txt)|*.txt";
                sw.FileName = "surface_rd";
                DialogResult = sw.ShowDialog();

                try
                {
                    string[] lines = new export_file_raw_data_txt(wkc_obj).output_str;
                    using StreamWriter file = new(sw.FileName);

                    foreach (string line in lines)
                    {
                        file.WriteLine(line);

                    }

                    file.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Sorry!!!!! Unable to Save.. File Writing Error" + ex.Message, "Samson Mano", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // toolStripMenuItem_screenshotbmp

        private void toolStripMenuItem_screenshotbmp_ItemClicked(object sender, EventArgs e)
        {
            // File -> Export -> Picture
            if (wkc_obj.geom_obj.all_end_pts.Count != 0)
            {
                using (Bitmap bmp = new Bitmap( main_pic.Width, main_pic.Height))
                {
                    mt_pic.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

                    SaveFileDialog sw = new SaveFileDialog();
                    sw.DefaultExt = "*.png";
                    sw.Filter = "|Png Image (.png)|*.png| Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg|Tiff Image (.tiff)|*.tiff|Wmf Image (.wmf)|*.wmf";
                    sw.FileName = "surface_png";
                    DialogResult = sw.ShowDialog();

                    try
                    {
                        for(int x =0;x< bmp.Width; x++)
                        {
                            for (int y =0;y< bmp.Height;y++)
                            {
                                if(bmp.GetPixel(x,y).R == gvariables.color_mainpic.R &&
                                   bmp.GetPixel(x, y).G == gvariables.color_mainpic.G &&
                                   bmp.GetPixel(x, y).B == gvariables.color_mainpic.B )
                                {
                                    // Make the background transparent
                                    bmp.SetPixel(x, y, Color.FromArgb(0, gvariables.color_mainpic));
                                }
                            }
                        }
                        // Save as PNG
                        bmp.Save(sw.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Sorry!!!!! Unable to Save.. File Writing Error" + ex.Message, "Samson Mano", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void toolStripMenuItem_close_ItemClicked(object sender, EventArgs e)
        {
            // File -> Close
            DialogResult dlg_rslt;
            if (wkc_obj.geom_obj.all_end_pts.Count != 0)
            {
                dlg_rslt = MessageBox.Show("Do you want to exit without saving? Unsaved items will be discarded", "Samson Mano", MessageBoxButtons.YesNoCancel);

                if (dlg_rslt == DialogResult.No)
                {
                    toolStripMenuItem_save_ItemClicked(sender, e);
                    return;
                }
                else if (dlg_rslt == DialogResult.Cancel)
                {
                    return;
                }
            }

            this.Close();
        }
        #endregion

        #region "Tool bar supporting Properties"
        private string update_toolbar_checked_state
        {
            set
            {
                // Writeonly property to update all the checked box at once
                // Update the tool bar checked state
                toolbarstate.update_toolbar_checkedstatus(value);

                string[] str_cstate = value.Split(',');
                Color back_color = Control.DefaultBackColor;
                Color highlight_color = Color.FromArgb(ProfessionalColors.ButtonCheckedHighlight.ToArgb());

                toolStripMenuItem_select.BackColor = Convert.ToInt32(str_cstate[0]) == 0 ? back_color : highlight_color;
                toolStripMenuItem_addLine.BackColor = Convert.ToInt32(str_cstate[1]) == 0 ? back_color : highlight_color;

                toolStripMenuItem_addCircle.BackColor = Convert.ToInt32(str_cstate[2]) == 0 ? back_color : highlight_color;
                toolStripMenuItem_addarc1.BackColor = Convert.ToInt32(str_cstate[3]) == 0 ? back_color : highlight_color;
                toolStripMenuItem_addarc2.BackColor = Convert.ToInt32(str_cstate[4]) == 0 ? back_color : highlight_color;
                toolStripMenuItem_addbezier.BackColor = Convert.ToInt32(str_cstate[5]) == 0 ? back_color : highlight_color;


                toolStripMenuItem_translate.BackColor = Convert.ToInt32(str_cstate[6]) == 0 ? back_color : highlight_color;
                toolStripMenuItem_rotate.BackColor = Convert.ToInt32(str_cstate[7]) == 0 ? back_color : highlight_color;
                toolStripMenuItem_mirror.BackColor = Convert.ToInt32(str_cstate[8]) == 0 ? back_color : highlight_color;

                toolStripMenuItem_addsurface.BackColor = Convert.ToInt32(str_cstate[9]) == 0 ? back_color : highlight_color;

                // Dispose surface form
                if (Convert.ToInt32(str_cstate[9]) == 0 && gvariables.Is_surface_frm_open == true)
                {
                    gvariables.Is_surface_frm_open = false;
                    surf_frm.Close();
                }
            }
        }

        private string enable_toolbars_buttons
        {
            set
            {
                string[] str_cstate = value.Split(',');
                // Writeonly property to set the checked state of tool bars
                // Add tool bars
                toolStripMenuItem_select.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[0]));
                toolStripMenuItem_addLine.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[1]));
                toolStripMenuItem_addCircle.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[2]));
                toolStripMenuItem_addarc1.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[3]));
                toolStripMenuItem_addarc2.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[4]));
                toolStripMenuItem_addbezier.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[5]));
                toolStripMenuItem_scale_to_fit.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[0]));

                // Modify tool bars
                toolStripMenuItem_translate.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[6]));
                toolStripMenuItem_duplicate.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[6]));
                toolStripMenuItem_rotate.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[7]));
                toolStripMenuItem_mirror.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[8]));

                toolStripMenuItem_delete.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[9]));
                toolStripMenuItem_intersect.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[10]));
                toolStripMenuItem_splitline.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[11]));

                // Surface tool bars
                toolStripMenuItem_addsurface.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[12]));
                toolStripMenuItem_options.Enabled = Convert.ToBoolean(Convert.ToInt32(str_cstate[14]));

            }
        }

        private string set_statusstrip_label
        {
            set
            {
                // Status strip tool tip to help user
                statusStripLabel_tooltip.Text = value;
            }
        }
        #endregion

        #region "Select & Add member events"
        private void toolStripMenuItem_select_ItemClicked(object sender, EventArgs e)
        {
            // 0 - Select member
            deleteTextBox();
            update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            set_statusstrip_label = toolbarstate.get_status_tooltip(0);
            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_addLine_ItemClicked(object sender, EventArgs e)
        {
            // 1 - Add Line
            deleteTextBox();
            update_toolbar_checked_state = "0,1,0,0,0,0,0,0,0,0";
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            set_statusstrip_label = toolbarstate.get_status_tooltip(1);
            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_addCircle_ItemClicked(object sender, EventArgs e)
        {
            // 2 - Add circle
            deleteTextBox();
            update_toolbar_checked_state = "0,0,1,0,0,0,0,0,0,0";
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            set_statusstrip_label = toolbarstate.get_status_tooltip(2);
            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_addarc1_ItemClicked(object sender, EventArgs e)
        {
            // 3 - Add Arc 1 (chord length and radius)
            deleteTextBox();
            update_toolbar_checked_state = "0,0,0,1,0,0,0,0,0,0";
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            set_statusstrip_label = toolbarstate.get_status_tooltip(3);
            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_addarc2_ItemClicked(object sender, EventArgs e)
        {
            // 4 - Add Arc 2 (radius and sector angle)
            deleteTextBox();
            update_toolbar_checked_state = "0,0,0,0,1,0,0,0,0,0";
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            set_statusstrip_label = toolbarstate.get_status_tooltip(4);
            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_addbezier_ItemClicked(object sender, EventArgs e)
        {
            // 5 - Add Bezier (n-points Bezier curve)
            deleteTextBox();
            update_toolbar_checked_state = "0,0,0,0,0,1,0,0,0,0";
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            set_statusstrip_label = toolbarstate.get_status_tooltip(5);
            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_3ptbezier_ItemClicked(object sender, EventArgs e)
        {
            // 5A - Bezier 3 pt curve
            deleteTextBox();
            gvariables.bezier_n_count = 3;
            toolStripMenuItem_bezier3pt.Checked = true;
            toolStripMenuItem_bezier4pt.Checked = false;
            toolStripMenuItem_bezier5pt.Checked = false;

            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_4ptbezier_ItemClicked(object sender, EventArgs e)
        {
            // 5A - Bezier 4 pt curve
            deleteTextBox();
            gvariables.bezier_n_count = 4;
            toolStripMenuItem_bezier3pt.Checked = false;
            toolStripMenuItem_bezier4pt.Checked = true;
            toolStripMenuItem_bezier5pt.Checked = false;

            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_5ptbezier_ItemClicked(object sender, EventArgs e)
        {
            // 5A - Bezier 5 pt curve
            deleteTextBox();
            gvariables.bezier_n_count = 5;
            toolStripMenuItem_bezier3pt.Checked = false;
            toolStripMenuItem_bezier4pt.Checked = false;
            toolStripMenuItem_bezier5pt.Checked = true;

            wkc_obj.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_scale_to_fit_ItemClicked(object sender, EventArgs e)
        {
            if (gvariables.Is_surface_frm_open == true || wkc_obj.interim_obj.Is_selected == true)
                return;

            // Set view to Fit the canvas
            // Cancel the operation in progress (if any)
            deleteTextBox();
            wkc_obj.mouse_click(true, gvariables.cursor_on_movept);
            wkc_obj.scale_to_fit(main_pic.Width, main_pic.Height);
            wkc_obj.cancel_operation();


            statusStripLabel_scale.Text = "Scale: " + gvariables.scale_factor.ToString("F3");
            statusStripLabel_zoom.Text = "Zoom: " + (gfunctions.RoundOff((int)(gvariables.zoom_factor * 100))).ToString() + "%";
            mt_pic.Refresh();
        }
        #endregion

        #region "Delete & Modify member events"
        private void toolStripMenuItem_delete_ItemClicked(object sender, EventArgs e)
        {
            // Delete member
            if (wkc_obj.interim_obj.Is_selected == true)
            {
                wkc_obj.button_click(9);
                // Update the toolbar enabled and checked state
                enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                mt_pic.Refresh();
            }
        }

        private void toolStripMenuItem_duplicate_ItemClicked(object sender, EventArgs e)
        {
            // Duplicate control (Checkbox)
            if (wkc_obj.interim_obj.Is_selected == true)
            {
                if (gvariables.Is_duplicate == true)
                {
                    toolStripMenuItem_duplicate.BackColor = Control.DefaultBackColor;
                    gvariables.Is_duplicate = false;
                }
                else
                {
                    toolStripMenuItem_duplicate.BackColor = Color.FromArgb(ProfessionalColors.ButtonCheckedHighlight.ToArgb());
                    gvariables.Is_duplicate = true;
                }
                mt_pic.Refresh();
            }
        }

        private void toolStripMenuItem_translate_ItemClicked(object sender, EventArgs e)
        {
            // 6 - Translate member
            if (toolbarstate.toolbar_rotate_Ischecked == false)
            {
                // Click on check translate operation
                update_toolbar_checked_state = "1,0,0,0,0,0,1,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(6);
            }
            else
            {
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(0);
            }

            // histU.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_rotate_ItemClicked(object sender, EventArgs e)
        {
            // 7 - Rotate member
            if (toolbarstate.toolbar_rotate_Ischecked == false)
            {
                // Click on check rotate operation
                update_toolbar_checked_state = "1,0,0,0,0,0,0,1,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(7);
            }
            else
            {
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(0);
            }

            //  histU.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_mirror_ItemClicked(object sender, EventArgs e)
        {
            // 8 - Mirror member
            if (toolbarstate.toolbar_rotate_Ischecked == false)
            {
                // Click on check mirror operation
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,1,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(7);
            }
            else
            {
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(0);
            }

            //  histU.cancel_operation();
            mt_pic.Refresh();
        }

        private void toolStripMenuItem_intersect_ItemClicked(object sender, EventArgs e)
        {
            // Intersect member
            if (wkc_obj.interim_obj.Is_selected == true)
            {
                wkc_obj.button_click(10);
                // Update the toolbar enabled and checked state
                deleteTextBox();
                enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                mt_pic.Refresh();
            }
        }

        private void toolStripMenuItem_splitline_ItemClicked(object sender, EventArgs e)
        {
            // Split member
            if (wkc_obj.interim_obj.Is_selected == true)
            {
                wkc_obj.button_click(11);
                // Update the toolbar enabled and checked state
                deleteTextBox();
                enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
                update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                mt_pic.Refresh();
            }
        }
        #endregion

        #region "Surface & option events"
        private void toolStripMenuItem_addsurface_ItemClicked(object sender, EventArgs e)
        {
            // Add Surface
            if (gvariables.Is_surface_frm_open == true)
                return;

            deleteTextBox();
            wkc_obj.cancel_operation();
            surf_frm = new surface_form_api(this, wkc_obj);

            set_statusstrip_label = toolbarstate.get_status_tooltip(9);
            enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
            update_toolbar_checked_state = "0,0,0,0,0,0,0,0,0,1";
            gvariables.Is_surface_frm_open = true;
            surf_frm.Show();
        }

        public void surface_frm_closed()
        {
            if (gvariables.Is_surface_frm_open == true)
            {
                update_toolbar_checked_state = "0,1,0,0,0,0,0,0,0,0";
                set_statusstrip_label = toolbarstate.get_status_tooltip(1);
            }
        }


        private void toolStripMenuItem_deletesurface_ItemClicked(object sender, EventArgs e)
        {
            // Delete Surface

        }

        private void toolStripMenuItem_options_ItemClicked(object sender, EventArgs e)
        {
            // Options menu
            deleteTextBox();
            wkc_obj.cancel_operation();
            Form opt_frm = new options_form(this);
            opt_frm.ShowDialog();
        }
        #endregion
        #endregion

        #region "Main Panel Events"
        private void main_pic_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            // paint background color and axis
            wkc_obj.interim_obj.paint_background_axis(e.Graphics);

            // save graphics state before the transformation
            GraphicsState gr_state = e.Graphics.Save();

            // Scale & Translate transform
            e.Graphics.ScaleTransform((float)gvariables.zoom_factor, (float)gvariables.zoom_factor);
            e.Graphics.TranslateTransform(gvariables.mainpic_center.X, gvariables.mainpic_center.Y);

            // Paint snap line
            wkc_obj.snap_obj.paint_snap_lines(e.Graphics);

            // Paint Transitionary Geometry
            bool only_paint_transitory = false;
            only_paint_transitory = wkc_obj.interim_obj.paint_interim_objects(e.Graphics);

            // Paint Geometry
            wkc_obj.geom_obj.paint_geometry(e.Graphics, only_paint_transitory);
        }

        #region "Intelli - Zoom Events"
        private void main_pic_MouseEnter(object sender, EventArgs e)
        {
            if (gvariables.Is_surface_frm_open == true)
                return;

            // Keep the focus on main pic
            main_pic.Focus();
        }

        private void main_pic_MouseWheel(object sender, MouseEventArgs e)
        {
            if (gvariables.Is_cntrldown == true)
            {
                // Intelli - zoom operation
                double xw, yw;
                main_pic.Focus();

                // Find the x,y (mouse location before zoom)
                xw = ((double)e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X;
                yw = ((double)e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y;

                if (e.Delta > 0)
                {
                    // zoom in
                    if (gvariables.zoom_factor < 100)
                        gvariables.zoom_factor += 0.1f;
                }
                else if (e.Delta < 0)
                {
                    // zoom out
                    if (gvariables.zoom_factor > 0.11)
                        gvariables.zoom_factor -= 0.1f;
                }

                // Find the translation of mouse pt due to zoom
                double t_tx, t_ty;
                t_tx = ((double)e.X / gvariables.zoom_factor) - xw;
                t_ty = ((double)e.Y / gvariables.zoom_factor) - yw;

                gvariables.mainpic_center = new PointF((float)t_tx, (float)t_ty);

                //Refresh the canvas
                statusStripLabel_zoom.Text = "Zoom: " + (gfunctions.RoundOff((int)(gvariables.zoom_factor * 100))).ToString() + "%";
                mt_pic.Refresh();
            }
        }

        private void main_pic_SizeChanged(object sender, EventArgs e)
        {
            // Update the canvas size
            gvariables.mainpic_size = new SizeF(main_pic.Width, main_pic.Height);
            gvariables.mainpic_center = new PointF(main_pic.Width * 0.5f, main_pic.Height * 0.5f);
            mt_pic.Refresh();
        }


        private void main_form_SizeChanged(object sender, EventArgs e)
        {
            main_pic.Dock = DockStyle.Fill;
            // Update the size of mainpic
            //main_pic.Location = new Point(2, menuStrip1.Height + 5);
            //main_pic.Size = new Size(this.Width - 20, this.Height - menuStrip1.Height - 20);

        }
        #endregion

        #region "Keyboard events"
        private void main_pic_KeyDown(object sender, KeyEventArgs e)
        {
            // MessageBox.Show(e.KeyCode.ToString());
            // User press and hold cntrl key
            gvariables.Is_cntrldown = false;
            if (e.Control == true)
            {
                gvariables.Is_cntrldown = true;

                if (e.KeyCode == Keys.F)
                {
                    // Set view to Fit the canvas
                    // Cancel the operation in progress (if any)
                    wkc_obj.mouse_click(true, gvariables.cursor_on_movept);
                    wkc_obj.scale_to_fit(main_pic.Width, main_pic.Height);
                    gvariables.Is_cntrldown = false;

                    statusStripLabel_scale.Text = "Scale: " + gvariables.scale_factor.ToString("F3");
                    statusStripLabel_zoom.Text = "Zoom: " + (gfunctions.RoundOff((int)(gvariables.zoom_factor * 100))).ToString() + "%";
                }
                else if (e.KeyCode == Keys.Z)
                {
                    // Undo
                    // Cancel the operation in progress (if any)
                    wkc_obj.mouse_click(true, gvariables.cursor_on_movept);
                    wkc_obj.cntrl_Z();
                    gvariables.Is_cntrldown = false;
                }
                else if (e.KeyCode == Keys.R)
                {
                    // Redo
                    // Cancel the operation in progress (if any)
                    wkc_obj.mouse_click(true, gvariables.cursor_on_movept);
                    wkc_obj.cntrl_R();
                    gvariables.Is_cntrldown = false;
                }
            }

            gvariables.Is_shiftdown = false;
            if (e.Shift == true)
            {
                gvariables.Is_shiftdown = true;
            }

            // Trigger delete
            if (e.KeyCode == Keys.Delete)
            {
                if (toolStripMenuItem_delete.Enabled == true)
                {
                    toolStripMenuItem_delete_ItemClicked(sender, e);
                }
            }

            if (gvariables.Is_txtboxliveflg == true)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Allowing user to create object by pressing Enter
                    txt_keyboard_inpt_KeyDown(sender, e);
                }
                else if (e.KeyCode == Keys.Space)
                {
                    // Press the space key to get into focus and selct all the text for easy edit
                    txt_keyboard_inpt.SelectAll();
                    txt_keyboard_inpt.Focus();
                }
                else if (check_key_input_Isvalid(e) == true)
                {
                    // If numeric keys are pressed keep the focus in txt_keyboard_input
                    txt_keyboard_inpt.Focus();

                }
                else
                {
                    // Any other keys shift the focus to main_pic
                    main_pic.Focus();
                    txt_keyboard_inpt.Text = wkc_obj.interim_obj.str_txt_box;
                    e.SuppressKeyPress = true;
                }
            }
            mt_pic.Refresh();
        }

        private void main_pic_KeyUp(object sender, KeyEventArgs e)
        {
            if (gvariables.Is_cntrldown == true)
            {
                // Release the control key
                gvariables.Is_cntrldown = false;
            }

            if (gvariables.Is_shiftdown == true)
            {
                // Release the shift key
                gvariables.Is_shiftdown = false;
            }
            mt_pic.Refresh();
        }

        private void main_pic_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Key press to trigger keyboard input
        }

        #region "TextBox Keyboard input control"
        private void createTextBox()
        {
            txt_keyboard_inpt.BackColor = gvariables.color_mainpic;
            txt_keyboard_inpt.BorderStyle = BorderStyle.None;

            // Get the initial value 
            txt_keyboard_inpt.Text = wkc_obj.interim_obj.str_txt_box;

            // Adjust the size of the Textbox
            Size size = TextRenderer.MeasureText(txt_keyboard_inpt.Text, txt_keyboard_inpt.Font);
            txt_keyboard_inpt.Width = size.Width;
            txt_keyboard_inpt.Height = size.Height;

            // Show the text box
            txt_keyboard_inpt.ForeColor = gvariables.color_txtforecolor;
            txt_keyboard_inpt.Show();

            gvariables.Is_txtboxliveflg = true;
        }

        private void updateTextBox(Point cursor_loc)
        {
            if (gvariables.Is_txtboxliveflg == true)
            {
                // Get the current dynamic value
                txt_keyboard_inpt.Text = wkc_obj.interim_obj.str_txt_box;

                // Adjust the size of the Textbox
                Size size = TextRenderer.MeasureText(txt_keyboard_inpt.Text, txt_keyboard_inpt.Font);
                txt_keyboard_inpt.Width = size.Width;
                txt_keyboard_inpt.Height = size.Height;

                // Update the Location
                txt_keyboard_inpt.Location = new Point(cursor_loc.X + 20, cursor_loc.Y + menuStrip1.Height + 25);

                // Keep mainpic focus when mouse moved along with the textbox
                main_pic.Focus();
            }
        }

        private void txt_keyboard_inpt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Check whether only angle input
                bool is_only_angle_input = false;
                bool is_only_length_input = false;

                if (wkc_obj.interim_obj.Is_addarc1 == true && wkc_obj.interim_obj.click_pts.Count == 2)
                {
                    is_only_length_input = true;
                }

                if (wkc_obj.interim_obj.Is_mirror == true || wkc_obj.interim_obj.Is_rotate == true)
                {
                    is_only_angle_input = true;
                }

                // Check the data for format ###.##,##.###
                if (text_input_control.is_textvalue_length_angle_valid(txt_keyboard_inpt.Text, is_only_length_input, is_only_angle_input) == true)
                {
                    // Get the length and angle
                    Tuple<double, double> l_a = text_input_control.get_textvalue_length_angle(txt_keyboard_inpt.Text, is_only_length_input, is_only_angle_input);
                    double Length_l = l_a.Item1 * gvariables.scale_factor;
                    double angle_a = -1.0 * l_a.Item2 * (Math.PI / 180);

                    double dx = Length_l * Math.Cos(angle_a);
                    double dy = Length_l * Math.Sin(angle_a);

                    // Send click point information to interim object to handle
                    PointF click_pt = new PointF((float)(wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 1].x + dx),
                        (float)(wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 1].y + dy));

                    // Special case for adding arc radius
                    if (wkc_obj.interim_obj.Is_addarc1 == true && wkc_obj.interim_obj.click_pts.Count == 2)
                    {
                        // Then find the center of arc point
                        double chord_length = gfunctions.get_length(wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 2].get_point,
                            wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 1].get_point);

                        // check the validity (Chord Length <= 2 * Radius
                        Length_l = Length_l + 0.00001;
                        if (chord_length <= (2 * Length_l))
                        {
                            // Find first center
                            PointF chrd_midpt = gfunctions.get_midpoint(wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 2].get_point,
                                                                wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 1].get_point);

                            PointF rad_pt1, rad_pt2;

                            double ht = Math.Sqrt((Length_l * Length_l) - (0.5 * 0.5 * chord_length * chord_length));
                            double norm_slope = ((wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 2].get_point.X -
                                wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 1].get_point.X) /
                                (wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 1].get_point.Y -
                                wkc_obj.interim_obj.click_pts[wkc_obj.interim_obj.click_pts.Count - 2].get_point.Y));

                            rad_pt1 = gfunctions.get_point_at_l_m(chrd_midpt, norm_slope, ht, 1);
                            rad_pt2 = gfunctions.get_point_at_l_m(chrd_midpt, norm_slope, ht, -1);

                            double diff1 = Math.Abs(Length_l - gfunctions.get_length(rad_pt1, gvariables.cursor_on_movept));
                            double diff2 = Math.Abs(Length_l - gfunctions.get_length(rad_pt2, gvariables.cursor_on_movept));

                            PointF cpt_1, cpt_2;
                            double dir_slope;
                            if (diff1 < diff2)
                            {
                                dir_slope = ((gvariables.cursor_on_movept.Y - rad_pt1.Y) /
                                                       (gvariables.cursor_on_movept.X - rad_pt1.X));
                                // Radius Point 1
                                cpt_1 = gfunctions.get_point_at_l_m(rad_pt1, dir_slope, Length_l, 1);
                                cpt_2 = gfunctions.get_point_at_l_m(rad_pt1, dir_slope, Length_l, -1);

                                diff1 = gfunctions.get_length(cpt_1, gvariables.cursor_on_movept);
                                diff2 = gfunctions.get_length(cpt_2, gvariables.cursor_on_movept);

                                if (diff1 < diff2)
                                {
                                    click_pt = cpt_1;
                                }
                                else
                                {
                                    click_pt = cpt_2;
                                }
                            }
                            else
                            {
                                dir_slope = ((gvariables.cursor_on_movept.Y - rad_pt2.Y) /
                                                        (gvariables.cursor_on_movept.X - rad_pt2.X));
                                // Radius Point 2
                                cpt_1 = gfunctions.get_point_at_l_m(rad_pt2, dir_slope, Length_l, 1);
                                cpt_2 = gfunctions.get_point_at_l_m(rad_pt2, dir_slope, Length_l, -1);

                                diff1 = gfunctions.get_length(cpt_1, gvariables.cursor_on_movept);
                                diff2 = gfunctions.get_length(cpt_2, gvariables.cursor_on_movept);

                                if (diff1 < diff2)
                                {
                                    click_pt = cpt_1;
                                }
                                else
                                {
                                    click_pt = cpt_2;
                                }

                            }

                        }
                        else
                        {
                            return;
                        }
                    }


                    bool continue_operation = false;
                    continue_operation = wkc_obj.mouse_click(false, click_pt);

                    // Show text box
                    if (continue_operation == true)
                    {
                        createTextBox();
                    }
                    else
                    {
                        // Hide the text box (keyboard input)
                        deleteTextBox();

                        // Operation ended so disable the modify tool bars
                        enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
                        if (toolbarstate.toolbar_select_Ischecked == true)
                        {
                            update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                            set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                        }
                    }

                    mt_pic.Refresh();
                }
                else
                {
                    // Revise texbox data if numerical input is invalid
                    txt_keyboard_inpt.Text = wkc_obj.interim_obj.str_txt_box;
                    main_pic.Focus();
                    e.SuppressKeyPress = true;
                }
                e.SuppressKeyPress = true;
            }
            else if (check_key_input_Isvalid(e) == true)
            {
                // If numeric keys are pressed keep the focus in txt_keyboard_input
                txt_keyboard_inpt.Focus();
            }
            else
            {
                txt_keyboard_inpt.Text = wkc_obj.interim_obj.str_txt_box;
                main_pic.Focus();
                e.SuppressKeyPress = true;
            }
            gvariables.Is_cntrldown = false;
            gvariables.Is_shiftdown = false;
        }

        private bool check_key_input_Isvalid(KeyEventArgs e1)
        {
            if ((e1.KeyCode >= Keys.D0 && e1.KeyCode <= Keys.D9) ||
                  (e1.KeyCode >= Keys.NumPad0 && e1.KeyCode <= Keys.NumPad9) ||
                  e1.KeyCode == Keys.Decimal || e1.KeyCode == Keys.Oemcomma || e1.KeyCode == Keys.OemPeriod ||
                  e1.KeyCode == Keys.Back || e1.KeyCode == Keys.Delete ||
                    e1.KeyCode == Keys.Left || e1.KeyCode == Keys.Right ||
                    e1.KeyCode == Keys.OemMinus || e1.KeyCode == Keys.Subtract)
            {
                return true;
            }

            return false;
        }

        private void deleteTextBox()
        {
            main_pic.Focus();
            if (gvariables.Is_txtboxliveflg == true)
            {
                // Hide the text box
                txt_keyboard_inpt.Hide();
                gvariables.Is_txtboxliveflg = false;
            }
        }
        #endregion
        #endregion

        #region "Mouse click events"
        private void main_pic_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (gvariables.Is_cntrldown == true)
                {
                    // start the pan operation
                    gvariables.Is_panflg = true;
                    gvariables.cursor_on_clickpt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                                                            (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                    mt_pic.Refresh();
                }
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                // Is select option chosen 
                if (toolbarstate.toolbar_select_Ischecked == true)
                {
                    if (toolbarstate.toolbar_translate_Ischecked == true && wkc_obj.interim_obj.Is_selected == true)
                    {
                        // Translate option is checked (and members are selected)

                        mt_pic.Refresh();
                        return;
                    }

                    if (toolbarstate.toolbar_rotate_Ischecked == true && wkc_obj.interim_obj.Is_selected == true)
                    {
                        // Rotate option is checked (and members are selected)

                        mt_pic.Refresh();
                        return;
                    }

                    if (toolbarstate.toolbar_mirror_Ischecked == true && wkc_obj.interim_obj.Is_selected == true)
                    {
                        // Mirror option is checked (and members are selected)

                        mt_pic.Refresh();
                        return;
                    }

                    // start the selection operation
                    gvariables.Is_selectflg = true;
                    gvariables.cursor_on_clickpt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                                                            (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                    wkc_obj.interim_obj.Is_selection = true;
                    // histU.selection_operation_inprogress(true);

                    mt_pic.Refresh();
                }
                return;
            }

        }

        private void main_pic_MouseUp(object sender, MouseEventArgs e)
        {
            if (gvariables.Is_panflg == true)
            {
                // Pan operation completes
                gvariables.Is_panflg = false;
                mt_pic.Refresh();
            }

            if (gvariables.Is_selectflg == true)
            {
                if (toolbarstate.toolbar_translate_Ischecked == true && wkc_obj.interim_obj.Is_selected == true)
                {
                    // Translate option is checked (and members are selected)
                    mt_pic.Refresh();
                    return;
                }

                if (toolbarstate.toolbar_rotate_Ischecked == true && wkc_obj.interim_obj.Is_selected == true)
                {
                    // Rotate option is checked (and members are selected)
                    mt_pic.Refresh();
                    return;
                }

                if (toolbarstate.toolbar_mirror_Ischecked == true && wkc_obj.interim_obj.Is_selected == true)
                {
                    // Mirror option is checked (and members are selected)
                    mt_pic.Refresh();
                    return;
                }

                // Selection operation completes
                gvariables.Is_selectflg = false;
                wkc_obj.interim_obj.Is_selection = false;
                // histU.selection_operation_inprogress(false);
                // Select Rectangle
                float min_x, min_y, max_x, max_y;
                float rect_width, rect_height;
                min_x = Math.Min(gvariables.cursor_on_clickpt.X, (float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X);
                min_y = Math.Min(gvariables.cursor_on_clickpt.Y, (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                max_x = Math.Max(gvariables.cursor_on_clickpt.X, (float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X);
                max_y = Math.Max(gvariables.cursor_on_clickpt.Y, (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                // rectangle size
                rect_width = max_x - min_x;
                rect_height = max_y - min_y;
                // Selection rectangle
                RectangleF select_rect = new RectangleF(min_x, min_y, rect_width, rect_height);
                wkc_obj.select_objects(select_rect, gvariables.Is_shiftdown);

                // Update the tool bar operation if any members are selected
                if (wkc_obj.interim_obj.Is_selected == true)
                {
                    // If any items selected (Enable the modify operation)
                    if ((wkc_obj.interim_obj.selected_lines.Count + wkc_obj.interim_obj.selected_arcs.Count + wkc_obj.interim_obj.selected_beziers.Count) == 2)
                    {
                        // Enable intersect toolbars only when the count is 2
                        enable_toolbars_buttons = "1,1,1,1,1,1,1,1,1,1,1,0,1,1,1";
                    }
                    else if ((wkc_obj.interim_obj.selected_lines.Count + wkc_obj.interim_obj.selected_arcs.Count + wkc_obj.interim_obj.selected_beziers.Count) == 1)
                    {
                        enable_toolbars_buttons = "1,1,1,1,1,1,1,1,1,1,0,1,1,1,1";
                    }
                    else
                    {
                        enable_toolbars_buttons = "1,1,1,1,1,1,1,1,1,1,0,0,1,1,1";
                    }
                    deleteTextBox();
                    update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                    set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                }
                else
                {
                    // Diable the modify operation
                    deleteTextBox();
                    enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
                    update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                    set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                }
                mt_pic.Refresh();
            }
        }

        private void main_pic_MouseMove(object sender, MouseEventArgs e)
        {
            gvariables.cursor_on_movept = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                                                    (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
            // Check snap when add & modify in progress
            if (toolbarstate.get_toolchecked_state > 0 && toolbarstate.get_toolchecked_state < 9)
            {
                if (wkc_obj.snap_obj.Check_snap(gvariables.cursor_on_movept, wkc_obj.geom_obj.all_end_pts) == true)
                {
                    mt_pic.Refresh();
                }
                else if (wkc_obj.snap_obj.Is_snapped == true)
                {
                    mt_pic.Refresh();
                    wkc_obj.snap_obj.Is_snapped = false;
                }
            }

            // Pan operation in progress
            if (gvariables.Is_panflg == true)
            {
                gvariables.mainpic_center = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.cursor_on_clickpt.X,
                                                    (float)(e.Y / gvariables.zoom_factor) - gvariables.cursor_on_clickpt.Y);


                mt_pic.Refresh();
            }

            // Selection operation in progress
            if (gvariables.Is_selectflg == true)
            {
                gvariables.cursor_on_movept = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                    (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);

                // Selection operation along with modify (translate, rotate, mirror)
                wkc_obj.interim_obj.cursor_pt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                        (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                mt_pic.Refresh();
            }

            if (wkc_obj.interim_obj.Is_addline == true || wkc_obj.interim_obj.Is_addarc1 == true || wkc_obj.interim_obj.Is_addbezier == true)
            {
                // Add member is in progress
                wkc_obj.interim_obj.cursor_pt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                     (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                updateTextBox(e.Location);
                mt_pic.Refresh();
            }

            if (wkc_obj.interim_obj.Is_translate == true || wkc_obj.interim_obj.Is_rotate == true || wkc_obj.interim_obj.Is_mirror == true)
            {
                // Modify member is in progress
                wkc_obj.interim_obj.cursor_pt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                     (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                updateTextBox(e.Location);
                mt_pic.Refresh();
            }

            toolStripStatusLabel_error.Text = (gvariables.cursor_on_movept.X / gvariables.scale_factor).ToString(gvariables.pt_coord_pres) + ", " +
                ((-1.0f * gvariables.cursor_on_movept.Y) / gvariables.scale_factor).ToString(gvariables.pt_coord_pres);
        }

        private void main_pic_MouseClick(object sender, MouseEventArgs e)
        {
            bool continue_operation = false;
            if (gvariables.Is_selectflg == true || gvariables.Is_panflg == true)
                return;

            if (gvariables.Is_surface_frm_open == true)
            {
                // Send click point information to surface form to handle
                PointF click_pt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                    (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);
                surf_frm.main_pic_click(click_pt);
                mt_pic.Refresh();
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                // Send click point information to interim object to handle
                PointF click_pt = new PointF((float)(e.X / gvariables.zoom_factor) - gvariables.mainpic_center.X,
                    (float)(e.Y / gvariables.zoom_factor) - gvariables.mainpic_center.Y);

                lock (drawLock)
                {
                    continue_operation = wkc_obj.mouse_click(false, click_pt);

                    // Show text box
                    if (continue_operation == true)
                    {
                        createTextBox();
                    }
                }
            }
            else
            {
                // other than left mouse click cancel event
                lock (drawLock)
                {
                    continue_operation = wkc_obj.mouse_click(true, e.Location);
                }
            }

            if (continue_operation == false)
            {
                // Hide the text box (keyboard input)
                deleteTextBox();

                // Operation ended so disable the modify tool bars
                enable_toolbars_buttons = "1,1,1,1,1,1,0,0,0,0,0,0,1,1,1";
                if (toolbarstate.toolbar_select_Ischecked == true)
                {
                    update_toolbar_checked_state = "1,0,0,0,0,0,0,0,0,0";
                    set_statusstrip_label = toolbarstate.get_status_tooltip(0);
                }
            }

            mt_pic.Refresh();
        }

        private void main_pic_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
        #endregion
        #endregion



        public main_form()
        {
            InitializeComponent();
        }

        private void main_form_Load(object sender, EventArgs e)
        {
            // Load preliminaries
            main_pic.Visible = false; // No drawing canvas till user starts new project
            enable_toolbars_buttons = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0"; // Disable the tool bars at start (till new project is started)

            // Add circle & Add Arc2 buttons are removed (its essentially doing the same thing as Add arc1)
            toolStripMenuItem_addCircle.Visible = false;
            toolStripMenuItem_addarc2.Visible = false;
            toolStripMenuItem_deletesurface.Visible = false;
            txt_keyboard_inpt.Visible = false;


            set_statusstrip_label = toolbarstate.get_status_tooltip(-1);

            toolStripMenuItem_new_ItemClicked(sender, e);
            gfunctions.set_default_global_variable();
        }

    }
}
