using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace varai2d_surface.global_static
{
  public static  class toolbarstate
    {
        // Add menu item - checkstate controller
        public static bool toolbar_select_Ischecked = false;
        public static bool toolbar_addline_Ischecked = false;
        public static bool toolbar_addcircle_Ischecked = false;
        public static bool toolbar_addpointarc_Ischecked = false;
        public static bool toolbar_addanglearc_Ischecked = false;
        public static bool toolbar_addbezier_Ischecked = false;

        // Modify menu item - checkstate controller
        public static bool toolbar_translate_Ischecked = false;
        public static bool toolbar_rotate_Ischecked = false;
        public static bool toolbar_mirror_Ischecked = false;

        public static bool toolbar_surface_creation_Ischecked = false;

        public static int checked_state_index = -1; // variable to store checked toolbar 0 - 8
        public static void update_toolbar_checkedstatus(string str_checked_state)
        {
            string[] str_cstate = str_checked_state.Split(',');

            toolbar_select_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[0]));
            toolbar_addline_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[1]));
            toolbar_addcircle_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[2]));
            toolbar_addpointarc_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[3]));
            toolbar_addanglearc_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[4]));
            toolbar_addbezier_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[5]));

            toolbar_translate_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[6]));
            toolbar_rotate_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[7]));
            toolbar_mirror_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[8]));

            toolbar_surface_creation_Ischecked = Convert.ToBoolean(Convert.ToInt32(str_cstate[9]));

            // Update checked state index
            if (toolbar_select_Ischecked == true)
            {
                // Select is checked
                if (toolbar_translate_Ischecked == true)
                {
                    // translate is checked
                    checked_state_index = 6;
                }
                else if (toolbar_rotate_Ischecked == true)
                {
                    // rotate is checked
                    checked_state_index = 7;
                }
                else if (toolbar_mirror_Ischecked == true)
                {
                    // Mirror is checked
                    checked_state_index = 8;
                }
                else
                {
                    // only selection is progress
                    checked_state_index = 0;
                }
            }
            else if (toolbar_addline_Ischecked == true)
            {
                // Addline is checked
                checked_state_index = 1;
            }
            else if (toolbar_addcircle_Ischecked == true)
            {
                // Add Circle is checked
                checked_state_index = 2;
            }
            else if (toolbar_addpointarc_Ischecked == true)
            {
                // Add point arc 1
                checked_state_index = 3;
            }
            else if (toolbar_addanglearc_Ischecked == true)
            {
                // Add angle arc 2
                checked_state_index = 4;
            }
            else if (toolbar_addbezier_Ischecked == true)
            {
                // Add bezier curve
                checked_state_index = 5;
            }
            else
            {
                // no selection
                checked_state_index = -1;
            }
        }

        public static int get_toolchecked_state
        {
            get
            {
                return checked_state_index;
            }
        }

        public static string get_status_tooltip(int checked_tool)
        {
            string tooltip = "";
            switch (checked_tool)
            {
                case -1:// new file
                    {
                        tooltip = "   File -> New (To start a new project)";
                        break;
                    }
                case 0: // Select
                    {
                        tooltip = "   Select member: Left click and drag to select member (Select members to Delete, translate, rotate, mirror)";
                        break;
                    }
                case 1: // Add Line
                    {
                        tooltip = "   Add Line: Left click to start add line + left click to complete the line";
                        break;
                    }
                case 2: // Add Circle
                    {
                        tooltip = "   Add Circle: Left click to fix center + left click to fix arc radius and complete the circle";
                        break;
                    }
                case 3: // Add Arc 1
                    {
                        tooltip = "   Add Arc 1: Left click + Left click to create chord + left click to fix arc radius and complete the arc";
                        break;
                    }
                case 4: // Add Arc 2
                    {
                        tooltip = "   Add Arc 2: Left click + Left click to create arc radius + left click to fix the sector angle and complet the arc";
                        break;
                    }
                case 5: // Add Bezier
                    {
                        tooltip = "   Add Bezier: Left clicks to add end and control point of bezier curve";
                        break;
                    }
                case 6: // Modify Translation
                    {
                        tooltip = "   Translate: Left click to start translation + left click to complete the translation";
                        break;
                    }
                case 7: // Modify Rotation
                    {
                        tooltip = "   Rotate: Left click to fix the rotation center + left click to fix the rotation angle";
                        break;
                    }
                case 8: // Modify Reflection
                    {
                        tooltip = "   Mirror: Left click to start the mirror line + left click to complete the mirror line";
                        break;
                    }
                case 9:
                    {
                        tooltip = "   Left Click on closed boundary to create surface (or) Right Click to delete";
                        break;
                    }

            }

            return tooltip;
        }
    }
}
