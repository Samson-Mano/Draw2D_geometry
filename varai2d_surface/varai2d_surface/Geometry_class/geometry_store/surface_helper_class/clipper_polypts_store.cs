using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using varai2d_surface.Drawing_area;
using varai2d_surface.global_static;

namespace varai2d_surface.Geometry_class.geometry_store.surface_helper_class
{
  public  class clipper_polypts_store
    {
        // Saved as int just in case if there is an error in precision messing up the comparison
        // signed integer size -2,147,483,648 to 2,147,483,647
        int _pt_id;
        int _x_int;
        int _y_int;
        double _x;
        double _y;

        public int p_id { get { return this._pt_id; } }

        public double x { get { return this._x; } }

        public double y { get { return this._y; } }

        public int x_int { get { return this._x_int; } }

        public int y_int { get { return this._y_int; } }

        public PointF get_pt { get { return new PointF((float)this._x, (float)this._y); } }

        public clipper_polypts_store(int id, double tx, double ty)
        {
            this._pt_id = id;
            // Main data
            this._x = tx;
            this._y = ty;

            // Store as integer for quick check
            this._x_int = (int)(Math.Round(tx, 6) * 100000);
            this._y_int = (int)(Math.Round(ty, 6) * 100000);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as clipper_polypts_store);
        }

        public bool Equals(clipper_polypts_store other_pt)
        {
            if ((this._x_int == other_pt._x_int) && (this._y_int == other_pt._y_int))
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this._x_int, this._y_int);
        }

    }
}
