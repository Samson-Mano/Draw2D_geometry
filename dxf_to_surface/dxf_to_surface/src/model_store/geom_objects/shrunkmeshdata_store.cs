using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.opentk_control.opentk_buffer;
using dxf_to_surface.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;





namespace dxf_to_surface.src.model_store.geom_objects
{
    public class shrunkmeshdata_store
    {

        private struct point_store
        {
            public int point_id;
            public float x_coord;
            public float y_coord;
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
            // Shrink rendering buffers 
            public IndexBuffer shrunk_triangle_ibo;
            public IndexBuffer shrunk_quadrilateral_ibo;

            public HashSet<int> shrunk_tri_ids = new HashSet<int>();
            public HashSet<int> shrunk_quad_ids = new HashSet<int>();

        }

        // Geometry data for OpenGL
        private Dictionary<int, int> pointIDToIndex = new Dictionary<int, int>();

        private VertexBuffer shrunk_point_vbo; // Pre-shrunk vertices
        private VertexArray shrunk_point_vao;

        public IndexBuffer selected_triangle_ibo;
        public IndexBuffer selected_quadrilateral_ibo;

        private int shrunkpt_id = 0;

        private const int FLOATS_PER_VERTEX = 2;
        private const float SHRINKFACTOR = 0.80f;

        private List<point_store> shrunk_points = new List<point_store>();
        private Dictionary<int, tri_store> shrunk_tris = new Dictionary<int, tri_store>();
        private Dictionary<int, quad_store> shrunk_quads = new Dictionary<int, quad_store>();


        private Dictionary<int, mat_mesh_store> mat_shrunkmesh_data = new Dictionary<int, mat_mesh_store>();

        private bool shrunk_buffersInitialized = false;

        // Selected mesh flag
        bool isMeshSelected = false;

        public shrunkmeshdata_store()
        {
            // Empty constructor
        }


        private void add_shrunk_point(int point_id, float x, float y)
        {
            shrunk_points.Add(new point_store()
            {
                point_id = point_id,
                x_coord = x,
                y_coord = y
            });
        }


        public void add_shrunk_triangle(int tri_id, float pt1_xcoord, float pt1_ycoord,
            float pt2_xcoord, float pt2_ycoord, float pt3_xcoord, float pt3_ycoord, int mat_id)
        {

            // Calculate centroid
            float cx = (pt1_xcoord + pt2_xcoord + pt3_xcoord) / 3.0f;
            float cy = (pt1_ycoord + pt2_ycoord + pt3_ycoord) / 3.0f;

            // Create 3 new points
            add_shrunk_point(shrunkpt_id + 0, cx + (pt1_xcoord - cx) * SHRINKFACTOR,
                cy + (pt1_ycoord - cy) * SHRINKFACTOR);
            add_shrunk_point(shrunkpt_id + 1, cx + (pt2_xcoord - cx) * SHRINKFACTOR,
                cy + (pt2_ycoord - cy) * SHRINKFACTOR);
            add_shrunk_point(shrunkpt_id + 2, cx + (pt3_xcoord - cx) * SHRINKFACTOR,
                cy + (pt3_ycoord - cy) * SHRINKFACTOR);


            if (!mat_shrunkmesh_data.TryGetValue(mat_id, out var matMesh))
            {
                matMesh = new mat_mesh_store() { material_id = mat_id };
                mat_shrunkmesh_data[mat_id] = matMesh;
            }

            shrunk_tris.Add(tri_id, new tri_store()
            {
                tri_id = tri_id,
                pt_id1 = shrunkpt_id + 0,
                pt_id2 = shrunkpt_id + 1,
                pt_id3 = shrunkpt_id + 2,
                mat_id = mat_id
            });

            matMesh.shrunk_tri_ids.Add(tri_id);

            shrunkpt_id += 3;

        }


