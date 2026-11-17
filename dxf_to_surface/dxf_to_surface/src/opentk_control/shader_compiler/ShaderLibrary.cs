using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxf_to_surface.src.opentk_control.shader_compiler
{
    public class ShaderLibrary
    {

        public enum ShaderType
        {
            LineShader,
            MeshShader,
            TextShader,

            SelectionShader,
            DrawingAxisShader,
        }


        #region "Line Shaders"

        private static string line_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform vec4 lineVertexColor;
                    
            layout(location = 0) in vec2 aPosition;
                    

            out vec4 vColor;
                    
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                vColor = lineVertexColor;
            }


                    ";

        }




        private static string line_frag_shader()
        {

            return @"

            #version 330 core

            in vec4 vColor;
            out vec4 fColor;
    
            void main()
            {
                // Simple color output without lighting
                fColor = vColor;
            }


                    ";

        }

        #endregion


        #region "Mesh Shaders"

        private static string mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform vec4 vertexColor;
                    
            layout(location = 0) in vec2 aPosition;
                    

            out vec4 vColor;
                    
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                vColor = vertexColor;
            }


                    ";

        }




        private static string mesh_frag_shader()
        {

            return @"

            #version 330 core

            in vec4 vColor;
            out vec4 fColor;
    
            void main()
            {
                // Simple color output without lighting
                fColor = vColor;
            }


                    ";

        }

        #endregion


        #region "Text shaders"

        public static string text_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform float zoomscale = 1.0f;

            uniform float vertexTransparency = 1.0f; // Transparency of the mesh

            layout(location = 0) in vec2 position;
            layout(location = 1) in vec2 origin;
            layout(location = 2) in vec2 textureCoord;
            layout(location = 3) in vec3 textColor;

            out vec4 v_textureColor;
            out vec2 v_textureCoord;

            void main()
            {

	            // apply Translation to the final position 
	            vec4 finalPosition =  uMVP * vec4(position,0.0f,1.0f);

	            // apply Translation to the text origin
	            vec4 finalTextorigin =  uMVP * vec4(origin,0.0f,1.0f);
    

	            // Remove the zoom scale
	            vec2 scaled_pt = vec2(finalPosition.x - finalTextorigin.x,finalPosition.y - finalTextorigin.y) / zoomscale;
		
	            // Set the final position of the vertex
	            gl_Position = vec4(scaled_pt.x + finalTextorigin.x, scaled_pt.y + finalTextorigin.y, 0.0f, 1.0f);


	            // Calculate texture coordinates for the glyph
	            v_textureCoord = textureCoord;
	
	            // Pass the texture color to the fragment shader
	            v_textureColor = vec4(textColor, vertexTransparency);
            }

                    ";

        }


        public static string text_frag_shader()
        {
            return @"

            #version 330 core
            uniform sampler2D u_Texture;

            in vec4 v_textureColor;
            in vec2 v_textureCoord;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
	            vec4 texColor = vec4(1.0, 1.0, 1.0, texture(u_Texture, v_textureCoord).r);
	            f_Color = v_textureColor * texColor;
            }

                    ";

        }

        #endregion


        #region "Selection Shader"

        private static string selrect_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;

            out vec4 v_Color;

            void main()
            {
	            v_Color = vec4(0.8039f,0.3608f,0.3608f,0.5f);

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string selrect_frag_shader()
        {
            return @"

            #version 330 core

            in vec4 v_Color;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
	            f_Color = v_Color;
            }

                    ";

        }


        #endregion


        #region "Drawing Axis Shader"

        private static string drawingaxis_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in vec3 node_color;

            out vec4 v_Color;

            void main()
            {
	            v_Color = vec4(node_color, 1.0f);

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string drawingaxis_frag_shader()
        {
            return @"

            #version 330 core

            in vec4 v_Color;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
	            f_Color = v_Color;
            }

                    ";

        }


        #endregion



        public static string get_vertex_shader(ShaderType type)
        {
            // Returns the vertex shader
            switch (type)
            {
                case ShaderType.LineShader:
                    return line_vert_shader();
                case ShaderType.MeshShader:
                    return mesh_vert_shader();
                case ShaderType.SelectionShader:
                    return selrect_vert_shader();
                case ShaderType.TextShader:
                    return text_vert_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_vert_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        public static string get_fragment_shader(ShaderType type)
        {
            // Returns the fragment shader
            switch (type)
            {
                case ShaderType.LineShader:
                    return line_frag_shader();
                case ShaderType.MeshShader:
                    return mesh_frag_shader();
                case ShaderType.SelectionShader:
                    return selrect_frag_shader();
                case ShaderType.TextShader:
                    return text_frag_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_frag_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        //___________________

    }
}
