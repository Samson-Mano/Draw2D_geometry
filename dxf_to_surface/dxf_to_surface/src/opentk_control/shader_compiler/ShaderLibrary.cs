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
            MeshShader,
            TextShader,
            ConstraintShader,
            LoadShader,
            ReactionForceShader,
            RsltMeshShader,
            RsltWireframeShader,
            RsltPSLShader,
            RsltPSLType2Shader,
            RsltPSLType2StreamFunctionShader,
            SelectionShader,
            DrawingAxisShader,
            ContourBarShader
        }


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




        #region "Result Mesh Shaders"

        private static string rslt_mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform float geomscale = 1.0f; // Geometry scale factor
            uniform float sinevalue = 1.0f;                    
            uniform float modelpercent = 0.01; // default 1 % scale factor             
            uniform float rsltoption = 0.0; 

            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aDisplacement;
            layout(location = 2) in float aDisplacementMagnitude;
            layout(location = 3) in float aScalarValue;
                    
            out float v_deflscale;
                    
            void main()
            {
                float scalevalue = geomscale * modelpercent * aDisplacementMagnitude;
                vec2 scaledDisplacement = aDisplacement * scalevalue * sinevalue;

                gl_Position = uMVP * vec4(aPosition + scaledDisplacement, 0.0, 1.0);
                
                float contourcolor = aScalarValue * sinevalue;  

                if(rsltoption != 0)
                    contourcolor = (contourcolor + 1.0) * 0.5; // Normalize to [0,1] if option is set

                v_deflscale = contourcolor;

            }


                    ";

        }

        private static string rslt_mesh_frag_shader()
        {

            return @"

            #version 330 core

            uniform float vertexTransparency; // Transparency of the mesh
            uniform float uNumContours = 10.0;      // number of contour bands
            uniform float uLineWidth = 1.0;         // contour line thickness (in pixels, roughly)
            uniform vec3  uLineColor = vec3(0.0);   // contour line color (black by default)
            uniform float uLineOpacity = 1.0;       // how strongly lines blend over the heatmap
            uniform float uMinContourValue = 0.01;  // minimum value for contour lines (smooth falloff)

            in float v_deflscale;

            out vec4 fColor;

            vec3 jetHeatmap(float value) 
            {
                // values between 0.0 and 1.0 are mapped to the jet colormap
                float t = value;
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            // Returns 1.0 exactly on a contour line, fading to 0.0 away from it
            float contourLines(float value, float numContours, float lineWidth)
            {
                float scaled = value * numContours;
                float distToLine = abs(fract(scaled + 0.5) - 0.5); // distance to nearest integer
                float aa = fwidth(scaled) * lineWidth;              // pixel-based line width
                return 1.0 - smoothstep(0.0, aa, distToLine);
            }


            void main()
            {
                vec3 baseColor = vec3(0.0); // jetHeatmap(v_deflscale);
                
                float line = 0.2f;

                // Contour bands
                if (v_deflscale < 0.0f)
                {
                    baseColor = vec3(0.4f, 0.4f, 0.4f); // Dark gray for negative values
                    line = 0.0f; // Disable contour lines for negative values
                }
                else if (v_deflscale > 1.0f)
                {
                    baseColor = vec3(0.8f, 0.8f, 0.8f); // Light gray for above 1.0 values
                    line = 0.0f; // Disable contour lines for values above 1.0
                }
                else
                {
                    baseColor = jetHeatmap(v_deflscale);
                }


                if (uLineOpacity > 0.1 && line > 0.1f)
                {
                    line = contourLines(v_deflscale, uNumContours, uLineWidth);
            
                    // Smoothly fade contour lines as value approaches zero
                    float valueScale = abs(v_deflscale);
                    float falloff = smoothstep(0.0, uMinContourValue, valueScale);
                    line *= falloff;
                }

                vec3 finalColor = mix(baseColor, uLineColor, line * uLineOpacity);

                fColor = vec4(finalColor, vertexTransparency);
            }


                    ";

        }

        #endregion


        #region "Result WireFrame Mesh Shaders"

        private static string rslt_wireframe_vert_shader()
        {
            return @"
            #version 330 core

            uniform mat4 uMVP;
            uniform float geomscale = 1.0f;
            uniform float sinevalue = 1.0f;                    
            uniform float modelpercent = 0.01;
    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aDisplacement;
            layout(location = 2) in float aDisplacementMagnitude;
            layout(location = 3) in float aScalarValue;
    
            out float v_deflscale;
        
            void main()
            {
                float scalevalue = geomscale * modelpercent * aDisplacementMagnitude;
                vec2 scaledDisplacement = aDisplacement * scalevalue * sinevalue;
                gl_Position = uMVP * vec4(aPosition + scaledDisplacement, 0.0, 1.0);

                v_deflscale = aScalarValue * sinevalue;
            }
            ";
        }


        private static string rslt_wireframe_frag_shader()
        {
            return @"
            #version 330 core
    
            uniform float wireframeAlpha;
    
            in float v_deflscale;
    
            out vec4 fColor;
    
            vec3 jetHeatmap(float value) 
            {
                float t = value; // (value + 1.0) * 0.5;
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }
    
            void main()
            {
                vec3 contourColor = jetHeatmap(v_deflscale);
                vec3 finalColor;
        
         
                // Use complementary color with some brightness enhancement
                finalColor = vec3(1.0) - contourColor;

                // Make it brighter for visibility
                finalColor = mix(finalColor, vec3(1.0), 0.1);
              
        
                fColor = vec4(finalColor, wireframeAlpha);
            }
            ";
        }

        #endregion


        #region "Result Plane stress line shader"


        private static string rslt_psline_mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform float geomscale = 1.0f; // Geometry scale factor
            uniform float sinevalue = 1.0f;                    
            uniform float modelpercent = 0.01; // default 1 % scale factor             

            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aDisplacement;
            layout(location = 2) in float aDisplacementMagnitude;
            layout(location = 3) in float aSigma1; // Principal stress value 1
            layout(location = 4) in float aSigma2; // Principal stress value 2
            layout(location = 5) in vec2 aDirection1; // Direction of principal stress 1 x 
            layout(location = 6) in vec2 aDirection2; // Direction of principal stress 2 x 
            
            out float v_geomscale;        // Pass geomscale to fragment

            out vec2 v_worldPos;         // World position for distance calculations

            out float v_sigma1;          // Pass sigma1 to fragment
            out float v_sigma2;          // Pass sigma2 to fragment

            out vec2 v_direction1;       // Pass direction1 to fragment
            out vec2 v_direction2;       // Pass direction2 to fragment                    

                    
            void main()
            {
                float scalevalue = geomscale * modelpercent * aDisplacementMagnitude;
                vec2 scaledDisplacement = aDisplacement * scalevalue * sinevalue * 0.0f;

                gl_Position = uMVP * vec4(aPosition + scaledDisplacement, 0.0, 1.0);
                
                // float contourcolor = aDisplacementMagnitude * sinevalue;  

                
                // Pass the principal stress values and angle to the fragment shader
                v_geomscale = geomscale;
                v_worldPos = aPosition + scaledDisplacement;

                v_sigma1 = aSigma1;
                v_sigma2 = aSigma2;

                v_direction1 = aDirection1;
                v_direction2 = aDirection2;
            }


                    ";

        }


        private static string rslt_psline_mesh_frag_shader()
        {

            return @"
            
            #version 330 core
            uniform float uNumContours = 10.0;      // number of contour bands
            uniform float uLineWidthMax = 2.0;
            uniform float uLineWidthMin = 1.0;
            uniform float uDensity = 1.0;

            uniform bool uShowTension = true;
            uniform bool uShowCompression = true;
            uniform vec3 uTensionColor = vec3(1.0, 0.0, 0.0);
            uniform vec3 uCompressionColor = vec3(0.0, 0.0, 1.0);
            
            in float v_geomscale;
            in vec2  v_worldPos;
            in float v_sigma1; // Normalized principal stress value 1 (value between 0 and 1)
            in float v_sigma2; // Normalized principal stress value 2 (value between 0 and 1)
            in vec2  v_direction1; // Normalized Direction of principal stress 1
            in vec2  v_direction2; // Normalized Direction of principal stress 2

            out vec4 fragColor;

            void main() 
            {
                // Initialize with black background (or transparent)
                vec3 color = vec3(0.0);
                float alpha = 0.2;
                
                // Adjust density based on geomscale to maintain consistent visual density across different scales
                float adjustedDensity = (uDensity / v_geomscale) * (uNumContours / 10.0); // Adjust density based on geomscale
                float adjustedLineWidthMax = uLineWidthMax * v_geomscale; // Adjust max line width based on geomscale
                float adjustedLineWidthMin = uLineWidthMin * v_geomscale; // Adjust min line width based on geomscale


                vec2 pos = v_worldPos;
    
                // Create grid of streamline seeds based on position and density
                float gridSize = 0.05 / adjustedDensity; // Adjust grid spacing
                vec2 gridPos = floor(pos / gridSize) * gridSize + gridSize * 0.5;
    
                // Check if we're near a seed point
                vec2 offset = pos - gridPos;
    
                // Process tension (sigma1) - RED color, width varies with sigma1
                if (uShowTension && v_sigma1 > 0.01) 
                {
                    // Calculate line width based on sigma1 value (maps 0-1 to min-max width)
                    float lineWidth = mix(adjustedLineWidthMin, adjustedLineWidthMax, v_sigma1);
                    float lineRadius = lineWidth * 0.001; // Scale appropriately
        
                    // Create a streamline along direction1
                    vec2 direction = v_direction1;
                    vec2 perpDir = vec2(-direction.y, direction.x);
        
                    // Compute distance to the streamline passing through the seed
                    float distToLine = abs(dot(offset, perpDir));
        
                    // Blend the line - constant color, varying width
                    if (distToLine < lineRadius) 
                    {
                        // Smooth falloff at edges (optional)
                        float lineFactor = 1.0 - (distToLine / lineRadius);
                        float alphaFactor = lineFactor * 0.9; // Slightly transparent at edges
            
                        // Use constant tension color (no intensity modulation)
                        color += uTensionColor * alphaFactor;
                        alpha = max(alpha, alphaFactor);
                    }
                }
    
                // Process compression (sigma2) - BLUE color, width varies with sigma2
                if (uShowCompression && v_sigma2 > 0.01) 
                {
                    // Calculate line width based on sigma2 value (maps 0-1 to min-max width)
                    float lineWidth = mix(adjustedLineWidthMin, adjustedLineWidthMax, v_sigma2);
                    float lineRadius = lineWidth * 0.001; // Scale appropriately
        
                    // Create a streamline along direction2
                    vec2 direction = v_direction2;
                    vec2 perpDir = vec2(-direction.y, direction.x);
        
                    // Compute distance to the streamline passing through the seed
                    float distToLine = abs(dot(offset, perpDir));
        
                    // Blend the line - constant color, varying width
                    if (distToLine < lineRadius) 
                    {
                        // Smooth falloff at edges (optional)
                        float lineFactor = 1.0 - (distToLine / lineRadius);
                        float alphaFactor = lineFactor * 0.9; // Slightly transparent at edges
            
                        // Use constant compression color (no intensity modulation)
                        color += uCompressionColor * alphaFactor;
                        alpha = max(alpha, alphaFactor);
                    }
                }
    
                // Optional: Add glow effect (maintains constant colors but adds glow based on width)
                if (alpha > 0.0) 
                {
                    // Calculate glow radius based on max possible width
                    float maxLineWidth = adjustedLineWidthMax * 0.001;
                    float glowRadius = maxLineWidth * 3.0;
        
                    float distToAnyLine = min(
                        uShowTension ? abs(dot(offset, vec2(-v_direction1.y, v_direction1.x))) : 999.0,
                        uShowCompression ? abs(dot(offset, vec2(-v_direction2.y, v_direction2.x))) : 999.0
                    );
        
                    if (distToAnyLine < glowRadius) 
                    {
                        float glowFactor = exp(-distToAnyLine * distToAnyLine / (glowRadius * glowRadius * 0.5));
                        // Glow should be subtle, don't overwhelm the main lines
                        color += color * glowFactor * 0.2;
                        alpha = min(alpha + glowFactor * 0.05, 1.0);
                    }
                }
    
                // Clamp and output
                fragColor = vec4(clamp(color, 0.0, 1.0), clamp(alpha, 0.0, 1.0));
            }

        ";
        }


        private static string rslt_psline_mesh_frag_shader_r1()
        {

            return @"
            
            #version 330 core

            uniform float uLineWidth = 5.0;
            uniform float uDensity = 1.0;


            uniform bool uShowTension = true;
            uniform bool uShowCompression = true;
            uniform vec3 uTensionColor = vec3(1.0, 0.0, 0.0);
            uniform vec3 uCompressionColor = vec3(0.0, 0.0, 1.0);

            in vec2  v_worldPos;
            in float v_sigma1; // Normalized principal stress value 1 (value between 0 and 1)
            in float v_sigma2; // Normalized principal stress value 2 (value between 0 and 1)
            in vec2  v_direction1; // Normalized Direction of principal stress 1
            in vec2  v_direction2; // Normalized Direction of principal stress 2

            out vec4 fragColor;

          
            // Function to compute distance to a line segment
            float distToLine(vec2 p, vec2 a, vec2 b) 
            {
                vec2 ap = p - a;
                vec2 ab = b - a;
                float t = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
                vec2 closest = a + t * ab;
                return length(p - closest);
            }

            // Function to trace streamline
            vec2 traceStreamline(vec2 start, vec2 direction, float stepSize, int maxSteps) 
            {
                vec2 pos = start;
                for (int i = 0; i < maxSteps; i++) 
                {
                    // In a real implementation, you'd sample the direction field here
                    // For this example, we use the interpolated direction
                    pos += direction * stepSize;
                }
                return pos;
            }

            void main() 
            {
                // Initialize with black background (or transparent)
                vec3 color = vec3(0.0);
                float alpha = 0.0;
    
                vec2 pos = v_worldPos;
    
                // Create grid of streamline seeds based on position and density
                float gridSize = 0.05 / uDensity; // Adjust grid spacing
                vec2 gridPos = floor(pos / gridSize) * gridSize + gridSize * 0.5;
    
                // Check if we're near a seed point
                vec2 offset = pos - gridPos;
                float distToSeed = length(offset);
    
                // Only render streamlines near seed points
                float lineRadius = uLineWidth * 0.001; // Scale line width appropriately
    
                // Process tension (sigma1)
                if (uShowTension && v_sigma1 > 0.01) {
                    // Create a streamline along direction1
                    vec2 direction = v_direction1;
                    vec2 perpDir = vec2(-direction.y, direction.x);
        
                    // Compute distance to the streamline passing through the seed
                    float distToLine = abs(dot(offset, perpDir));
        
                    // Calculate intensity based on sigma1 value
                    float intensity = v_sigma1;
        
                    // Blend the line
                    if (distToLine < lineRadius) {
                        float lineFactor = 1.0 - (distToLine / lineRadius);
                        float alphaFactor = lineFactor * intensity;
            
                        // Mix with tension color
                        vec3 lineColor = uTensionColor * intensity;
                        color += lineColor * alphaFactor;
                        alpha = max(alpha, alphaFactor * 0.8);
                    }
                }
    
                // Process compression (sigma2)
                if (uShowCompression && v_sigma2 > 0.01) {
                    // Create a streamline along direction2
                    vec2 direction = v_direction2;
                    vec2 perpDir = vec2(-direction.y, direction.x);
        
                    // Compute distance to the streamline passing through the seed
                    float distToLine = abs(dot(offset, perpDir));
        
                    // Calculate intensity based on sigma2 value
                    float intensity = v_sigma2;
        
                    // Blend the line
                    if (distToLine < lineRadius) {
                        float lineFactor = 1.0 - (distToLine / lineRadius);
                        float alphaFactor = lineFactor * intensity;
            
                        // Mix with compression color
                        vec3 lineColor = uCompressionColor * intensity;
                        color += lineColor * alphaFactor;
                        alpha = max(alpha, alphaFactor * 0.8);
                    }
                }
    
                // Add glow effect to make streamlines more visible
                if (alpha > 0.0) {
                    // Soft glow around lines
                    float glowRadius = lineRadius * 3.0;
                    float distToAnyLine = min(
                        uShowTension ? abs(dot(offset, vec2(-v_direction1.y, v_direction1.x))) : 999.0,
                        uShowCompression ? abs(dot(offset, vec2(-v_direction2.y, v_direction2.x))) : 999.0
                    );
        
                    if (distToAnyLine < glowRadius) {
                        float glowFactor = exp(-distToAnyLine * distToAnyLine / (glowRadius * glowRadius * 0.5));
                        color += color * glowFactor * 0.3;
                        alpha = min(alpha + glowFactor * 0.1, 1.0);
                    }
                }
    
                // Clamp and output
                fragColor = vec4(clamp(color, 0.0, 1.0), clamp(alpha, 0.0, 1.0));
            }


        ";
        }



        private static string rslt_psline_mesh_frag_shader_r0()
        {

            return @"


            # version 330 core

            uniform float vertexTransparency;

            uniform float uNumContours = 10.0;
            uniform float uLineWidth = 1.0;
            uniform vec3  uLineColor = vec3(0.0);
            uniform float uLineOpacity = 1.0;
            uniform float uMinContourValue = 0.01;

            // Controls density of principal stress lines.
            uniform float uStressLineDensity = 25.0;

            in float v_sigma1;
            in float v_sigma2;
            in float v_principalAngle;
            in vec2  v_direction1;
            in vec2  v_direction2;
            in float v_deflscale;

            out vec4 fColor;


            // ------------------------------------------------------------
            // Heatmap
            // ------------------------------------------------------------

            vec3 jetHeatmap(float value)
            {
                float t = clamp(value, 0.0, 1.0);

                return clamp(
                    vec3(1.5) -
                    abs(4.0 * vec3(t) + vec3(-3.0, -2.0, -1.0)),
                    vec3(0.0),
                    vec3(1.0)
                );
            }


            // ------------------------------------------------------------
            // Ordinary scalar contour
            // ------------------------------------------------------------

            float scalarContour(float value, float numContours, float lineWidth)
            {
                float scaled = value * numContours;

                float distToLine =
                    abs(fract(scaled + 0.5) - 0.5);

                float aa = max(fwidth(scaled) * lineWidth, 1e-5);

                return 1.0 -
                       smoothstep(0.0, aa, distToLine);
            }


            // ------------------------------------------------------------
            // Directional stripe pattern
            //
            // The stripes are perpendicular to dir.
            //
            // NOTE:
            // This is an approximation. It does NOT integrate the
            // direction field, so strongly curved stress trajectories
            // will not be represented correctly.
            // ------------------------------------------------------------

            float principalStripe(vec2 direction, float density, float width)
            {
                direction = normalize(direction);

                // Pixel coordinates.
                vec2 p = gl_FragCoord.xy;

                // Coordinate perpendicular to the principal direction.
                vec2 normal = vec2(-direction.y, direction.x);

                float coordinate = dot(p, normal);

                float phase = coordinate * density / 100.0;

                float distToLine =
                    abs(fract(phase + 0.5) - 0.5);

                float aa = max(fwidth(phase), 1e-5);

                return 1.0 -
                       smoothstep(
                           width * aa,
                           (width + 1.0) * aa,
                           distToLine
                       );
            }


            // ------------------------------------------------------------
            // Stress information
            // ------------------------------------------------------------

            void getStressAtFragment(out float sigma1, out float sigma2, out vec2 dir1,
                out vec2 dir2, out float angle)
            {
                sigma1 = v_sigma1;
                sigma2 = v_sigma2;

                dir1 = normalize(v_direction1);
                dir2 = normalize(v_direction2);

                angle = v_principalAngle;
            }


            // ------------------------------------------------------------
            // Main
            // ------------------------------------------------------------

            void main()
            {

                float sigma1;
                float sigma2;
                vec2 dir1;
                vec2 dir2;
                float angle;

                getStressAtFragment(
                    sigma1,
                    sigma2,
                    dir1,
                    dir2,
                    angle
                );


                // --------------------------------------------------------
                // Heatmap value
                //
                // Replace this with whatever stress normalization you use.
                // --------------------------------------------------------

                float stressValue = clamp(abs(sigma1), 0.0, 1.0);

                vec3 heatColor = jetHeatmap(stressValue);


                // --------------------------------------------------------
                // Principal stress lines
                // --------------------------------------------------------

                float line1 = principalStripe(dir1, uStressLineDensity, uLineWidth);

                float line2 = principalStripe(dir2, uStressLineDensity, uLineWidth);

                // Choose either family:
                float principalLines = line1;

                // Or both families:
                // float principalLines = max(line1, line2);


                // --------------------------------------------------------
                // Suppress lines when stress magnitude is very small
                // --------------------------------------------------------

                float magnitude = max(abs(sigma1), abs(sigma2));

                float stressMask =
                    smoothstep(0.0, uMinContourValue, magnitude);

                principalLines *= stressMask;


                // --------------------------------------------------------
                // Blend lines over heatmap
                // --------------------------------------------------------

                vec3 finalColor = mix(
                    heatColor,
                    uLineColor,
                    principalLines * uLineOpacity
                );


                float alpha = vertexTransparency;

                fColor = vec4(finalColor, alpha);
            }



                    ";

        }



        #endregion



        #region "Result Plane stress line Type 2 shader"


        private static string rslt_pslinetype2_mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
                    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in float colorOption;
            
            out vec4 vColor;            
                  
            void main()
            {

                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                
                if (colorOption == 0.0)
                    vColor = vec4(1.0, 0.0, 0.0, 1.0); // Red for tension
                else if (colorOption == 1.0)
                    vColor = vec4(0.0, 0.0, 1.0, 1.0); // Blue for compression
                else
                    vColor = vec4(1.0, 1.0, 1.0, 1.0); // Default color


            }


                    ";

        }


        private static string rslt_pslinetype2_mesh_frag_shader()
        {

            return @"
            #version 330 core

            in vec4 vColor;
            in float vStreamFuncValue;  

            out vec4 fColor;
            

            void main()
            {
                // Simple color output without lighting
                fColor = vColor;

            }

        ";
        }


        #endregion




        #region "Result Plane stress line Type 2 Stream Function shader"


        private static string rslt_pslinetype2_streamfunction_mesh_vert_shader()
        {
            return @"

            #version 330 core

            // Pre-computed MVP matrix on CPU for better performance
            uniform mat4 uMVP;           // Model-View-Projection matrix
                    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in float aStreamFuncValueTension; // Stream function value for tension varies between 0 and 1
            layout(location = 2) in float aStreamFuncValueCompression; // Stream function value for compression varies between 0 and 1
            
            out float vStreamFuncValueTension;
            out float vStreamFuncValueCompression;

            void main()
            {

                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);

                vStreamFuncValueTension = aStreamFuncValueTension;
                vStreamFuncValueCompression = aStreamFuncValueCompression;
                
            }


                    ";

        }


        private static string rslt_pslinetype2_streamfunction_mesh_frag_shader()
        {

            return @"
            
           #version 330 core

            uniform float uNumContours = 10.0;      // number of contour bands
            uniform float uLineWidth = 1.0;         // contour line thickness (in pixels, roughly)
            uniform float uLineOpacity = 1.0;       // how strongly lines blend over the heatmap
            uniform float uMinContourValue = 0.01;  // minimum value for contour lines (smooth falloff)
            uniform vec3 uBackgroundColor = vec3(0.1, 0.1, 0.1); // dark background

            in float vStreamFuncValueTension;
            in float vStreamFuncValueCompression;

            out vec4 fColor;

            // Returns 1.0 exactly on a contour line, fading to 0.0 away from it
            float contourLines(float value, float numContours, float lineWidth)
            {
                // Clamp value to valid range
                value = clamp(value, 0.0, 1.0);
    
                float scaled = value * numContours;
                float distToLine = abs(fract(scaled + 0.5) - 0.5); // distance to nearest integer
    
                // Calculate anti-aliased line width
                float aa = fwidth(scaled) * lineWidth;
    
                // Return 1.0 on the line, 0.0 away from it
                return 1.0 - smoothstep(0.0, aa, distToLine);
            }

            // Enhanced contour with smooth falloff near min/max values
            float contourLinesWithFalloff(float value, float numContours, float lineWidth, float minValue)
            {
                // Clamp value to valid range
                value = clamp(value, 0.0, 1.0);
    
                // Skip contour lines near the edges (smooth falloff)
                float edgeFalloff = 1.0;
                if (value < minValue || value > 1.0 - minValue)
                {
                    float distToEdge = min(value, 1.0 - value);
                    edgeFalloff = smoothstep(0.0, minValue, distToEdge);
                }
    
                // Calculate contour line intensity
                float scaled = value * numContours;
                float distToLine = abs(fract(scaled + 0.5) - 0.5);
                float aa = fwidth(scaled) * lineWidth;
                float lineIntensity = 1.0 - smoothstep(0.0, aa, distToLine);
    
                return lineIntensity * edgeFalloff;
            }

            void main()
            {
                // Calculate contour intensities
                // float tensionLine = contourLines(vStreamFuncValueTension, uNumContours, uLineWidth);
                // float compressionLine = contourLines(vStreamFuncValueCompression, uNumContours, uLineWidth);
    
                // Apply minimum value falloff if desired
                float tensionLine = contourLinesWithFalloff(vStreamFuncValueTension, uNumContours, uLineWidth, uMinContourValue);
                float compressionLine = contourLinesWithFalloff(vStreamFuncValueCompression, uNumContours, uLineWidth, uMinContourValue);
    
                // Define colors
                vec3 tensionColor = vec3(1.0, 0.0, 0.0);   // Red for tension
                vec3 compressionColor = vec3(0.0, 0.0, 1.0); // Blue for compression
    
                // Mix colors: if both lines overlap, blend them (magenta)
                vec3 lineColor = vec3(0.0);
                float maxIntensity = 0.0;
    
                if (tensionLine > 0.0 && compressionLine > 0.0)
                {
                    // Both lines overlap - blend to magenta
                    lineColor = mix(tensionColor, compressionColor, 0.5);
                    maxIntensity = max(tensionLine, compressionLine);
                }
                else if (tensionLine > 0.0)
                {
                    lineColor = tensionColor;
                    maxIntensity = tensionLine;
                }
                else if (compressionLine > 0.0)
                {
                    lineColor = compressionColor;
                    maxIntensity = compressionLine;
                }
                else
                {
                    // No contour lines - transparent background
                    fColor = vec4(0.0, 0.0, 0.0, 0.0);
                    return;
                }
    
                // Apply opacity
                float finalAlpha = maxIntensity * uLineOpacity;
    
                // Optional: Add glow effect to make lines more visible
                float glow = pow(maxIntensity, 0.5) * 0.3;
                lineColor = mix(lineColor, vec3(1.0), glow);
    
                // Output final color
                fColor = vec4(lineColor, finalAlpha);
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


        #region "Constraint Shader"

        public static string constraint_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform float zoomscale = 1.0f;

            uniform vec4 vertexColor;

            layout(location = 0) in vec2 position;
            layout(location = 1) in vec2 origin;
            layout(location = 2) in vec2 textureCoord;
            layout(location = 3) in float textureType;


            flat out uint v_textureType;
            out vec2 v_textureCoord;
            out vec4 v_textureColor;

            void main()
            {

	            // apply Translation to the final position 
	            vec4 finalPosition = uMVP * vec4(position, 0.0f, 1.0f);

	            // apply Translation to the text origin
	            vec4 finalTextorigin = uMVP * vec4(origin, 0.0f, 1.0f);
    

	            // Remove the zoom scale
	            vec2 scaled_pt = vec2(finalPosition.x - finalTextorigin.x, finalPosition.y - finalTextorigin.y) / zoomscale;
		
	            // Set the final position of the vertex
	            gl_Position = vec4(scaled_pt.x + finalTextorigin.x, scaled_pt.y + finalTextorigin.y, 0.0f, 1.0f);


	            // update the texture type
	            v_textureType = uint(textureType);
	            v_textureCoord = textureCoord;
	            v_textureColor = vertexColor;

            }

                    ";

        }


        public static string constraint_frag_shader()
        {
            return @"

            #version 330 core
            // uniform float transparency;
            uniform sampler2D u_TexturePin;    // Pin support texture
            uniform sampler2D u_TextureRoller; // Roller support texture

            flat in uint v_textureType;  // 0 = Pin, 1 = Roller
            in vec2 v_textureCoord;
            in vec4 v_textureColor;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)

            void main()
            {
                vec4 texColor;
        
                // Select which texture to sample based on v_textureType
                if (v_textureType == 0u)
                    texColor = texture(u_TexturePin, v_textureCoord);
                else
                    texColor = texture(u_TextureRoller, v_textureCoord);
        
                f_Color = v_textureColor * texColor;
            }

                    ";

        }



        #endregion


        #region "Load Shader"

        public static string load_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;
            uniform vec4 vertexColor;
            uniform float zoomscale = 1.0f;
    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aOrigin;
    
            out vec4 vColor;
    
            void main()
            {
                // Transform to clip space
                vec4 clipPos = uMVP * vec4(aPosition, 0.0, 1.0);
                vec4 clipOrigin = uMVP * vec4(aOrigin, 0.0, 1.0);
        
                // Calculate NDC coordinates
                vec3 ndcPos = clipPos.xyz / clipPos.w;
                vec3 ndcOrigin = clipOrigin.xyz / clipOrigin.w;
        
                // Scale offset in NDC space
                vec2 scaledOffset = (ndcPos.xy - ndcOrigin.xy) / zoomscale;
        
                // Final position (back to clip space)
                gl_Position = vec4(ndcOrigin.xy + scaledOffset, 0.0, 1.0);
        
                vColor = vertexColor;
            }

                    ";

        }


        public static string load_frag_shader()
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



        #region "Reaction Force Shader"

        public static string reactionForce_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;
            uniform vec4 vertexColor;
            uniform float zoomscale = 1.0f;
    
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aOrigin;
            layout(location = 2) in float IsArrowPt;
    
            out vec4 vColor;
    
            void main()
            {
                // Transform to clip space
                vec4 clipPos = uMVP * vec4(aPosition, 0.0, 1.0);
                vec4 clipOrigin = uMVP * vec4(aOrigin, 0.0, 1.0);
        
                // Calculate NDC coordinates
                vec3 ndcPos = clipPos.xyz / clipPos.w;
                vec3 ndcOrigin = clipOrigin.xyz / clipOrigin.w;
                
                float zscale = zoomscale;

                if (IsArrowPt == 0)
                {
                    zscale = 1.0f; // For non-arrow points, we scale with zoom
                }


                // Scale offset in NDC space
                vec2 scaledOffset = (ndcPos.xy - ndcOrigin.xy) / zscale;
        
                // Final position (back to clip space)
                gl_Position = vec4(ndcOrigin.xy + scaledOffset, 0.0, 1.0);
        
                vColor = vertexColor;
            }

                    ";

        }


        public static string reactionForce_frag_shader()
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


        #region "Contour Bar Shader"

        private static string contourbar_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in float node_value; // Value for the contour level between 0.0 and 1.0

            out float v_node_value;

            void main()
            {
	            // Map the node_value to a color (e.g., from blue to red)
	            v_node_value = node_value;

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string contourbar_frag_shader()
        {
            return @"

            #version 330 core

            in float v_node_value;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)
            
             vec3 jetHeatmap(float value) 
            {
                float t = value; // (value + 1.0) * 0.5;
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            void main()
            {
                vec3 contourColor = vec3(0.0); 
                
                if(v_node_value < 0.0f)
                    contourColor = vec3(0.4f, 0.4f, 0.4f); // Dark gray for negative values
                else if(v_node_value > 1.0f)
                    contourColor = vec3(0.8f, 0.8f, 0.8f); // Light gray for values greater than 1
                else
                    contourColor = jetHeatmap(v_node_value); // Use the heatmap for values between 0 and 1


	            f_Color = vec4(contourColor, 1.0f);
            }

                    ";

        }


        #endregion




        public static string get_vertex_shader(ShaderType type)
        {
            // Returns the vertex shader
            switch (type)
            {
                case ShaderType.MeshShader:
                    return mesh_vert_shader();
                case ShaderType.RsltMeshShader:
                    return rslt_mesh_vert_shader();
                case ShaderType.RsltWireframeShader:
                    return rslt_wireframe_vert_shader();
                case ShaderType.RsltPSLShader:
                    return rslt_psline_mesh_vert_shader();
                case ShaderType.RsltPSLType2Shader:
                    return rslt_pslinetype2_mesh_vert_shader();
                case ShaderType.RsltPSLType2StreamFunctionShader:
                    return rslt_pslinetype2_streamfunction_mesh_vert_shader();
                case ShaderType.SelectionShader:
                    return selrect_vert_shader();
                case ShaderType.ConstraintShader:
                    return constraint_vert_shader();
                case ShaderType.LoadShader:
                    return load_vert_shader();
                case ShaderType.ReactionForceShader:
                    return reactionForce_vert_shader();
                case ShaderType.TextShader:
                    return text_vert_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_vert_shader();
                case ShaderType.ContourBarShader:
                    return contourbar_vert_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        public static string get_fragment_shader(ShaderType type)
        {
            // Returns the fragment shader
            switch (type)
            {
                case ShaderType.MeshShader:
                    return mesh_frag_shader();
                case ShaderType.RsltMeshShader:
                    return rslt_mesh_frag_shader();
                case ShaderType.RsltWireframeShader:
                    return rslt_wireframe_frag_shader();
                case ShaderType.RsltPSLShader:
                    return rslt_psline_mesh_frag_shader();
                case ShaderType.RsltPSLType2Shader:
                    return rslt_pslinetype2_mesh_frag_shader();
                case ShaderType.RsltPSLType2StreamFunctionShader:
                    return rslt_pslinetype2_streamfunction_mesh_frag_shader();
                case ShaderType.SelectionShader:
                    return selrect_frag_shader();
                case ShaderType.ConstraintShader:
                    return constraint_frag_shader();
                case ShaderType.LoadShader:
                    return load_frag_shader();
                case ShaderType.ReactionForceShader:
                    return reactionForce_frag_shader();
                case ShaderType.TextShader:
                    return text_frag_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_frag_shader();
                case ShaderType.ContourBarShader:
                    return contourbar_frag_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        //___________________

    }
}