        public void add_shrunk_quadrilateral(int quad_id, float pt1_xcoord, float pt1_ycoord,
            float pt2_xcoord, float pt2_ycoord, float pt3_xcoord, float pt3_ycoord,
            float pt4_xcoord, float pt4_ycoord, int mat_id)
        {
            // Calculate centroid
            float cx = (pt1_xcoord + pt2_xcoord + pt3_xcoord + pt4_xcoord) / 4.0f;
            float cy = (pt1_ycoord + pt2_ycoord + pt3_ycoord + pt4_ycoord) / 4.0f;

            // Create 4 new points
            add_shrunk_point(shrunkpt_id + 0, cx + (pt1_xcoord - cx) * SHRINKFACTOR,
                cy + (pt1_ycoord - cy) * SHRINKFACTOR);
            add_shrunk_point(shrunkpt_id + 1, cx + (pt2_xcoord - cx) * SHRINKFACTOR,
                cy + (pt2_ycoord - cy) * SHRINKFACTOR);
            add_shrunk_point(shrunkpt_id + 2, cx + (pt3_xcoord - cx) * SHRINKFACTOR,
                cy + (pt3_ycoord - cy) * SHRINKFACTOR);
            add_shrunk_point(shrunkpt_id + 3, cx + (pt4_xcoord - cx) * SHRINKFACTOR,
                cy + (pt4_ycoord - cy) * SHRINKFACTOR);


            if (!mat_shrunkmesh_data.TryGetValue(mat_id, out var matMesh))
            {
                matMesh = new mat_mesh_store() { material_id = mat_id };
                mat_shrunkmesh_data[mat_id] = matMesh;
            }

            shrunk_quads.Add(quad_id, new quad_store()
            {
                quad_id = quad_id,
                pt_id1 = shrunkpt_id + 0,
                pt_id2 = shrunkpt_id + 1,
                pt_id3 = shrunkpt_id + 2,
                pt_id4 = shrunkpt_id + 3,
                mat_id = mat_id
            });

            matMesh.shrunk_quad_ids.Add(quad_id);

            shrunkpt_id += 4;

        }


        public void create_shrunkmesh_buffer_data()
        {
            // Create the OpenGL buffer for the mesh
            pointIDToIndex.Clear();

            // Build point index mapping
            for (int i = 0; i < shrunk_points.Count; i++)
            {
                pointIDToIndex[shrunk_points[i].point_id] = i;

            }


            //_______________________________________________________________
            // prepare the Vertex data for openGL
            List<float> shrunk_vertexData = new List<float>();

            for (int i = 0; i < shrunk_points.Count; i++)
            {
                point_store pt = shrunk_points[i];
                shrunk_vertexData.Add(pt.x_coord);
                shrunk_vertexData.Add(pt.y_coord);
            }

            // Create VAO and VBO for points
            shrunk_point_vao = new VertexArray();
            shrunk_point_vbo = new VertexBuffer(Math.Max(10, shrunk_vertexData.Count));

            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(FLOATS_PER_VERTEX);

            shrunk_point_vao.Add_vertexBuffer(shrunk_point_vbo, pointLayout);
            shrunk_point_vbo.AppendVertexBuffer(shrunk_vertexData.ToArray());


            //_______________________________________________________________
            // prepare shrunk triangle and shrunk quadrilateral index data for openGL

            foreach (mat_mesh_store matMesh in mat_shrunkmesh_data.Values)
            {
                List<int> shrunk_triangleIndexData = new List<int>();


                foreach (int tri_id in matMesh.shrunk_tri_ids)
                {
                    tri_store tri = shrunk_tris[tri_id];

                    int pt_idx1 = pointIDToIndex[tri.pt_id1];
                    int pt_idx2 = pointIDToIndex[tri.pt_id2];
                    int pt_idx3 = pointIDToIndex[tri.pt_id3];


                    shrunk_triangleIndexData.Add(pt_idx1);
                    shrunk_triangleIndexData.Add(pt_idx2);
                    shrunk_triangleIndexData.Add(pt_idx3);

                }

                matMesh.shrunk_triangle_ibo = new IndexBuffer(Math.Max(10, shrunk_triangleIndexData.Count));
                if (shrunk_triangleIndexData.Count > 0)
                {
                    matMesh.shrunk_triangle_ibo.AppendIndexBuffer(shrunk_triangleIndexData.ToArray());
                }

                List<int> shrunk_quadrilateralIndexData = new List<int>();

                foreach (int quad_id in matMesh.shrunk_quad_ids)
                {
                    quad_store quad = shrunk_quads[quad_id];

                    int pt_idx1 = pointIDToIndex[quad.pt_id1];
                    int pt_idx2 = pointIDToIndex[quad.pt_id2];
                    int pt_idx3 = pointIDToIndex[quad.pt_id3];
                    int pt_idx4 = pointIDToIndex[quad.pt_id4];


                    // Make two triangles from the quad (pt1, pt2, pt3) and (pt1, pt3, pt4)
                    // Triangle 1 (nd1, nd2, nd3)
                    shrunk_quadrilateralIndexData.Add(pt_idx1);
                    shrunk_quadrilateralIndexData.Add(pt_idx2);
                    shrunk_quadrilateralIndexData.Add(pt_idx3);

                    // Triangle 2 (nd1, nd3, nd4)
                    shrunk_quadrilateralIndexData.Add(pt_idx1);
                    shrunk_quadrilateralIndexData.Add(pt_idx3);
                    shrunk_quadrilateralIndexData.Add(pt_idx4);

                }

                matMesh.shrunk_quadrilateral_ibo = new IndexBuffer(Math.Max(10, shrunk_quadrilateralIndexData.Count));
                if (shrunk_quadrilateralIndexData.Count > 0)
                {
                    matMesh.shrunk_quadrilateral_ibo.AppendIndexBuffer(shrunk_quadrilateralIndexData.ToArray());
                }

            }

            //  Initialize selected mesh IBO
            selected_triangle_ibo = new IndexBuffer(10);
            selected_quadrilateral_ibo = new IndexBuffer(10);

            shrunk_buffersInitialized = true;

        }

