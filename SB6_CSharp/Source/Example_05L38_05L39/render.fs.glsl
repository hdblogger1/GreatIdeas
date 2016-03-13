#version 430 core

layout (binding = 0) uniform sampler2D tex_object;

in VS_OUT
{
    vec2 tc;
} fs_in;

out vec4 color;

void main(void)
{
	//color = vec4(0.0, 0.8, 1.0, 1.0);
    color = texture(tex_object, fs_in.tc * vec2(3.0, 1.0));
}
