using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace varai2d_surface.Geometry_class.modify_operation
{
    [Serializable]
    public class modify_member_control
    {
        public void modify_member(int c_state,workarea_control wkc)
        {
            // Save the state
            if (c_state > 5 && c_state < 12)
            {
                // Save the state before the modification
                wkc.save_state();
            }

            // Processing Modify member
            switch (c_state)
            {
                case 6:
                    {
                        // Translate modification
                        (new modify_translation_control()).modify_translation(wkc);
                        break;
                    }
                case 7:
                    {
                        // Rotate modification
                        (new modify_rotation_control()).modify_rotation(wkc);
                        break;
                    }
                case 8:
                    {
                        // Mirror modification
                        (new modify_mirror_control()).modify_mirror(wkc);
                        break;
                    }
                case 9:
                    {
                        // Delete operation
                        (new delete_operation_control()).delete_member(wkc);
                        break;
                    }
                case 10:
                    {
                        // Intersect operation
                        (new modify_intersect_control()).intersect_member(wkc);
                        break;
                    }
                case 11:
                    {
                        // Split operation
                        (new modify_split_control()).split_member(wkc);
                        break;
                    }
            }
        }


    }
}
