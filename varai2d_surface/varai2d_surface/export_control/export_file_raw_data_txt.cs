using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using varai2d_surface.Geometry_class.geometry_store;
using varai2d_surface.Geometry_class;
using varai2d_surface.global_static;

namespace varai2d_surface.export_control
{
    public class export_file_raw_data_txt
    {
        private workarea_control wkc_obj;
        private List<string> _output_str = new List<string>();

        public string[] output_str { get { return this._output_str.ToArray(); } }

        public export_file_raw_data_txt(workarea_control wkc)
        {
            this.wkc_obj = wkc;

            // Create the Raw data export
            string temp_str = "";
            double o_x, o_y;

            // Information lines
            temp_str = "######################################################";
            this._output_str.Add(temp_str);
            temp_str = "####### Samson Mano's Varai2D Raw Data ###############";
            this._output_str.Add(temp_str);
            temp_str = "######################################################";
            this._output_str.Add(temp_str);

            // Add End points
            temp_str = "[+] End Points, " + this.wkc_obj.geom_obj.all_end_pts.Count.ToString();
            this._output_str.Add(temp_str);

            foreach (points_store pts in this.wkc_obj.geom_obj.all_end_pts)
            {
                o_x = pts.x / gvariables.scale_factor;
                o_y = (-1.0d * pts.y) / gvariables.scale_factor;

                temp_str = pts.nd_id.ToString() + ", " + o_x.ToString() + ", " + o_y.ToString();
                this._output_str.Add(temp_str);
            }

            // Add Lines
            if (this.wkc_obj.geom_obj.all_lines.Count != 0)
            {
                temp_str = "[+] Lines, " + this.wkc_obj.geom_obj.all_lines.Count.ToString();
                this._output_str.Add(temp_str);

                foreach (lines_store ln in this.wkc_obj.geom_obj.all_lines)
                {
                    temp_str = ln.line_id.ToString() + ", " + ln.pt_start_id.ToString() + ", " + ln.pt_end_id.ToString();
                    this._output_str.Add(temp_str);
                }
            }

            // Add arcs
            if (this.wkc_obj.geom_obj.all_arcs.Count != 0)
            {
                temp_str = "[+] Arcs, " + this.wkc_obj.geom_obj.all_arcs.Count.ToString();
                this._output_str.Add(temp_str);

                foreach (arcs_store arcs in this.wkc_obj.geom_obj.all_arcs)
                {
                    temp_str = arcs.arc_id.ToString() + ", " + arcs.pt_chord_start_id.ToString() + ", " + arcs.pt_chord_end_id.ToString();
                    this._output_str.Add(temp_str);

                    // Add the center point and crown point to the raw data
                    o_x = arcs.arc_center_pt.X / gvariables.scale_factor;
                    o_y = (-1.0d * arcs.arc_center_pt.Y) / gvariables.scale_factor;
                    temp_str = "c0, " + o_x.ToString() + ", " + o_y.ToString();
                    this._output_str.Add(temp_str);

                    o_x = arcs.arc_crown_pt.X / gvariables.scale_factor;
                    o_y = (-1.0d * arcs.arc_crown_pt.Y) / gvariables.scale_factor;
                    temp_str = "c1, " + arcs.arc_crown_pt.X.ToString() + ", " + arcs.arc_crown_pt.Y.ToString();
                    this._output_str.Add(temp_str);
                }
            }

            // Add bezier
            if (this.wkc_obj.geom_obj.all_beziers.Count != 0)
            {
                temp_str = "[+] Beziers, " + this.wkc_obj.geom_obj.all_beziers.Count.ToString();
                this._output_str.Add(temp_str);

                foreach (beziers_store bz in this.wkc_obj.geom_obj.all_beziers)
                {
                    temp_str = bz.bezier_id.ToString() + ", " + bz.pt_bz_start_id.ToString() + ", " + bz.pt_bz_end_id.ToString();
                    // Add the control point count
                    temp_str = temp_str + ", c@" + (bz.bezier_cntrl_pts.Count - 2).ToString();

                    this._output_str.Add(temp_str);

                    // Add the internal control points to the raw data
                    int j = 0;
                    for (int i = 1; i < (bz.bezier_cntrl_pts.Count - 1); i++)
                    {
                        o_x = bz.bezier_cntrl_pts[i].X / gvariables.scale_factor;
                        o_y = (-1.0d * bz.bezier_cntrl_pts[i].Y) / gvariables.scale_factor;
                        temp_str = "c" + j.ToString() + ", " + o_x.ToString() + ", " + o_y.ToString();
                        this._output_str.Add(temp_str);
                        j++;
                    }
                }
            }

            // Add surface
            if (this.wkc_obj.geom_obj.all_surfaces.Count != 0)
            {
                temp_str = "[+] Surfaces, " + this.wkc_obj.geom_obj.all_surfaces.Count.ToString();
                this._output_str.Add(temp_str);

                foreach (surface_store surf in this.wkc_obj.geom_obj.all_surfaces)
                {
                    // Closed loop boundary id
                    temp_str = surf.surf_id.ToString() + ", {";
                    foreach (int m_id in surf.closed_loop_bndry_id)
                    {
                        temp_str = temp_str + m_id.ToString() + ", ";
                    }
                    temp_str = temp_str.Remove(temp_str.Length - 2, 2);
                    temp_str = temp_str + "}";
                    // Add the nested surface count
                    temp_str = temp_str + ", n@" + surf.nested_surface_count;

                    this._output_str.Add(temp_str);

                    // Add the nested surface's closed loop member id
                    string outpt = surf.get_nested_surface_closed_boundary_member_ids();
                    string[] split_nested_surface_output = new string[0];
                    if (outpt != null)
                    {
                       split_nested_surface_output = surf.get_nested_surface_closed_boundary_member_ids().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    }
            
                    for (int j = 0; j < split_nested_surface_output.Length; j++)
                    {
                        temp_str = "n" + j.ToString() + split_nested_surface_output[j];
                        this._output_str.Add(temp_str);
                    }
                }
            }

            // End the lines
            temp_str = "######################################################";
            this._output_str.Add(temp_str);
            temp_str = "######################################################";
            this._output_str.Add(temp_str);
        }

    }
}
