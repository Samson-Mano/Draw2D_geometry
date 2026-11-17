using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class circle_store
    {
        public int circle_id;
        public Vector2 circleCenterPt;
        public double circleRadius;
    }


    public class circlelist_store
    {

        public Dictionary<int, circle_store> circleMap { get; } = new Dictionary<int, circle_store>();
        private int circleCount = 0;

   

        public circlelist_store()
        {
            circleMap = new Dictionary<int, circle_store>();
            circleCount = 0;


        }
        
        public void add_circle(int circleID, Vector2 centerPt, double radius)
        {
            var circle = new circle_store
            {
                circle_id = circleID,
                circleCenterPt = centerPt,
                circleRadius = radius
            };
            circles.Add(circle);
        }

        public void clear_circles()
        {
            circles.Clear();
        }
    }
}
