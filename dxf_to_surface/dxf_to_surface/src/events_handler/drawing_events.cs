// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using dxf_to_surface.src.global_variables;
using dxf_to_surface.src.model_store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace dxf_to_surface.src.events_handler
{
    public class drawing_events
    {
        private readonly modeldata_store modeldata;

        private Vector2 click_pt = new Vector2(0);
        // private Vector2 curr_pt = new Vector2(0);
        private Vector2 prev_translation = new Vector2(0);
        private Vector2 total_translation = new Vector2(0);


        private bool isCtrlDown = false;
        private bool isShiftDown = false;


        private bool is_pan = false;
        private bool is_rightbutton = false;
        private bool is_select = false;
        public double zoom_val = 1.0;

        public int window_width = 0;
        public int window_height = 0;


        public Matrix4 modelMatrix { get; private set; } = Matrix4.Zero; // model matrix
        public Matrix4 viewMatrix { get; private set; } = Matrix4.Identity; // view matrix
        public Matrix4 projectionMatrix { get; private set; } = Matrix4.Identity; // projection matrix


        // Temporary variables to initiate the zoom to fit animation
        public bool isZoomToFitInProgress = false;
        private double param_t = 0.0f;
        private double temp_zm = 1.0f;
        private Vector2 temp_transl = new Vector2(0);

        private Timer myTimer = new Timer();

        public drawing_events(modeldata_store modeldata)
        {
            // Set the model data
            this.modeldata = modeldata;


            // Set the default projection matrix
            // Define the orthographic projection parameters
            float left = -1.0f;
            float right = 1.0f;
            float bottom = -1.0f;
            float top = 1.0f;
            float nearPlane = 100.0f;
            float farPlane = -100.0f;

            // Create the orthographic projection matrix
            Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, nearPlane, farPlane);

            // Assign it to your class or struct
            this.projectionMatrix = projectionMatrix;

            this.modeldata.update_openTK_uniforms();
        }


        #region "Handle Mouse Events"
        public void handleMouseLeftButtonClick(bool isDown, float e_X, float e_Y)
        {
            if (isDown == true)
            {
                // Left mouse button press
                if (isCtrlDown == true)
                {
                    // Rotate operation
                }

                if (isShiftDown == true)
                {
                    // Select operation starts (Left drag)
                    select_operation_start(new Vector2(e_X, e_Y), false);

                }
            }
            else
            {
                // Left mouse button release
                isCtrlDown = false;
                isShiftDown = false;

                if (is_select == true)
                {
                    select_operation_end(new Vector2(e_X, e_Y));
                }

            }

        }

        public void handleMouseRightButtonClick(bool isDown, float e_X, float e_Y)
        {
            if (isDown == true)
            {

                // Right mouse button press
                if (isCtrlDown == true)
                {
                    // Pan operation
                    pan_operation_start(new Vector2(e_X, e_Y));

                }

                if (isShiftDown == true)
                {
                    // Select operation starts (Right drag)
                    select_operation_start(new Vector2(e_X, e_Y), true);
                }

            }
            else
            {
                // Right mouse button release
                // isCtrlDown = false;
                // isShiftDown = false;

                // Right mouse button release
                if (is_pan == true)
                {
                    pan_operation_end();
                }

                if (is_select == true)
                {
                    select_operation_end(new Vector2(e_X, e_Y));
                }
            }

        }


        public void handleMouseMove(float e_X, float e_Y)
        {
            if (isCtrlDown == true || isShiftDown == true)
            {
                // Perform the mouse move operation
                Vector2 loc = new Vector2(e_X, e_Y);
                mouse_location(loc);

            }

        }

        public void handleMouseScroll(int e_Delta, float e_X, float e_Y)
        {
            if (isCtrlDown == true)
            {
                // Perform zoom operation
                zoom_operation(e_Delta, e_X, e_Y);

            }

        }

        public void handleKeyboardAction(bool isDown, int key)
        {
            if (isDown == true)
            {
                // Key pressed
                Keys pressedKey = (Keys)key;


                // Modifier keys
                isShiftDown = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
                isCtrlDown = (Control.ModifierKeys & Keys.Control) == Keys.Control;

                if (isCtrlDown && pressedKey == Keys.F)
                {
                    // Perform zoom to fit
                    zoom_to_fit();
                }
            }
            else
            {
                // key released
                isCtrlDown = false;
                isShiftDown = false;
            }

        }

        #endregion


        private void mouse_location(Vector2 loc)
        {

            if (is_pan == true)
            {
                // Pan operation in progress
                Vector2 delta_d = click_pt - loc;
                // pan
                Vector2 current_translation = delta_d / (float)(Math.Max(window_width, window_height) * 0.5f);

                pan_operation(current_translation);
            }

            // Select operation in progress
            if (is_select == true)
            {
                select_operation(loc);
            }

        }


        public void update_drawing_area_size(int window_width, int window_height)
        {
            // Update the drawing area size
            this.window_width = window_width;
            this.window_height = window_height;

            // 1. Determine max dimension
            int max_drawing_area_size = Math.Max(window_width, window_height);

            int drawing_area_center_x = (int)((window_width - max_drawing_area_size) * 0.5f);
            int drawing_area_center_y = (int)((window_height - max_drawing_area_size) * 0.5f);

            // 1A. Set the viewport
            // Update the graphics drawing area
            GL.Viewport(drawing_area_center_x, drawing_area_center_y,
                max_drawing_area_size, max_drawing_area_size);


            // 2. Normalize screen dimensions
            double normalizedScreenWidth = 1.8d * ((double)window_width / (double)max_drawing_area_size);
            double normalizedScreenHeight = 1.8d * ((double)window_height / (double)max_drawing_area_size);

            // 3. Compute scale factor
            double geom_scale = Math.Min(normalizedScreenWidth / modeldata.geom_bounds.X,
                normalizedScreenHeight / modeldata.geom_bounds.Y);

            // 4. Compute translation to center geometry
            Vector3 geomTranslation = new Vector3(
                -1.0f * (float)((modeldata.max_bounds.X + modeldata.min_bounds.X) * 0.5 * geom_scale),
                -1.0f * (float)((modeldata.max_bounds.Y + modeldata.min_bounds.Y) * 0.5 * geom_scale),
                0.0f
            );

            // 5. Build model matrix
            Matrix4 translationMatrix = Matrix4.Transpose(Matrix4.CreateTranslation(geomTranslation));
            Matrix4 scaleMatrix = Matrix4.CreateScale((float)geom_scale, (float)geom_scale, 1.0f);

            // 6. Combine into model matrix
            this.modelMatrix = translationMatrix * scaleMatrix;

            this.modeldata.update_openTK_uniforms();

        }


        private void zoom_operation(int e_Delta, float e_X, float e_Y)
        {
            // Perform intelli zoom operation
            // Screen point before zoom
            Vector2 screen_pt_b4_scale = intellizoom_normalized_screen_pt(e_X, e_Y);

            // Zoom operation
            zoom_val = gvariables_static.UpdateZoom(zoom_val, e_Delta);

            // Transformed Hypothetical Screen point after zoom
            Vector2 screen_pt_a4_scale = intellizoom_normalized_screen_pt(e_X, e_Y);
            Vector2 g_tranl = -0.5f * (float)zoom_val * (screen_pt_b4_scale - screen_pt_a4_scale);

            // Perform Translation for Intelli Zoom
            pan_operation(g_tranl);
            pan_operation_end();

        }


        private void pan_operation_start(Vector2 loc)
        {
            // Pan operation start
            is_pan = true;
            // Note the click point when the pan operation start
            click_pt = loc;

        }


        private void pan_operation_end()
        {
            // End the pan operation saving translate transformation
            // Pan operation complete
            prev_translation = total_translation;
            is_pan = false;
        }


        private void select_operation_start(Vector2 loc, bool is_rightbutton)
        {
            // Select operation start
            is_select = true;
            this.is_rightbutton = is_rightbutton;

            // Note the click point when the pan operation start
            click_pt = loc;

        }


        private void select_operation_end(Vector2 current_loc)
        {
            // Location when the selection rectangle ends
            modeldata.selection_rectangle.UpdateSelectionRectangle(new Vector2(0), new Vector2(0), false);
            modeldata.selection_circle.UpdateSelectionCircle(new Vector2(0), new Vector2(0), false);


            int max_dim = window_width > window_height ? window_width : window_height;

            // Transform the mouse location to openGL screen coordinates
            float screen_opt_x = 2.0f * ((click_pt.X - (window_width * 0.5f)) / max_dim);
            float screen_opt_y = 2.0f * (((window_height * 0.5f) - click_pt.Y) / max_dim);

            float screen_cpt_x = 2.0f * ((current_loc.X - (window_width * 0.5f)) / max_dim);
            float screen_cpt_y = 2.0f * (((window_height * 0.5f) - current_loc.Y) / max_dim);

            Vector2 o_pt = new Vector2(screen_opt_x, screen_opt_y);
            Vector2 c_pt = new Vector2(screen_cpt_x, screen_cpt_y);

           this.modeldata.select_model_objects(o_pt, c_pt, is_rightbutton);

            is_select = false;
        }




        public void zoom_to_fit()
        {
            // Save the current zoom and translation values to temporary variables (for the animation)
            param_t = 0.0f;
            temp_zm = zoom_val;
            temp_transl = prev_translation;


            myTimer = new Timer();
            myTimer.Enabled = true;
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 10;
            myTimer.Start();
        }


        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            param_t = param_t + 0.05f;

            if (param_t > 1.0f)
            {
                // Set the zoom value to 1.0f
                this.zoom_val = 1.0;
                pan_operation(-temp_transl);
                pan_operation_end();

                // Refresh the painting area
                isZoomToFitInProgress = false;
                // this_Gcntrl.Invalidate();

                // End the animation
                myTimer.Stop();
                return;
            }

            // map param_t through easing
            double easedT = gvariables_static.EaseInOut(param_t);

            // Animate the translation & zoom value
            this.zoom_val = temp_zm * (1 - easedT) + (1.0f * easedT);
            pan_operation(-(float)easedT * temp_transl);

            // Refresh the painting area
            isZoomToFitInProgress = true;
            // this_Gcntrl.Invalidate();

        }




        public Vector2 intellizoom_normalized_screen_pt(float e_X, float e_Y)
        {
            // Function returns normalized screen point for zoom operation
            Vector2 loc = new Vector2(e_X, e_Y);

            Vector2 mid_pt = new Vector2((float)window_width, (float)window_height) * 0.5f;
            int min_size = (int)Math.Min(window_width, window_height);

            Vector2 mouse_pt = (-1.0f * (loc - mid_pt)) / (min_size * 0.5f);

            return (mouse_pt - (2.0f * prev_translation)) / (float)(zoom_val);

        }


        // Private function
        private void pan_operation(Vector2 current_translation)
        {
            // Pan operation in progress
            total_translation = (prev_translation + current_translation);

            //__________________________________________________________________________________________
            // Update the openGL Uniform matrix (View Matrix)
            Matrix4 panTranslation = Matrix4.Identity;

            // Apply translation
            panTranslation.M41 = -1.0f * total_translation.X; // X translation (negated)
            panTranslation.M42 = total_translation.Y;         // Y translation 


            Matrix4 scalingMatrix = Matrix4.CreateScale((float)zoom_val, (float)zoom_val, 1.0f);

            this.viewMatrix = Matrix4.Transpose(panTranslation) * scalingMatrix;

            this.modeldata.update_openTK_uniforms();
        }


        private void select_operation(Vector2 current_loc)
        {
            // Convert the point to screen coordinates
            // Set the parameters
            int max_dim = window_width > window_height ? window_width : window_height;

            // Transform the mouse location to openGL screen coordinates
            float screen_opt_x = 2.0f * ((click_pt.X - (window_width * 0.5f)) / max_dim);
            float screen_opt_y = 2.0f * (((window_height * 0.5f) - click_pt.Y) / max_dim);

            float screen_cpt_x = 2.0f * ((current_loc.X - (window_width * 0.5f)) / max_dim);
            float screen_cpt_y = 2.0f * (((window_height * 0.5f) - current_loc.Y) / max_dim);

            Vector2 o_pt = new Vector2(screen_opt_x, screen_opt_y);
            Vector2 c_pt = new Vector2(screen_cpt_x, screen_cpt_y);

            if (gvariables_static.is_RectangleSelection == true)
            {
                // Update the selection rectangle points
                this.modeldata.selection_rectangle.UpdateSelectionRectangle(o_pt, c_pt, true);
            }
            else
            {
                // Update the selection circle points
                this.modeldata.selection_circle.UpdateSelectionCircle(o_pt, c_pt, true);
            }

        }



        //__________________
    }
}
