using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class arc_store
    {
        public int arc_id;
        public Vector2 arcStartPt;
        public Vector2 arcEndPt;
        public Vector2 arcCenterPt;
        public double arcRadius;
        public double arcStartAngle;
        public double arcEndAngle;
    }



    public class arclist_store
    {


    }
}
