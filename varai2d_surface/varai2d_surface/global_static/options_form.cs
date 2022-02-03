using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using varai2d_surface.Properties;

namespace varai2d_surface.global_static
{
    public partial class options_form : Form
    {
        private main_form the_parent;

        // Default values
        Color color_mainpic_dflt = Color.White;
        Color color_txtforecolor_dflt = Color.DarkMagenta;
        int linewidth_curves_dflt = 3;
        int radius_points_dflt = 3;
        int snaplinewidth_dflt = 1;
        int str_font_size_dflt = 10;

        Color color_memberclr_dflt = Color.BlueViolet;
        Color color_pointsclr_dflt = Color.DarkRed;
        Color color_snaplineclr_dflt = Color.DarkOrange;
        Color color_stringforeclr_dflt = Color.DarkSeaGreen;

        string pt_coord_pres_dflt = "F3";
        string ln_length_pres_dflt = "F3";

        // snap option control
        bool Is_hvsnap_dflt = true;
        double hvsnap_intensity_dflt = 2.0;
        bool Is_xysnap_dflt = true;
        double xysnap_intensity_dflt = 4.0;

        // paint node
        bool Is_paint_pt_dflt = true;
        bool Is_paint_ptid_dflt = true;
        bool Is_paint_ptcoord_dflt = false;
        bool Is_paint_memberid_dflt = true;
        bool Is_paint_memberlength_dflt = true;
        bool Is_paint_surfaceid_dflt = true;

        public options_form(main_form m_frm)
        {
            InitializeComponent();
            // Initialize the Parent.
            the_parent = m_frm;
            the_parent.mt_pic.Refresh();
            // Fill all the combo box
            fill_combo_box();
        }

        private void options_form_Load(object sender, EventArgs e)
        {
            // Set the combobox values to current values
            set_combo_values();

        }

        private void fill_combo_box()
        {
            // Color menus
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            // KnownColor[] allColors = new KnownColor[colorsArray.Length];

            foreach (KnownColor knowColor in colorsArray)
            {
                comboBox_backcolor.Items.Add(knowColor);
                comboBox_pointcolor.Items.Add(knowColor);
                comboBox_membercolor.Items.Add(knowColor);
                comboBox_snaplinecolor.Items.Add(knowColor);
                comboBox_txtforecolor.Items.Add(knowColor);
            }


            // Width addition
            for (int i = 1; i < 6; i++)
            {
                comboBox_memberwidth.Items.Add(i);
                comboBox_pointradius.Items.Add(i);
                comboBox_snaplinewidth.Items.Add(i);
            }

            // Intensity addition
            double db = 0;
            for (int i = 1; i < 101; i++)
            {
                db = ((double)i) / 10.0f;
                comboBox_ptsnapintensity.Items.Add(db);
                comboBox_hvsnapintensity.Items.Add(db);
            }

            // Font Size addition
            for (int i =4;i<21;i++)
            {
                comboBox_txtsize.Items.Add(i);
            }

            // Precision addition
            comboBox_preccoord.Items.Add("0");
            comboBox_preccoord.Items.Add("0.0");
            comboBox_preccoord.Items.Add("0.00");
            comboBox_preccoord.Items.Add("0.000");
            comboBox_preccoord.Items.Add("0.0000");
            comboBox_preccoord.Items.Add("0.00000");

            comboBox_preclength.Items.Add("0");
            comboBox_preclength.Items.Add("0.0");
            comboBox_preclength.Items.Add("0.00");
            comboBox_preclength.Items.Add("0.000");
            comboBox_preclength.Items.Add("0.0000");
            comboBox_preclength.Items.Add("0.00000");
        }

