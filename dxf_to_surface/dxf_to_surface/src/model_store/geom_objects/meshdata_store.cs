// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using dxf_to_surface.src.events_handler;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store.geom_objects;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;



namespace dxf_to_surface.src.model_store.geom_objects
{

    public class meshdata_store : IDisposable
    {

        private struct point_store
        {
            public int point_id;
            public float x_coord;
            public float y_coord;
        }

        private struct line_store
        {
            public int line_id;
            public int line_start_id;
            public int line_end_id;
        }

        private struct tri_store
        {
            public int tri_id;
            public int pt_id1;
            public int pt_id2;
            public int pt_id3;
            public int mat_id;
        }

        private struct quad_store
        {
            public int quad_id;
            public int pt_id1;
            public int pt_id2;
            public int pt_id3;
            public int pt_id4;
            public int mat_id;
        }

        private class mat_mesh_store
        {
            public int material_id;

            // Index buffer for  triangles and quadrilaterals (EBO)
            public IndexBuffer triangle_ibo;
            public IndexBuffer quadrilateral_ibo;

            public HashSet<int> tri_ids = new HashSet<int>();
            public HashSet<int> quad_ids = new HashSet<int>();

        }

        private const int FLOATS_PER_VERTEX = 2;

        private List<point_store> points = new List<point_store>();
        private List<line_store> wireframe_lines = new List<line_store>();
        private Dictionary<int, tri_store> tris = new Dictionary<int, tri_store>();
        private Dictionary<int, quad_store> quads = new Dictionary<int, quad_store>();

        public int get_wireframe_line_count { get { return wireframe_lines.Count; } }

        private Dictionary<int, mat_mesh_store> mat_mesh_data = new Dictionary<int, mat_mesh_store>();

        // Geometry data for OpenGL
        private Dictionary<int, int> pointIDToIndex = new Dictionary<int, int>();

        private Shader meshShader;

        // Vertex Buffer object and Vertex Array object 
        private VertexBuffer point_vbo;
        private VertexArray point_vao;

        // Index buffer for the points and lines (EBO)
        private IndexBuffer point_ibo;
        private IndexBuffer selected_point_ibo;
        private IndexBuffer wireframe_ibo;

        // Shrunk mesh data
        private shrunkmeshdata_store shrunk_mesh_data = new shrunkmeshdata_store();

        private bool buffersInitialized = false;

        public meshdata_store()
        {
            InitializeShader();
        }


        public void add_point(int point_id, float x, float y)
        {
            points.Add(new point_store()
            {
                point_id = point_id,
                x_coord = x,
                y_coord = y
            });
        }

        public void add_tri(int tri_id, int pt_id1, int pt_id2, int pt_id3, int mat_id)
        {
            if (!mat_mesh_data.TryGetValue(mat_id, out var matMesh))
            {
                matMesh = new mat_mesh_store() { material_id = mat_id };
                mat_mesh_data[mat_id] = matMesh;
            }

            tris.Add(tri_id, new tri_store()
            {
                tri_id = tri_id,
                pt_id1 = pt_id1,
                pt_id2 = pt_id2,
                pt_id3 = pt_id3,
                mat_id = mat_id
            });

            matMesh.tri_ids.Add(tri_id);

        }

        public void add_quad(int quad_id, int pt_id1, int pt_id2, int pt_id3, int pt_id4, int mat_id)
        {
            if (!mat_mesh_data.TryGetValue(mat_id, out var matMesh))
            {
                matMesh = new mat_mesh_store() { material_id = mat_id };
                mat_mesh_data[mat_id] = matMesh;
            }

            quads.Add(quad_id, new quad_store()
            {
                quad_id = quad_id,
                pt_id1 = pt_id1,
                pt_id2 = pt_id2,
                pt_id3 = pt_id3,
                pt_id4 = pt_id4,
                mat_id = mat_id
            });

            matMesh.quad_ids.Add(quad_id);

        }