        public void shrunk_update_material(HashSet<int> selected_tri_ids, HashSet<int> selected_quad_ids, int mat_id)
        {
            if (!mat_shrunkmesh_data.TryGetValue(mat_id, out var matMesh))
            {
                matMesh = new mat_mesh_store() { material_id = mat_id };
                mat_shrunkmesh_data[mat_id] = matMesh;
            }

            // Update the material 
            matMesh.shrunk_tri_ids.UnionWith(selected_tri_ids);
            matMesh.shrunk_quad_ids.UnionWith(selected_quad_ids);

            foreach (mat_mesh_store matMesh_ot in mat_shrunkmesh_data.Values)
            {
                // Reset other mesh
                if (matMesh_ot.material_id == mat_id)
                    continue;   


                // Remove the tri ids & quad ids
                matMesh_ot.shrunk_tri_ids.ExceptWith(selected_tri_ids);
                matMesh_ot.shrunk_quad_ids.ExceptWith(selected_quad_ids);

            }

            reset_shrunk_matmesh_buffer();
        }


        public void shrunk_deletematerial(int mat_id)
        {
            List<int> materialchanged_element_tris = mat_shrunkmesh_data[mat_id].shrunk_tri_ids.ToList();
            List<int> materialchanged_element_quads = mat_shrunkmesh_data[mat_id].shrunk_quad_ids.ToList();

            // Delete the mat mesh associated with this material id
            mat_shrunkmesh_data.Remove(mat_id);

            // Assign default material to the element tris and quads
            mat_shrunkmesh_data[0].shrunk_tri_ids.UnionWith(materialchanged_element_tris);
            mat_shrunkmesh_data[0].shrunk_quad_ids.UnionWith(materialchanged_element_quads);

            reset_shrunk_matmesh_buffer();
        }

