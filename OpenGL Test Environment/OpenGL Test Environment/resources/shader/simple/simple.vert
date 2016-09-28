#version 400
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec3 vertex_normal;


out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main(){

    // Output position of the vertex, in clip space : MVP * position
    gl_Position =  projectionMatrix * viewMatrix * modelMatrix * vec4(vertexPosition_modelspace,1);
	Normal = mat3(transpose(inverse(modelMatrix))) * vertex_normal; // INVERSION IS EXPENSIVE
	FragPos = vec3(modelMatrix * vec4(vertexPosition_modelspace, 1.0f));
	
    TexCoords = vec2(vertexUV.x, 1-vertexUV.y);
}