        public void update_material(HashSet<int> selected_tri_ids, HashSet<int> selected_quad_ids, int mat_id)
        {
            if (!mat_mesh_data.TryGetValue(mat_id, out var matMesh))
            {
                matMesh = new mat_mesh_store() { material_id = mat_id };
                mat_mesh_data[mat_id] = matMesh;
            }

            // Update the material 
            matMesh.tri_ids.UnionWith(selected_tri_ids);
            matMesh.quad_ids.UnionWith(selected_quad_ids);

            foreach (mat_mesh_store matMesh_ot in mat_mesh_data.Values)
            {
                // Reset other mesh
                if (matMesh_ot.material_id == mat_id)
                    continue;


                // Remove the tri ids & quad ids
                matMesh_ot.tri_ids.ExceptWith(selected_tri_ids);
                matMesh_ot.quad_ids.ExceptWith(selected_quad_ids);

            }

            reset_matmesh_buffer();

            // Update for shrunk mesh
            shrunk_mesh_data.shrunk_update_material(selected_tri_ids, selected_quad_ids, mat_id);

        }


        public void deletematerial(int mat_id)
        {
            if (!mat_mesh_data.ContainsKey(mat_id))
                return;


            List<int> materialchanged_element_tris = mat_mesh_data[mat_id].tri_ids.ToList();
            List<int> materialchanged_element_quads = mat_mesh_data[mat_id].quad_ids.ToList();

            // Delete the mat mesh associated with this material id
            mat_mesh_data.Remove(mat_id);

            // Assign default material to the element tris and quads
            mat_mesh_data[0].tri_ids.UnionWith(materialchanged_element_tris);
            mat_mesh_data[0].quad_ids.UnionWith(materialchanged_element_quads);

            reset_matmesh_buffer();

            // Update for shrunk mesh
            shrunk_mesh_data.shrunk_deletematerial(mat_id);

        }


        private void reset_matmesh_buffer()
        {
            
            //_______________________________________________________________
            // Reset triangle and quadrilateral index data after material update for openGL

            foreach (mat_mesh_store matMesh in mat_mesh_data.Values)
            {
                List<int> triangleIndexData = new List<int>();

                foreach (int tri_id in matMesh.tri_ids)
                {
                    tri_store tri = tris[tri_id];

                    int pt_idx1 = pointIDToIndex[tri.pt_id1];
                    int pt_idx2 = pointIDToIndex[tri.pt_id2];
                    int pt_idx3 = pointIDToIndex[tri.pt_id3];


                    triangleIndexData.Add(pt_idx1);
                    triangleIndexData.Add(pt_idx2);
                    triangleIndexData.Add(pt_idx3);

                }

                if (matMesh.triangle_ibo == null)
                    matMesh.triangle_ibo = new IndexBuffer(10);

                matMesh.triangle_ibo.ClearIndexBuffer();
                if (triangleIndexData.Count > 0)
                {
                    matMesh.triangle_ibo.AppendIndexBuffer(triangleIndexData.ToArray());
                }

                List<int> quadrilateralIndexData = new List<int>();

                foreach (int quad_id in matMesh.quad_ids)
                {
                    quad_store quad = quads[quad_id];

                    int pt_idx1 = pointIDToIndex[quad.pt_id1];
                    int pt_idx2 = pointIDToIndex[quad.pt_id2];
                    int pt_idx3 = pointIDToIndex[quad.pt_id3];
                    int pt_idx4 = pointIDToIndex[quad.pt_id4];


                    // Make two triangles from the quad (pt1, pt2, pt3) and (pt1, pt3, pt4)
                    // Triangle 1 (nd1, nd2, nd3)
                    quadrilateralIndexData.Add(pt_idx1);
                    quadrilateralIndexData.Add(pt_idx2);
                    quadrilateralIndexData.Add(pt_idx3);

                    // Triangle 2 (nd1, nd3, nd4)
                    quadrilateralIndexData.Add(pt_idx1);
                    quadrilateralIndexData.Add(pt_idx3);
                    quadrilateralIndexData.Add(pt_idx4);

                }

                if (matMesh.quadrilateral_ibo == null)
                    matMesh.quadrilateral_ibo = new IndexBuffer(10);

                matMesh.quadrilateral_ibo.ClearIndexBuffer();
                if (quadrilateralIndexData.Count > 0)
                {
                    matMesh.quadrilateral_ibo.AppendIndexBuffer(quadrilateralIndexData.ToArray());
                }

            }

        }



