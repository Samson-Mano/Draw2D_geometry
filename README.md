# VARAI2D
Varai2D is a powerful windows application to create 2D geometry. The main objective of this application is to allow user to create 2D domains which can be easily imported to mesh generation or other FE softwares. 

![Plate with slits](/Images/plate_with_slits.png)

![Dam cross-section](/Images/dam_cross_section.png)

![Circular plate with 3holes](/Images/circular_plate_with_3holes.png)

![Varai2D Menu option](/Images/varai2d_menu_option.PNG)

# Status: In progress

# How to use
The attached how_to_use_varai2d.pdf gives a very detailed information on how to use this software.

# Features
Main graphics features are 
- [x] Zoom (ctrl + scroll)
- [x] Pan (ctrl + right click and drag)
- [x] Scale to fit drawing area (Ctrl + F)

Drawing objects
- [x] AddLine (2 Left click or keyboard input)
- [x] Add Circular arc (3 Left clicks or keyboard input)
- [x] Add Bezier curve (n clicks or keyboard input)

Modify objects
- [x] Delete objects
- [x] Translate objects
- [x] Rotate objects
- [x] Reflect (mirror) objects
- [x] Intersect objects (intersect any 2 objects)
- [x] Split objects (split based on parameter t)
- [x] Undo (Ctrl + Z)
- [x] Redo (Ctrl + R)

Other features
- [x] Create/ Delete surface (automatically identify surfaces based on non-self intersecting closed boundaries)
- [x] Options (Enable user to control the graphics, snapping & other options of the software)

Export option
- [x] Raw text export (*.TXT)
- [x] Save as Picture file (*.PNG)
- [X] Varai Object export (*.2DS)
- [ ] Autocad format (*.DWG, *.DXF)
- [ ] CALS compliant Metafile (*.CGM)
- [ ] Drawing Web Format (*.DWF)
- [ ] Solid Edge 2D (*.DFT)
- [ ] Unigraphics 2D (*.PRT)

# Solution Structure
![Varai2D Solution](/Images/varai2d_solution_structure.png)

# Future work

Integrate the graphics into Modern OpenGL v3.3+ from the current GDI+.
