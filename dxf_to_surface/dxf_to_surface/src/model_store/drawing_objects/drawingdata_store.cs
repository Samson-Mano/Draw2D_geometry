using dxf_to_surface.src.events_handler;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.model_store.drawing_objects
{
    public class drawingdata_store
    {

        public linelist_store line_data; // { get; }


        public drawingdata_store()
        {
            line_data = new linelist_store();
        }

        public void paint_drawing_data()
        {
            line_data.paint_lines();
        }


        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            // Update the drawing object uniforms
            line_data.update_openTK_uniforms(graphic_events_control);

        }




    }
}