        private void InitializeShader()
        {
            // Create Shader
            meshShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader)
                );

        }


        public void paint_mesh()
        {
            if (!gvariables_static.is_paint_mesh || !buffersInitialized)
                return;


            meshShader.Bind();

            if (gvariables_static.is_paint_shrunk_triangle)
            {
                // Paint the shrunk mesh 
                shrunk_mesh_data.paint_shrunk_mesh(ref meshShader);
                meshShader.UnBind();
                return;
            }

            point_vao.Bind();

            foreach (mat_mesh_store mat_mesh_vals in mat_mesh_data.Values)
            {

                // Get the Color for the mesh (based on material ID)
                Vector4 MeshColor = new Vector4(gvariables_static.ColorUtils.MeshGetRandomColor(mat_mesh_vals.material_id),
                    gvariables_static.geom_transparency);


                meshShader.SetVector4("vertexColor", MeshColor);


                if (mat_mesh_vals.triangle_ibo.BufferCount > 0)
                {
                    // Paint the triangle mesh
                    mat_mesh_vals.triangle_ibo.Bind();
                    GL.DrawElements(PrimitiveType.Triangles, mat_mesh_vals.triangle_ibo.BufferCount,
                        DrawElementsType.UnsignedInt, 0);
                    mat_mesh_vals.triangle_ibo.UnBind();
                }

                if (mat_mesh_vals.quadrilateral_ibo.BufferCount > 0)
                {
                    // Paint the quadrilateral mesh
                    mat_mesh_vals.quadrilateral_ibo.Bind();
                    GL.DrawElements(PrimitiveType.Triangles, mat_mesh_vals.quadrilateral_ibo.BufferCount,
                        DrawElementsType.UnsignedInt, 0);
                    mat_mesh_vals.quadrilateral_ibo.UnBind();
                }

            }


            meshShader.UnBind();
            point_vao.UnBind();

        }


        public void paint_mesh_wireframe()
        {
            if (!gvariables_static.is_paint_mesh_boundaries || !buffersInitialized)
                return;


            if (wireframe_ibo.BufferCount > 0)
            {
                // Paint the wire frame
                Vector4 WireframeColor = new Vector4(gvariables_static.ColorUtils.get_WireframeColor(),
                    gvariables_static.geom_transparency);

                meshShader.Bind();
                meshShader.SetVector4("vertexColor", WireframeColor);

                point_vao.Bind();
                wireframe_ibo.Bind();

                GL.DrawElements(PrimitiveType.Lines, wireframe_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);

                meshShader.UnBind();
                point_vao.UnBind();
                wireframe_ibo.UnBind();

            }

        }


        public void paint_mesh_points()
        {
            if (!gvariables_static.is_paint_meshpoints || !buffersInitialized)
                return;


            if (point_ibo.BufferCount > 0)
            {
                // Paint the mesh points
                Vector4 PtColor = new Vector4(gvariables_static.ColorUtils.get_PtColor(),
                    gvariables_static.geom_transparency);

                meshShader.Bind();
                meshShader.SetVector4("vertexColor", PtColor);

                point_vao.Bind();
                point_ibo.Bind();

                GL.PointSize(2.0f);
                GL.DrawElements(PrimitiveType.Points, point_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);
                GL.PointSize(1.0f);

                meshShader.UnBind();
                point_vao.UnBind();
                point_ibo.UnBind();

            }

        }


        public void paint_selected_mesh_points()
        {
            if (selected_point_ibo.BufferCount > 0)
            {
                // Paint the selected points
                Vector4 selectedPtColor = new Vector4(gvariables_static.ColorUtils.get_SelectionPtColor(),
                    gvariables_static.geom_transparency);

                meshShader.Bind();
                meshShader.SetVector4("vertexColor", selectedPtColor);

                point_vao.Bind();
                selected_point_ibo.Bind();

                GL.PointSize(6.0f);
                GL.DrawElements(PrimitiveType.Points, selected_point_ibo.BufferCount, DrawElementsType.UnsignedInt, 0);
                GL.PointSize(1.0f);

                meshShader.UnBind();
                point_vao.UnBind();
                selected_point_ibo.UnBind();

            }

        }


        public void paint_selected_mesh()
        {
            if (!buffersInitialized)
                return;


            meshShader.Bind();

            // Paint the selected mesh 
            shrunk_mesh_data.paint_selected_mesh(ref meshShader);
            meshShader.UnBind();

        }


        public void add_selected_points(List<int> selected_point_ids)
        {
            List<int> selectedPointIndexData = new List<int>();

            foreach (int pointId in selected_point_ids)
            {
                if (pointIDToIndex.TryGetValue(pointId, out int index))
                {
                    selectedPointIndexData.Add(index);
                }
            }

            // Update the selected point ibo
            selected_point_ibo.ClearIndexBuffer();
            selected_point_ibo.AppendIndexBuffer(selectedPointIndexData.ToArray());

        }


        public void clear_selected_points()
        {
            // Clear the selected points
            selected_point_ibo.ClearIndexBuffer();
        }


        public void add_selected_mesh(List<int> selected_tri_ids, List<int> selected_quad_ids)
        {
            // Add the selected mesh
            shrunk_mesh_data.add_selected_mesh(selected_tri_ids, selected_quad_ids);

        }


        public void clear_selected_mesh()
        {
            // Clear the selected mesh
            shrunk_mesh_data.clear_selected_mesh();
        }


        public void create_wireframe()
        {
            // Create the wireframe from the mesh data
            wireframe_lines = new List<line_store>();

            // HashSet to track unique edges (using Tuple for pair)
            HashSet<(int, int)> edgeSet = new HashSet<(int, int)>();
            int wireframeLineId = 0;

            // Local function to add unique edge
            void AddUniqueEdge(int a, int b)
            {
                // Create ordered pair (smaller first, larger second) for uniqueness
                var edge = (Math.Min(a, b), Math.Max(a, b));

                // HashSet.Add returns true if the element was added to the HashSet
                if (edgeSet.Add(edge))
                {
                    wireframe_lines.Add(new line_store()
                    {
                        line_id = wireframeLineId++,
                        line_start_id = edge.Item1,
                        line_end_id = edge.Item2
                    });
                }
            }

            foreach (tri_store tri in tris.Values)
            {
                AddUniqueEdge(tri.pt_id1, tri.pt_id2);
                AddUniqueEdge(tri.pt_id2, tri.pt_id3);
                AddUniqueEdge(tri.pt_id3, tri.pt_id1);

            }

            foreach (quad_store quad in quads.Values)
            {
                AddUniqueEdge(quad.pt_id1, quad.pt_id2);
                AddUniqueEdge(quad.pt_id2, quad.pt_id3);
                AddUniqueEdge(quad.pt_id3, quad.pt_id4);
                AddUniqueEdge(quad.pt_id4, quad.pt_id1);

            }

        }


        public void create_buffer_data()
        {
            // Create the OpenGL buffer for the mesh
            pointIDToIndex.Clear();

            // Build point index mapping
            for (int i = 0; i < points.Count; i++)
            {
                pointIDToIndex[points[i].point_id] = i;

            }


            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> vertexData = new List<float>();
            List<int> pointIndexData = new List<int>();

            for (int i = 0; i < points.Count; i++)
            {
                point_store pt = points[i];
                vertexData.Add(pt.x_coord);
                vertexData.Add(pt.y_coord);
                pointIndexData.Add(i);
            }

            // Create VAO and VBO for points
            point_vao = new VertexArray();
            point_vbo = new VertexBuffer(Math.Max(10, vertexData.Count));
            point_ibo = new IndexBuffer(Math.Max(10, pointIndexData.Count));
            selected_point_ibo = new IndexBuffer(10); // Added dynamically


            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(FLOATS_PER_VERTEX);
            point_vao.Add_vertexBuffer(point_vbo, pointLayout);


            point_vbo.AppendVertexBuffer(vertexData.ToArray());
            point_ibo.AppendIndexBuffer(pointIndexData.ToArray());

            //_______________________________________________________________
            // prepare wireframe index data for openGL
            List<int> wireframeIndexData = new List<int>();

            foreach (line_store ln in wireframe_lines)
            {

                if (pointIDToIndex.TryGetValue(ln.line_start_id, out int startIdx) &&
                        pointIDToIndex.TryGetValue(ln.line_end_id, out int endIdx))
                {
                    wireframeIndexData.Add(startIdx);
                    wireframeIndexData.Add(endIdx);
                }

                //int start_idx = pointIDToIndex[ln.line_start_id];
                //int end_idx = pointIDToIndex[ln.line_end_id];

                //wireframeIndexData.Add(start_idx);
                //wireframeIndexData.Add(end_idx);

            }


            wireframe_ibo = new IndexBuffer(Math.Max(10, wireframeIndexData.Count));
            if (wireframeIndexData.Count > 0)
            {
                wireframe_ibo.AppendIndexBuffer(wireframeIndexData.ToArray());
            }

            //_______________________________________________________________
            // prepare triangle and quadrilateral index data for openGL

            foreach (mat_mesh_store matMesh in mat_mesh_data.Values)
            {
                List<int> triangleIndexData = new List<int>();


                foreach (int tri_id in matMesh.tri_ids)
                {
                    tri_store tri = tris[tri_id];

                    int pt_idx1 = pointIDToIndex[tri.pt_id1];
                    int pt_idx2 = pointIDToIndex[tri.pt_id2];
                    int pt_idx3 = pointIDToIndex[tri.pt_id3];


                    triangleIndexData.Add(pt_idx1);
                    triangleIndexData.Add(pt_idx2);
                    triangleIndexData.Add(pt_idx3);

                }

                matMesh.triangle_ibo = new IndexBuffer(Math.Max(10, triangleIndexData.Count));
                if (triangleIndexData.Count > 0)
                {
                    matMesh.triangle_ibo.AppendIndexBuffer(triangleIndexData.ToArray());
                }

                List<int> quadrilateralIndexData = new List<int>();

                foreach (int quad_id in matMesh.quad_ids)
                {
                    quad_store quad = quads[quad_id];

                    int pt_idx1 = pointIDToIndex[quad.pt_id1];
                    int pt_idx2 = pointIDToIndex[quad.pt_id2];
                    int pt_idx3 = pointIDToIndex[quad.pt_id3];
                    int pt_idx4 = pointIDToIndex[quad.pt_id4];


                    // Make two triangles from the quad (pt1, pt2, pt3) and (pt1, pt3, pt4)
                    // Triangle 1 (nd1, nd2, nd3)
                    quadrilateralIndexData.Add(pt_idx1);
                    quadrilateralIndexData.Add(pt_idx2);
                    quadrilateralIndexData.Add(pt_idx3);

                    // Triangle 2 (nd1, nd3, nd4)
                    quadrilateralIndexData.Add(pt_idx1);
                    quadrilateralIndexData.Add(pt_idx3);
                    quadrilateralIndexData.Add(pt_idx4);

                }

                matMesh.quadrilateral_ibo = new IndexBuffer(Math.Max(10, quadrilateralIndexData.Count));
                if (quadrilateralIndexData.Count > 0)
                {
                    matMesh.quadrilateral_ibo.AppendIndexBuffer(quadrilateralIndexData.ToArray());
                }

            }

            // Shrunk Mesh buffers
            generate_shrunk_mesh();

            buffersInitialized = true;
        }


        private void generate_shrunk_mesh()
        {

            point_store GetPointById(int pt_id)
            {
                int idx = pointIDToIndex[pt_id];

                return points[idx];
            }

            // Generate shrunk vertices for triangles
            foreach (tri_store tri in tris.Values)
            {
                var p1 = GetPointById(tri.pt_id1);
                var p2 = GetPointById(tri.pt_id2);
                var p3 = GetPointById(tri.pt_id3);

                shrunk_mesh_data.add_shrunk_triangle(tri.tri_id,
                    p1.x_coord, p1.y_coord, p2.x_coord, p2.y_coord, p3.x_coord, p3.y_coord,
                    tri.mat_id);
            }


            foreach (quad_store quad in quads.Values)
            {
                var p1 = GetPointById(quad.pt_id1);
                var p2 = GetPointById(quad.pt_id2);
                var p3 = GetPointById(quad.pt_id3);
                var p4 = GetPointById(quad.pt_id4);

                shrunk_mesh_data.add_shrunk_quadrilateral(quad.quad_id,
                    p1.x_coord, p1.y_coord, p2.x_coord, p2.y_coord,
                    p3.x_coord, p3.y_coord, p4.x_coord, p4.y_coord,
                    quad.mat_id);

            }


            // Initialize the buffer
            shrunk_mesh_data.create_shrunkmesh_buffer_data();

        }



        public void update_openTK_uniforms(drawing_events graphic_events_control)
        {
            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                graphic_events_control.viewMatrix * graphic_events_control.modelMatrix;

            meshShader.SetMatrix4("uMVP", uMVP);
        }


        public void Dispose()
        {
            point_vbo?.Dispose();
            point_vao?.Dispose();
            point_ibo?.Dispose();
            selected_point_ibo?.Dispose();
            wireframe_ibo?.Dispose();
            // meshShader?.Dispose();

            foreach (var matMesh in mat_mesh_data.Values)
            {
                matMesh.triangle_ibo?.Dispose();
                matMesh.quadrilateral_ibo?.Dispose();
            }
        }


    }
}
