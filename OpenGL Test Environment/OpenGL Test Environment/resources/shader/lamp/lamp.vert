#version 400
layout(location = 0) in vec3 vertexPosition_modelspace;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;


void main(){

    // Output position of the vertex, in clip space : MVP * position
    gl_Position =  projectionMatrix * viewMatrix * modelMatrix * vec4(vertexPosition_modelspace,1);

}