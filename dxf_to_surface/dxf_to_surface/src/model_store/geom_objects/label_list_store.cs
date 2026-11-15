// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace dxf_to_surface.src.model_store.geom_objects
{
    public class label_store
    {
        // Stores the individual label
        public int label_id { get; set; }
        public string label { get; set; }
        public Vector2 label_loc { get; set; }
        public Vector3 label_color { get; set; }
        public double label_angle { get; set; }
        public bool label_above_loc { get; set; }
        public int label_char_count { get; set; }

    }


    public class label_list_store
    {
        public Dictionary<int, label_store> labelMap { get; } = new Dictionary<int, label_store>();
        private int label_count = 0;
        private int total_char_count = 0;

        private float font_size = 14.0f;
        private float geom_size = 1.0f;


        private VertexArray _labelVAO;
        private VertexBuffer _labelVBO;
        private IndexBuffer _labelIBO;

        public Shader _labelshader;


        public label_list_store()
        {
            // (Re)Initialize the data
            labelMap = new Dictionary<int, label_store>();
            label_count = 0;
            total_char_count = 0;


            InitializeShader();
            InitializeBuffers();

        }


        public void add_label(int label_id, string label, Vector2 label_loc, Vector3 label_color)
        {
            // Add the Label to the list
            label_store temp_label = new label_store
            {
                label_id = label_id,
                label = label,
                label_loc = label_loc,
                label_color = label_color,
                label_angle = 0.0, // radian
                label_above_loc = true,
                label_char_count = label.Length
            };

            labelMap[label_id] = temp_label;
            label_count++;

            // Add to the total character count
            total_char_count = total_char_count + label.Length;

            // update_buffer();
        }



        public void delete_label(int label_id)
        {
            // Adjust the total character count
            int label_char_count = labelMap[label_id].label_char_count;
            total_char_count = total_char_count - label_char_count;

            // Delete the label
            labelMap.Remove(label_id);
            label_count--;

            // update_buffer();

        }


        private void InitializeShader()
        {

            // Create Shader
            _labelshader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.TextShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.TextShader));

        }


        private void InitializeBuffers()
        {
            // Label buffer
            _labelVAO = new VertexArray();
            _labelVBO = new VertexBuffer(10);
            _labelIBO = new IndexBuffer(10);

            var labelLayout = new VertexBufferLayout();
            labelLayout.AddFloat(2);  // Character Position
            labelLayout.AddFloat(2);  // Text location
            labelLayout.AddFloat(2);  // Texture coordinate
            labelLayout.AddFloat(3);  // Text color


            _labelVAO.Add_vertexBuffer(_labelVBO, labelLayout);

        }


        public void update_buffer(float geom_size)
        {
            this.geom_size = geom_size;


            // Set the buffer for index (6 indices to form a two triangle, quadrilateral )
            List<int> label_indices = new List<int>();

            // Set the label index buffers
            int t_id = 0;
            foreach (var lb in labelMap)
            {
                get_label_index_buffer(lb.Value, ref label_indices, ref t_id);
            }

            // Update the index buffer
            _labelIBO.ClearIndexBuffer();
            _labelIBO.AppendIndexBuffer(label_indices.ToArray());


            // Define the vertex buffer size for a character
           List<float> label_vertices = new List<float>();

            // Set the label vertex buffers
            foreach (var lb in labelMap)
            {
                // Add vertex buffers
                get_label_vertex_buffer(lb.Value, ref label_vertices);
            }


            // Update the vertex buffer
            _labelVBO.ClearVertexBuffer();
            _labelVBO.AppendVertexBuffer(label_vertices.ToArray());
            
        }



        public void clear_labels()
        {
            // Clear the data
            labelMap.Clear();
            label_count = 0;
            total_char_count = 0;

            // Clear the buffers
            _labelIBO.ClearIndexBuffer();
            _labelVBO.ClearVertexBuffer();

        }


        public void update_openTK_uniforms(Matrix4 uMVP, float zoomscale, float transparency)
        {
            // Update the openGl uniform matrices
            _labelshader.SetMatrix4("uMVP", uMVP);

            _labelshader.SetFloat("zoomscale", zoomscale);

            // Set the transparency float
            _labelshader.SetFloat("vertexTransparency", transparency);

        }


        public void paint_static_labels()
        {
            // Paint all the static points
            _labelshader.Bind();
            _labelVAO.Bind();
            _labelIBO.Bind();

            // Activate texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture to the active texture unit
            GL.BindTexture(TextureTarget.Texture2D, gvariables_static.main_font.TextureID);

            // Draw the elements
            GL.DrawElements(PrimitiveType.Triangles, _labelIBO.BufferCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);


            _labelshader.UnBind();
            _labelVAO.UnBind();
            _labelIBO.UnBind();

        }


        private void get_label_vertex_buffer(label_store lb, ref List<float> label_vertices)
        {

            float font_scale = this.geom_size * 0.01f * (this.font_size / 12.0f) * 0.025f; 

            // Find the label total width and total height of the label
            float total_label_width = 0.0f;
            float total_label_height = 0.0f;

            // lb.label[i] != '\0'

            for (int i = 0; i < lb.label_char_count; ++i)
            {
                // get the atlas information
                char ch = lb.label[i];
                Character ch_data = gvariables_static.main_font.Glyphs[ch];

                total_label_width += ch_data.Advance * font_scale;
                total_label_height = Math.Max(total_label_height, ch_data.Size.Y * font_scale);
            }


            // Get the x,y location
            Vector2 loc = lb.label_loc;
            float x = loc.X - (total_label_width * 0.5f);

            // Whether paint above the location or not
            float y = 0.0f;
            if (lb.label_above_loc == true)
            {
                y = loc.Y + (total_label_height * 0.5f);
            }
            else
            {
                y = loc.Y - (total_label_height + (total_label_height * 0.5f));
            }


            Vector2 rotated_pt = new Vector2(0, 0);

            for (int i = 0; i < lb.label_char_count; ++i)
            {
                // get the atlas information
                char ch = lb.label[i];

                Character ch_data = gvariables_static.main_font.Glyphs[ch];

                float xpos = x + (ch_data.Bearing.X * font_scale);
                float ypos = y - (ch_data.Size.Y - ch_data.Bearing.Y) * font_scale;

                float w = ch_data.Size.X * font_scale;
                float h = ch_data.Size.Y * font_scale;

                float margin = 0.00022f; // This value prevents the minor overlap with the next char when rendering

                // Point 1
                // Vertices [0,0] // 0th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos, ypos + h), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.TopLeft.X + margin);
                label_vertices.Add(ch_data.TopLeft.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________

                // Point 2
                // Vertices [0,1] // 1th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos, ypos), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.TopLeft.X + margin);
                label_vertices.Add(ch_data.BottomRight.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________

                // Point 3
                // Vertices [1,1] // 2th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos + w, ypos), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.BottomRight.X - margin);
                label_vertices.Add(ch_data.BottomRight.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________

                // Point 4
                // Vertices [1,0] // 3th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos + w, ypos + h), lb.label_angle);

                // Character location
                label_vertices.Add(rotated_pt.X);
                label_vertices.Add(rotated_pt.Y);

                // character origin
                label_vertices.Add(loc.X);
                label_vertices.Add(loc.Y);

                // Texture Glyph coordinate
                label_vertices.Add(ch_data.BottomRight.X - margin);
                label_vertices.Add(ch_data.TopLeft.Y);

                // Text color
                label_vertices.Add(lb.label_color.X);
                label_vertices.Add(lb.label_color.Y);
                label_vertices.Add(lb.label_color.Z);

                //__________________________________________________________________________________________
                x += ch_data.Advance * font_scale;

            }

        }


        private void get_label_index_buffer(label_store lb, ref List<int> label_indices, ref int t_id)
        {

            for (int i = 0; i < lb.label_char_count; ++i)
            {
                // Set the index buffers
                // Triangle 0,1,2
                label_indices.Add(t_id + 0);
                label_indices.Add(t_id + 1);
                label_indices.Add(t_id + 2);

                // Triangle 2,3,0
                label_indices.Add(t_id + 2);
                label_indices.Add(t_id + 3);
                label_indices.Add(t_id + 0);

                // Increment
                t_id = t_id + 4;
            }

        }

    }


}
