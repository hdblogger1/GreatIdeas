#version 430 core
// Uniform image variables:
// Input image - note use of format qualifier because of loads
layout (binding = 0, rgba32f) readonly uniform image2D image_in;

// Output image
layout (binding = 1) uniform writeonly image2D image_out;

void main( void )
{
	// Use fragment coordinate as image coordinate
	ivec2 P = ivec2( gl_FragCoord.xy );

	// Read from input image
	vec4 data = imageLoad( image_in, P );

	// Write inverted data to output image
	imageStore( image_out, P, vec4( 1.0f - data.r, 1.0f - data.g, 1.0f - data.b, 1.0f ) );
}