        private void reset_shrunk_matmesh_buffer()
        {
            //_______________________________________________________________
            // prepare shrunk triangle and shrunk quadrilateral index data for openGL

            foreach (mat_mesh_store matMesh in mat_shrunkmesh_data.Values)
            {
                List<int> shrunk_triangleIndexData = new List<int>();


                foreach (int tri_id in matMesh.shrunk_tri_ids)
                {
                    tri_store tri = shrunk_tris[tri_id];

                    int pt_idx1 = pointIDToIndex[tri.pt_id1];
                    int pt_idx2 = pointIDToIndex[tri.pt_id2];
                    int pt_idx3 = pointIDToIndex[tri.pt_id3];


                    shrunk_triangleIndexData.Add(pt_idx1);
                    shrunk_triangleIndexData.Add(pt_idx2);
                    shrunk_triangleIndexData.Add(pt_idx3);

                }

                if (matMesh.shrunk_triangle_ibo == null)
                    matMesh.shrunk_triangle_ibo = new IndexBuffer(10);


                matMesh.shrunk_triangle_ibo.ClearIndexBuffer();
                if (shrunk_triangleIndexData.Count > 0)
                {
                    matMesh.shrunk_triangle_ibo.AppendIndexBuffer(shrunk_triangleIndexData.ToArray());
                }

                List<int> shrunk_quadrilateralIndexData = new List<int>();

                foreach (int quad_id in matMesh.shrunk_quad_ids)
                {
                    quad_store quad = shrunk_quads[quad_id];

                    int pt_idx1 = pointIDToIndex[quad.pt_id1];
                    int pt_idx2 = pointIDToIndex[quad.pt_id2];
                    int pt_idx3 = pointIDToIndex[quad.pt_id3];
                    int pt_idx4 = pointIDToIndex[quad.pt_id4];


                    // Make two triangles from the quad (pt1, pt2, pt3) and (pt1, pt3, pt4)
                    // Triangle 1 (nd1, nd2, nd3)
                    shrunk_quadrilateralIndexData.Add(pt_idx1);
                    shrunk_quadrilateralIndexData.Add(pt_idx2);
                    shrunk_quadrilateralIndexData.Add(pt_idx3);

                    // Triangle 2 (nd1, nd3, nd4)
                    shrunk_quadrilateralIndexData.Add(pt_idx1);
                    shrunk_quadrilateralIndexData.Add(pt_idx3);
                    shrunk_quadrilateralIndexData.Add(pt_idx4);

                }

                if (matMesh.shrunk_quadrilateral_ibo == null)
                    matMesh.shrunk_quadrilateral_ibo = new IndexBuffer(10);


                matMesh.shrunk_quadrilateral_ibo.ClearIndexBuffer();
                if (shrunk_quadrilateralIndexData.Count > 0)
                {
                    matMesh.shrunk_quadrilateral_ibo.AppendIndexBuffer(shrunk_quadrilateralIndexData.ToArray());
                }

            }


        }

        public void paint_shrunk_mesh(ref Shader meshShader)
        {
            if (!shrunk_buffersInitialized)
                return;


            shrunk_point_vao.Bind();

            foreach (mat_mesh_store mat_mesh_vals in mat_shrunkmesh_data.Values)
            {

                // Get the Color for the mesh (based on material ID)
                Vector4 MeshColor = new Vector4(gvariables_static.ColorUtils.MeshGetRandomColor(mat_mesh_vals.material_id),
                    gvariables_static.geom_transparency);


                meshShader.SetVector4("vertexColor", MeshColor);


                if (mat_mesh_vals.shrunk_triangle_ibo.BufferCount > 0)
                {
                    // Paint the triangle mesh
                    mat_mesh_vals.shrunk_triangle_ibo.Bind();
                    GL.DrawElements(PrimitiveType.Triangles, mat_mesh_vals.shrunk_triangle_ibo.BufferCount,
                        DrawElementsType.UnsignedInt, 0);
                    mat_mesh_vals.shrunk_triangle_ibo.UnBind();
                }


                if (mat_mesh_vals.shrunk_quadrilateral_ibo.BufferCount > 0)
                {
                    // Paint the quadrilateral mesh
                    mat_mesh_vals.shrunk_quadrilateral_ibo.Bind();
                    GL.DrawElements(PrimitiveType.Triangles, mat_mesh_vals.shrunk_quadrilateral_ibo.BufferCount,
                        DrawElementsType.UnsignedInt, 0);
                    mat_mesh_vals.shrunk_quadrilateral_ibo.UnBind();
                }

            }

            shrunk_point_vao.UnBind();

        }


