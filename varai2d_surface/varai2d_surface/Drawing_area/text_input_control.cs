using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace varai2d_surface.Drawing_area
{
   public static class text_input_control
    {

        public static bool is_textvalue_length_angle_valid(string check_str, bool is_only_length, bool is_only_angle)
        {
            double rslt;
            bool is_numeric;

            if ( is_only_length == false && is_only_angle == false )
            {
                // Check whether a string is of format <positive double,double>
                // ####.###,-###.###
                // ##.##,##.###

                string[] str_split_comma = check_str.Split(',');

                // Check 1 String data must be two parts with comma in the middle
                if (str_split_comma.Length != 2)
                {
                    return false;
                }

                // Check the first part of the string
                is_numeric = double.TryParse(str_split_comma[0], out rslt);
                if (is_numeric == false)
                {
                    return false;
                }
                else
                {
                    if (rslt < 0)
                    {
                        // Negative values are not allowed
                        return false;
                    }
                }

                // Check the second part of the string
                is_numeric = double.TryParse(str_split_comma[1], out rslt);
                if (is_numeric == false)
                {
                    return false;
                }
            }
            else if (is_only_length == true)
            {
                // Check the Length part of the string
                is_numeric = double.TryParse(check_str, out rslt);
                if (is_numeric == false)
                {
                    return false;
                }
                else
                {
                    if (rslt < 0)
                    {
                        // Negative values are not allowed
                        return false;
                    }
                }

            }
            else if (is_only_angle == true)
            {
                // Check only angle is true (For mirror and rotate operation)
                is_numeric = double.TryParse(check_str, out rslt);
                if (is_numeric == false)
                {
                    return false;
                }
           }

            return true;
        }

        public static Tuple<double,double> get_textvalue_length_angle(string check_str,bool is_only_length, bool is_only_angle)
        {
            double length_rslt =0 , angle_rslt = 0;
            if (is_only_length == false && is_only_angle == false)
            {
                // Check the string for format <positive double,double> before using this function
                // ####.###,-###.###
                // ##.##,##.###

                string[] str_split_comma = check_str.Split(',');

                // Convert the first part of the string
               
                double.TryParse(str_split_comma[0], out length_rslt);

                // Convert the second part of the string
                double.TryParse(str_split_comma[1], out angle_rslt);

                
            }
            else if (is_only_length == true)
            {
                angle_rslt = 0; // Just a dummy value
                // Convert the angle part of the string
                double.TryParse(check_str, out length_rslt);
            }
            else if(is_only_angle == true)
            {
                length_rslt = 100; // Just a dummy value
                // Convert the angle part of the string
                double.TryParse(check_str, out angle_rslt);
            }

            return new Tuple<double, double>(length_rslt, angle_rslt);

        }

    }
}
