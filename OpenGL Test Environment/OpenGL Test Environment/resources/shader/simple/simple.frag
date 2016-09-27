#version 400
struct Material {
    sampler2D diffuse;
    sampler2D specular;
	sampler2D emission;
    float shininess;
}; 
  
struct Light {
    vec4 position;
	vec3 direction;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

	vec3 attenuationParameter;

	float cutOff;
	float outerCutOff;
};


in vec3 normal;
in vec3 fragPos;
in vec2 texCoords;

// Ouput data
out vec4 color;


// Values that stay constant for the whole mesh.

uniform vec3 cameraPosition;
uniform Material material;
uniform Light light;  

void main(){
	vec3 lightDirection = vec3(-1, -1, -1);
	float distance = -1;
	if (light.position.w != 1.0){
	   lightDirection = normalize(light.position.xyz - fragPos);
	   distance = length(light.position.xyz - fragPos);
	} else if (light.position.w == 1.0){
	   lightDirection = normalize(-light.direction.xyz);
	} 
	float theta = dot(lightDirection, normalize(-light.direction)); 

	if (light.position.w < 2 || theta > light.cutOff){
		//Diffuse light
		vec3 norm = normalize(normal);
		float diff = max(dot(norm, lightDirection), 0.0);
		vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, texCoords));

		//specular light
		vec3 viewDir = normalize(cameraPosition - fragPos);
		vec3 reflectDir = reflect(-lightDirection, normal);  
		float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
		vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoords));  

		//ambient light
		vec3 ambientLight = light.ambient * vec3(texture(material.diffuse, texCoords));

		vec3 emission = vec3(texture(material.emission, texCoords));
	
		// spotlight circle soft edges
		float epsilon = (light.cutOff - light.outerCutOff);
		float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
		
		// all together

		vec3 result = vec3 (0,0,0);
		float attenuation =  1.0f / (light.attenuationParameter.x + light.attenuationParameter.y * distance + 
    		light.attenuationParameter.z * (distance * distance));
		if (light.position.w == 0.0){
			result = ambientLight * attenuation + diffuse * attenuation + specular * attenuation ;
		} else if (light.position.w == 1.0){
			result = ambientLight + diffuse  + specular ;
		}else if (light.position.w == 2.0){
			result = ambientLight + (diffuse + specular) * intensity * attenuation ;
		}
		//result = result + emission;
	
		color = vec4 (result, 1.0f);
	} else {
		// some ambient light out of the spot
		//color = vec4(light.ambient * vec3(texture(material.diffuse, texCoords)), 1.0f);
		color = vec4(1,0,0,1);
	}
	
}