        public void add_selected_mesh(List<int> selected_tri_ids, List<int> selected_quad_ids)
        {
            // Add the selected mesh
            List<int> selected_triangleIndexData = new List<int>();

            foreach (int tri_id in selected_tri_ids)
            {
                tri_store tri = shrunk_tris[tri_id];

                int pt_idx1 = pointIDToIndex[tri.pt_id1];
                int pt_idx2 = pointIDToIndex[tri.pt_id2];
                int pt_idx3 = pointIDToIndex[tri.pt_id3];


                selected_triangleIndexData.Add(pt_idx1);
                selected_triangleIndexData.Add(pt_idx2);
                selected_triangleIndexData.Add(pt_idx3);

            }

            selected_triangle_ibo.ClearIndexBuffer();
            if (selected_triangleIndexData.Count > 0)
            {
                selected_triangle_ibo.AppendIndexBuffer(selected_triangleIndexData.ToArray());
            }

            List<int> selected_quadrilateralIndexData = new List<int>();

            foreach (int quad_id in selected_quad_ids)
            {
                quad_store quad = shrunk_quads[quad_id];

                int pt_idx1 = pointIDToIndex[quad.pt_id1];
                int pt_idx2 = pointIDToIndex[quad.pt_id2];
                int pt_idx3 = pointIDToIndex[quad.pt_id3];
                int pt_idx4 = pointIDToIndex[quad.pt_id4];


                // Make two triangles from the quad (pt1, pt2, pt3) and (pt1, pt3, pt4)
                // Triangle 1 (nd1, nd2, nd3)
                selected_quadrilateralIndexData.Add(pt_idx1);
                selected_quadrilateralIndexData.Add(pt_idx2);
                selected_quadrilateralIndexData.Add(pt_idx3);

                // Triangle 2 (nd1, nd3, nd4)
                selected_quadrilateralIndexData.Add(pt_idx1);
                selected_quadrilateralIndexData.Add(pt_idx3);
                selected_quadrilateralIndexData.Add(pt_idx4);

            }

            selected_quadrilateral_ibo.ClearIndexBuffer();
            if (selected_quadrilateralIndexData.Count > 0)
            {
                selected_quadrilateral_ibo.AppendIndexBuffer(selected_quadrilateralIndexData.ToArray());
            }


            isMeshSelected = false;
            if(selected_triangle_ibo.BufferCount > 0 || selected_quadrilateral_ibo.BufferCount > 0)
                isMeshSelected =true;

        }

        public void clear_selected_mesh()
        {
            selected_triangle_ibo.ClearIndexBuffer();
            selected_quadrilateral_ibo.ClearIndexBuffer();

            // Clear the selected mesh
            isMeshSelected = false;
        }


        public void paint_selected_mesh(ref Shader meshShader)
        {
            if (!shrunk_buffersInitialized && !isMeshSelected)
                return;

            shrunk_point_vao.Bind();


            // Get the Color for the mesh (based on material ID)
            Vector4 MeshColor = new Vector4(gvariables_static.ColorUtils.get_SelectionPtColor(),
                gvariables_static.geom_transparency);


            meshShader.SetVector4("vertexColor", MeshColor);


            if (selected_triangle_ibo.BufferCount > 0)
            {
                // Paint the selected triangle mesh
                selected_triangle_ibo.Bind();
                GL.DrawElements(PrimitiveType.Triangles, selected_triangle_ibo.BufferCount,
                    DrawElementsType.UnsignedInt, 0);
                selected_triangle_ibo.UnBind();
            }


            if (selected_quadrilateral_ibo.BufferCount > 0)
            {
                // Paint the selected quadrilateral mesh
                selected_quadrilateral_ibo.Bind();
                GL.DrawElements(PrimitiveType.Triangles, selected_quadrilateral_ibo.BufferCount,
                    DrawElementsType.UnsignedInt, 0);
                selected_quadrilateral_ibo.UnBind();
            }

            shrunk_point_vao.UnBind();


        }


    }
}
