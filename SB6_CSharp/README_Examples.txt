https://github.com/openglsuperbible


+-------------------------------------------------------------------------------------------------+
| SuperBible Example  |   SB6_CSharp Example   |  Description                                     |
+-------------------------------------------------------------------------------------------------+
[CHAPTER 1]
---------------------------------------------------------------------------------------------------
    {NO EXAMPLES GIVEN}
---------------------------------------------------------------------------------------------------
[CHAPTER 2]
---------------------------------------------------------------------------------------------------
'simpleclear          | Listing 2.1            | Simple Clear
  {CSharp Only}       | Listing 2.3
'singlepoint          | Listing 2.3-2.7        | Single Point
singletri*            | Listing 2.8-2.9        | Single Triangle
---------------------------------------------------------------------------------------------------
[CHAPTER 3]
---------------------------------------------------------------------------------------------------
'movingtri            | Listing 3.1-3.2        | Moving Triangle
  {CSharp Only}       | Listing 3.3-3.4
  {CSharp Only}       | Listing 3.5-3.6
'tessellatedtri       | Listing 3.7-3.8
'tessellatedgstri     | Listing 3.9
  {CSharp Only}       | Listing 3.10
'fragcolorfrompos     | Listing 3.11-3.12
---------------------------------------------------------------------------------------------------
[CHAPTER 4]
---------------------------------------------------------------------------------------------------
    {NO EXAMPLES GIVEN}
---------------------------------------------------------------------------------------------------
[CHAPTER 5]
---------------------------------------------------------------------------------------------------
  {CSharp Only}       | Listing 5.1-5.5        | Copying data to buffer objects
  {CSharp Only}       | Listing 5.6-5.7        |
  {CSharp Only}       | Listing 5.8            |
  {CSharp Only}       | Listing 5.9-5.17 (A)   | Shared
  {CSharp Only}       | Listing 5.9-5.17 (B)   | Standard
'spinnycube           | Listing 5.20-5.25,5.26 |
  {CSharp Only}       | Listing 5.27-5.28      |
  {CSharp Only}       | Listing 5.29-5.32      | Atomic Counters
simpletexture         | Listing 5.33-5.35
ktxview               | Listing 5.36-5.37      | Simple KTX viewer
'simpletexcoords      | Listing 5.38-5.39      | Texture Coordinates
alienrain*            | Listing 5.40 - 5.43    | Texture Arrays


