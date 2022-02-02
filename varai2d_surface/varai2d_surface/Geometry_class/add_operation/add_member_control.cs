using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace varai2d_surface.Geometry_class.add_operation
{
    [Serializable]
    public class add_member_control
    {

        public void add_member(int c_state, workarea_control wkc)
        {
            // Save the state
            if (c_state > 0 && c_state < 6)
            {
                // Save the state before the modification
                wkc.save_state();
            }

            // Processing Add member
            switch (c_state)
            {
                case 1:
                    {
                        (new add_line_control()).add_line(wkc);
                        break;
                    }
                case 2:
                    {
                        // Not implemented
                        break;
                    }
                case 3:
                    {
                        // add_arc();
                        (new add_arc_control()).add_arc(wkc);
                        break;
                    }
                case 4:
                    {
                        // Not implemented
                        break;
                    }
                case 5:
                    {
                        //  add_bezier();
                        (new add_bezier_control()).add_bezier(wkc);
                        break;
                    }
            }
        }
    }
}
