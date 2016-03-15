#version 430 core

uniform sampler2D s;

out vec4 color;

void main( void )
{
    color = texelFetch( s, ivec2( gl_FragCoord.xy ), 0 );
}
