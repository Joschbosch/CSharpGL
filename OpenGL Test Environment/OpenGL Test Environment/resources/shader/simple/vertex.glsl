#version 400
layout (location = 0) in vec3 vertex_position;
uniform mat4 mvp_matrix;


void main(void)
{
    //ref line 124
    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}