        private void set_combo_values()
        {
            checkBox_showpoints.Checked = gvariables.Is_paint_pt;
            checkBox_showpointids.Checked = gvariables.Is_paint_ptid;
            checkBox_showcoord.Checked = gvariables.Is_paint_ptcoord;
            checkBox_showmemberid.Checked = gvariables.Is_paint_memberid;
            checkBox_showmemberlengths.Checked = gvariables.Is_paint_memberlength;
            checkBox_showsurfid.Checked = gvariables.Is_paint_surfaceid;

            comboBox_txtsize.SelectedIndex = get_combo_current_index_fontsize(gvariables.str_font_size);

            // Snap to point & horizontal/ vertical
            checkBox_snaptopt.Checked = gvariables.Is_xysnap;
            checkBox_snaptohv.Checked = gvariables.Is_hvsnap;

            // Combo precision
            comboBox_preccoord.SelectedIndex = get_combo_current_index_precision(gvariables.pt_coord_pres);
            comboBox_preclength.SelectedIndex = get_combo_current_index_precision(gvariables.ln_length_pres);

            // Combobox list
            comboBox_backcolor.SelectedIndex = get_combo_current_index_color(gvariables.color_mainpic.Name.ToString());
            comboBox_pointcolor.SelectedIndex = get_combo_current_index_color(gvariables.color_pointsclr.Name.ToString());
            comboBox_membercolor.SelectedIndex = get_combo_current_index_color(gvariables.color_memberclr.Name.ToString());
            comboBox_snaplinecolor.SelectedIndex = get_combo_current_index_color(gvariables.color_snaplineclr.Name.ToString());
            comboBox_txtforecolor.SelectedIndex = get_combo_current_index_color(gvariables.color_stringforeclr.Name.ToString());
            // comboBox_backcolor.SelectedText = color_mainpic_dflt.Name;

            // Point radius, member width, snap width
            comboBox_pointradius.SelectedIndex = get_combo_current_index_width(gvariables.radius_points);
            comboBox_memberwidth.SelectedIndex = get_combo_current_index_width(gvariables.linewidth_curves);
            comboBox_snaplinewidth.SelectedIndex = get_combo_current_index_width(gvariables.linewidth_snapline);

            // Snap intensity
            comboBox_ptsnapintensity.SelectedIndex = get_combo_current_index_snapintensity(gvariables.xysnap_intensity);
            comboBox_hvsnapintensity.SelectedIndex = get_combo_current_index_snapintensity(gvariables.hvsnap_intensity);

        }

        private void set_combo_values_default()
        {

            // Adjust the global variables
            gvariables.Is_paint_pt = Is_paint_pt_dflt;
            gvariables.Is_paint_ptid = Is_paint_ptid_dflt;
            gvariables.Is_paint_ptcoord = Is_paint_ptcoord_dflt;
            gvariables.Is_paint_memberid = Is_paint_memberid_dflt;
            gvariables.Is_paint_memberlength = Is_paint_memberlength_dflt;
            gvariables.Is_paint_surfaceid = Is_paint_surfaceid_dflt;

            gvariables.str_font_size = str_font_size_dflt;

            // Snap to point & horizontal/ vertical
            gvariables.Is_xysnap = Is_xysnap_dflt;
            gvariables.Is_hvsnap = Is_hvsnap_dflt;

            // Combo precision
            gvariables.pt_coord_pres = pt_coord_pres_dflt;
            gvariables.ln_length_pres = ln_length_pres_dflt;

            // Combobox list
            gvariables.color_mainpic = color_mainpic_dflt;
            gvariables.color_pointsclr = color_pointsclr_dflt;
            gvariables.color_memberclr = color_memberclr_dflt;
            gvariables.color_snaplineclr = color_snaplineclr_dflt;
            gvariables.color_stringforeclr = color_stringforeclr_dflt;

            // Point radius, member width, snap width
            gvariables.radius_points = radius_points_dflt;
            gvariables.linewidth_curves = linewidth_curves_dflt;
            gvariables.linewidth_snapline = snaplinewidth_dflt;

            // Snap intensity
            gvariables.xysnap_intensity = xysnap_intensity_dflt;
            gvariables.hvsnap_intensity = hvsnap_intensity_dflt;

            set_combo_values();
        }

        private int get_combo_current_index_color(string s_value)
        {
            int i = 0;
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            foreach (KnownColor knowColor in colorsArray)
            {
                if (s_value == knowColor.ToString())
                {
                    return i;
                }
                i++;
            }
            return 0;
        }

        private int get_combo_current_index_snapintensity(double d_value)
        {
            int i = 0;
            for (i = 1; i < 101; i++)
            {
                double db = ((double)i) / 10.0f;

                if (d_value == db)
                {
                    return (i - 1);
                }
            }
            return 0;
        }

        private int get_combo_current_index_width(int i_value)
        {
            for (int i = 1; i < 6; i++)
            {
                if (i == i_value)
                {
                    return (i - 1);
                }
            }
            return 0;
        }

