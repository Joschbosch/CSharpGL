#version 400
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec3 vertex_normal;


out vec3 normal;
out vec3 fragPos;
out vec2 texCoords;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(){

    // Output position of the vertex, in clip space : MVP * position
    gl_Position =  projectionMatrix * viewMatrix * modelMatrix * vec4(vertexPosition_modelspace,1);
	normal = mat3(transpose(inverse(modelMatrix))) * vertex_normal; // INVERSION IS EXPENSIVE
	fragPos = vec3(modelMatrix * vec4(vertexPosition_modelspace, 1.0f));
	
    texCoords = vertexUV;
}