'fragmentlist         | Listing 5.45-5.46
---------------------------------------------------------------------------------------------------
[CHAPTER 6]
---------------------------------------------------------------------------------------------------
'programinfo          | Listing 6.4
subroutines           | Listing 6.6: Setting values of subroutine uniforms
---------------------------------------------------------------------------------------------------
[CHAPTER 7]
---------------------------------------------------------------------------------------------------
'msaanative           | Listing 7.2-7.3
indexedcube           | ?Listing 7.2?: Setting up indexed cube geometry
grass                 | Figure 7.7: The final field of grass
instancedattribs      | ?Listing 7.7?: Simple vertex shader with per-vertex color
multidrawindirect     | ?Listing 7.10?: Example use of an indirect draw command
springmass*           | Listing 7.16: Spring-mass system vertex setup
clipdistance          | ?Listing 7.20?: Clipping an object against a plane and a sphere
---------------------------------------------------------------------------------------------------
[CHAPTER 8]
---------------------------------------------------------------------------------------------------
tessmodes*            | Listing 8.1: Simple quad tessellation control shader example
dispmap*              | Listing 8.8 - Vertex shader for terrain rendering(Displacement Mapping)
'tesssubdivmodes      | Figure 8.10
cubicbezier*          | Listing 8.12 - Cubic Bezier patch vertex shader
gsculling*            | Listing 8.20: Custom culling geometry shader
'objectexploder       | Listing 8.23-8.24
gstessellate*         | Listing 8.25: Generating Geometry in the Geometry Shader
normalviewer*         | Listing 8.30: A pass-through vertex shader that includes normals
gsquads*              | Listing 8.34 - 8.35: Rendering quads
multiviewport         | Listing 8.36: Rendering to multiple viewports in a geometry shader
---------------------------------------------------------------------------------------------------
[CHAPTER 9]
---------------------------------------------------------------------------------------------------
multiscissor*         | Listing 9.1: Setting up scissor rectangle arrays
noperspective         | Figure 9.1: Contrasting perspective-correct and linear interpolation
blendmatrix*          | Listing 9.3: Rendering with all blending functions
depthclamp            | ?Figure 9.4? - Depth Clamping
basicfbo*             | Listing 9.5: Rendering to a texture
gslayered*            | Listing 9.8: Setting up a layered framebuffer
polygonsmooth         | Figure 9.10 Antialiasing using polygon smoothing
stereo                | Listing 9.15: Drawing into a stereo window
linesmooth            | Listing 9.18 Turning on line smoothing
sampleshading         | ?Listing 9.22?: Fragment shader producing high-frequency output
hdrtonemap*           | ?Listing 9.24?: Applying simple exposure coefficient to an HDR
hdrexposure           | ?Listing 9.24?: Applying simple exposure coefficient to an HDR
hdrbloom*             | Listing 9.26: Bloom fragment shader; output bright data to a separate buffer
starfield*            | Listing 9.31: Vertex shader for the star field effect
shapedpoints          | Listing 9.33: Fragment shader for generating shaped points
perf-readpixels       | Listing 9.37: Taking a screenshot with glReadPixels()
---------------------------------------------------------------------------------------------------
[CHAPTER 10]
---------------------------------------------------------------------------------------------------
prefixsum             | Listing 10.6: Prefix sum implementation using a compute shader
dof*                  | Listing 10.7 - Depth of field
prefixsum2d           | Listing 10.7: Compute shader to generate a 2D prefix sum
csflocking            | ?Listing 10.9?: Initializing shader storage buffers for flocking
flocking              | ?Listing 10.10?: Flocking algorithm
---------------------------------------------------------------------------------------------------
[CHAPTER 11]
---------------------------------------------------------------------------------------------------
    {NO EXAMPLES GIVEN}
---------------------------------------------------------------------------------------------------
[CHAPTER 12]
---------------------------------------------------------------------------------------------------
phonglighting*        | Listing 12.1: The Gouraud shading vertex shader
multimaterial         | ?Listing 12.3?: The Phong shading vertex shader
blinnphong*           | Listing 12.5: Blinn-Phong fragment shader
rimlight*             | Listing 12.6: Rim lighting shader function
bumpmapping*          | Listing 12.7: Vertex shader for normal mapping
envmapsphere*         | Listing 12.10 - Spherical environment mapping fragment shader
equirectangular*      | Listing 12.11 - Equirectangular Environment Maps
cubemapenv*           | Listing 12.13 - 12.16: Fragment shader for cube map environment rendering
perpixelgloss*        | Listing 12.17: Fragment shader for per-fragment shininess
shadowmapping*        | Listing 12.22: Simplified fragment shader for shadow mapping
toonshading*          | Listing 12.25: The toon vertex shader
noise                 | Figure 12.30: Effect of introducing noise in ambient occlusion
deferredshading*      | Listing 12.31 - Deferred shading with normal mapping
ssao*                 | Listing 12.32: Ambient occlusion fragment shader
julia*                | Listing 12.33: Setting up the Julia set renderer
raytracer*            | Listing 12.38: Ray-plane intersection test
---------------------------------------------------------------------------------------------------
[EXTRAS]
---------------------------------------------------------------------------------------------------
  {CSharp Only}       | BasicAtomicCounters    | Basic atomic counters
sb6mrender            | SBM6ModelRenderer      | Simple render for SBM6 model files (arcball still needs implementing)
tunnel*               | Tunnel                 | Figure 5.9: A tunnel rendered with three textures and mipmapping
wrapmodes             | WrapModes              | Figure 5.10: Example of texture coordinate wrapping modes
tessellatedcube       | ?????