        private int get_combo_current_index_fontsize(int i_value)
        {
            for (int i = 4; i < 21; i++)
            {
                if (i == i_value)
                {
                    return (i - 4);
                }
            }
            return 0;
        }

        private int get_combo_current_index_precision(string s_value)
        {
            int a;
            int.TryParse(s_value[1].ToString(), out a);
            return a;
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            // Button cancel
            // global variables revert back to saved settings
            gvariables.color_mainpic = Settings.Default.Sett_mainpic;
            gvariables.color_memberclr = Settings.Default.Sett_member_clr;
            gvariables.color_pointsclr = Settings.Default.Sett_point_clr;
            gvariables.color_snaplineclr = Settings.Default.Sett_snapline_clr;
            gvariables.color_stringforeclr = Settings.Default.Sett_stringfore_clr;

            gvariables.str_font_size = Settings.Default.Sett_font_size;

            // Point radius, member width, snap width
            gvariables.linewidth_curves = Settings.Default.Sett_member_width;
            gvariables.radius_points = Settings.Default.Sett_point_radius;
            gvariables.linewidth_snapline = Settings.Default.Sett_snapline_width;

            // Snap to point & horizontal/ vertical
            gvariables.Is_hvsnap = Settings.Default.Sett_horiz_vert_snap;
            gvariables.Is_xysnap = Settings.Default.Sett_point_snap;

            // Snap intensity
            gvariables.hvsnap_intensity = Settings.Default.Sett_horiz_vert_intensity;
            gvariables.xysnap_intensity = Settings.Default.Sett_point_intensity;

            gvariables.Is_paint_pt = Settings.Default.Sett_is_paint_pt;
            gvariables.Is_paint_ptcoord = Settings.Default.Sett_is_paint_ptcoord;
            gvariables.Is_paint_ptid = Settings.Default.Sett_is_paint_ptid;
            gvariables.Is_paint_memberid = Settings.Default.Sett_is_paint_memid;
            gvariables.Is_paint_memberlength = Settings.Default.Sett_is_paint_memlength;
            gvariables.Is_paint_surfaceid = Settings.Default.Sett_is_paint_surfid;

            // Combo precision
            gvariables.pt_coord_pres = Settings.Default.Sett_coord_pres;
            gvariables.ln_length_pres = Settings.Default.Sett_length_pres;

            the_parent.mt_pic.Refresh();
            this.Close();
        }


        private void button_defaults_Click(object sender, EventArgs e)
        {
            // Default options
            set_combo_values_default();
            the_parent.mt_pic.Refresh();
            // this.Close();
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            // Button ok
            // Save the settings to global variables
            Settings.Default.Sett_mainpic = gvariables.color_mainpic;
            Settings.Default.Sett_member_clr = gvariables.color_memberclr;
            Settings.Default.Sett_point_clr = gvariables.color_pointsclr;
            Settings.Default.Sett_snapline_clr = gvariables.color_snaplineclr;
            Settings.Default.Sett_stringfore_clr = gvariables.color_stringforeclr;

            Settings.Default.Sett_font_size = gvariables.str_font_size;

            // Point radius, member width, snap width
            Settings.Default.Sett_member_width = gvariables.linewidth_curves;
            Settings.Default.Sett_point_radius = gvariables.radius_points;
            Settings.Default.Sett_snapline_width = gvariables.linewidth_snapline;

            // Snap to point & horizontal/ vertical
            Settings.Default.Sett_horiz_vert_snap = gvariables.Is_hvsnap;
            Settings.Default.Sett_point_snap = gvariables.Is_xysnap;

            // Snap intensity
            Settings.Default.Sett_horiz_vert_intensity = gvariables.hvsnap_intensity;
            Settings.Default.Sett_point_intensity = gvariables.xysnap_intensity;

            Settings.Default.Sett_is_paint_pt = gvariables.Is_paint_pt;
            Settings.Default.Sett_is_paint_ptcoord = gvariables.Is_paint_ptcoord;
            Settings.Default.Sett_is_paint_ptid = gvariables.Is_paint_ptid;
            Settings.Default.Sett_is_paint_memid = gvariables.Is_paint_memberid;
            Settings.Default.Sett_is_paint_memlength = gvariables.Is_paint_memberlength;
            Settings.Default.Sett_is_paint_surfid = gvariables.Is_paint_surfaceid;

            // Combo precision
            Settings.Default.Sett_coord_pres = gvariables.pt_coord_pres;
            Settings.Default.Sett_length_pres = gvariables.ln_length_pres;

            Settings.Default.Save();
            the_parent.mt_pic.Refresh();
            this.Close();
        }

