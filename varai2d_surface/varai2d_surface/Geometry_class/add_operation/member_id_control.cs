using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace varai2d_surface.Geometry_class.add_operation
{
    [Serializable]
    public class member_id_control
    {
        private  HashSet<int> _point_id = new HashSet<int>();
        private HashSet<int> _line_id = new HashSet<int>();
        private HashSet<int> _arc_id = new HashSet<int>();
        private HashSet<int> _bezier_id = new HashSet<int>();
        private HashSet<int> _member_id = new HashSet<int>();


        public HashSet<int> point_id {get { return this._point_id; } }
        public HashSet<int> line_id { get { return this._line_id; } }
        public HashSet<int> arc_id { get { return this._arc_id; } }
        public HashSet<int> bezier_id { get { return this._bezier_id; } }

        public HashSet<int> member_id0 { get { return this._member_id; } }

        public member_id_control()
        {
        // Empty constructor
        }

        public member_id_control(HashSet<int> t_point_id,
            HashSet<int> t_line_id,
            HashSet<int> t_arc_id,
            HashSet<int> t_bezier_id,
            HashSet<int> t_member_id)
        {
            // Constructor to renew the entire data
            // Point
            this._point_id = new HashSet<int>();
            this._point_id.UnionWith(t_point_id);
            // Line
            this._line_id = new HashSet<int>();
            this._line_id.UnionWith(t_line_id);
            // Arc
            this._arc_id = new HashSet<int>();
            this._arc_id.UnionWith(t_arc_id);
            // Bezier
            this._bezier_id = new HashSet<int>();
            this._bezier_id.UnionWith(t_bezier_id);
            // Member
            this._member_id = new HashSet<int>();
            this._member_id.UnionWith(t_member_id);
        }

        public void add_point_id(int pt_id)
        {
            point_id.Add(pt_id);
        }

        public void delete_point_id(int pt_id)
        {
            point_id.Remove(pt_id);
        }

        public void add_member_id(int mem_id, int type)
        {
            //type: 1 Line, type: 2 Arc, type: 3 Bezier
            if (type == 1)
            {
                this._line_id.Add(mem_id);
            }
            else if (type == 2)
            {
                this._arc_id.Add(mem_id);
            }
            else if (type == 3)
            {
                this._bezier_id.Add(mem_id);
            }
            this._member_id.Add(mem_id);
        }

        public void delete_member_id(int mem_id, int type)
        {            
            //type: 1 Line, type: 2 Arc, type: 3 Bezier
            if (type == 1)
            {
                this._line_id.Remove(mem_id);
            }
            else if (type == 2)
            {
                this._arc_id.Remove(mem_id);
            }
            else if (type == 3)
            {
                this._bezier_id.Remove(mem_id);
            }
            this._member_id.Remove(mem_id);
        }

        public int get_member_id()
        {
            List<int> all_m_ids = new List<int>();
            all_m_ids.AddRange(this._member_id.ToList());

            return get_unique_id(all_m_ids);
        }

        public int get_point_id(List<int> temp_pt_ids)
        {
            List<int> all_pt_ids = new List<int>();
            all_pt_ids.AddRange(this._point_id);
            all_pt_ids.AddRange(temp_pt_ids);

            return get_unique_id(all_pt_ids);
        }

        private int get_unique_id(List<int> all_ids)
        {
            if (all_ids.Count != 0)
            {
                int i;
                all_ids.Sort();

                // Find if any of the nodes are missing in an ordered int
                for (i = 0; i < all_ids.Count; i++)
                {
                    if (all_ids[i] != i)
                    {
                        return i;
                    }
                }
                // no node id is missing in an ordered list so add to the end
                return all_ids.Count;
            }
            // id for the first node is 0
            return 0;
        }

    }
}