        #region "General Tab"
        private void checkBox_showpoints_CheckedChanged(object sender, EventArgs e)
        {
            // Show Points
            gvariables.Is_paint_pt = checkBox_showpoints.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void checkBox_showpointids_CheckedChanged(object sender, EventArgs e)
        {
            // Show point id
            gvariables.Is_paint_ptid = checkBox_showpointids.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void checkBox_showcoord_CheckedChanged(object sender, EventArgs e)
        {
            // Show point coordinate
            gvariables.Is_paint_ptcoord = checkBox_showcoord.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void checkBox_showmemberid_CheckedChanged(object sender, EventArgs e)
        {
            // Show member id
            gvariables.Is_paint_memberid = checkBox_showmemberid.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void checkBox_showmemberlengths_CheckedChanged(object sender, EventArgs e)
        {
            // Show member Length
            gvariables.Is_paint_memberlength = checkBox_showmemberlengths.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_preccoord_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Co-ordinate precision
            gvariables.pt_coord_pres = "F" + comboBox_preccoord.SelectedIndex;
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_preclength_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Length precision
            gvariables.ln_length_pres = "F" + comboBox_preclength.SelectedIndex;
            the_parent.mt_pic.Refresh();
        }

        private void checkBox_showsurfid_CheckedChanged(object sender, EventArgs e)
        {
            // Show surface id
            gvariables.Is_paint_surfaceid = checkBox_showsurfid.Checked;
            the_parent.mt_pic.Refresh();
        }

        #endregion

        #region"Display setting"
        private void comboBox_backcolor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Mainpic backcolor
            gvariables.color_mainpic = Color.FromName(comboBox_backcolor.SelectedItem.ToString());
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_pointcolor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Point color
            gvariables.color_pointsclr = Color.FromName(comboBox_pointcolor.SelectedItem.ToString());
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_pointradius_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Point radius
            int rslt;
            int.TryParse(comboBox_pointradius.SelectedItem.ToString(), out rslt);
            gvariables.radius_points = rslt;
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_membercolor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Member color
            gvariables.color_memberclr = Color.FromName(comboBox_membercolor.SelectedItem.ToString());
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_memberwidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Member width
            int rslt;
            int.TryParse(comboBox_memberwidth.SelectedItem.ToString(), out rslt);
            gvariables.linewidth_curves = rslt;
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_snaplinecolor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Snap line color
            gvariables.color_snaplineclr = Color.FromName(comboBox_snaplinecolor.SelectedItem.ToString());
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_snaplinewidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Snap line width
            int rslt;
            int.TryParse(comboBox_snaplinewidth.SelectedItem.ToString(), out rslt);
            gvariables.linewidth_snapline = rslt;
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_txtforecolor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // String fore color
            gvariables.color_stringforeclr = Color.FromName(comboBox_txtforecolor.SelectedItem.ToString());
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }


        private void comboBox_txtsize_SelectedIndexChanged(object sender, EventArgs e)
        {
            // String Size
            int rslt;
            int.TryParse(comboBox_txtsize.SelectedItem.ToString(), out rslt);
            gvariables.str_font_size = rslt;
            gfunctions.update_pen();
            the_parent.mt_pic.Refresh();
        }

        #endregion

        #region "Snap setting"
        private void checkBox_snaptopt_CheckedChanged(object sender, EventArgs e)
        {
            // Snap to point
            gvariables.Is_xysnap = checkBox_snaptopt.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void checkBox_snaptohv_CheckedChanged(object sender, EventArgs e)
        {
            // Snap to horizontal/ Vertical
            gvariables.Is_hvsnap = checkBox_snaptohv.Checked;
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_ptsnapintensity_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Point snap intensity
            double rslt;
            double.TryParse(comboBox_ptsnapintensity.SelectedItem.ToString(), out rslt);
            gvariables.xysnap_intensity = rslt;
            the_parent.mt_pic.Refresh();
        }

        private void comboBox_hvsnapintensity_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Horizontal/ Vertical snap intensity
            double rslt;
            double.TryParse(comboBox_hvsnapintensity.SelectedItem.ToString(), out rslt);
            gvariables.hvsnap_intensity = rslt;
            the_parent.mt_pic.Refresh();
        }
        #endregion

    }
}
