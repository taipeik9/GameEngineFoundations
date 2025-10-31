#version 330 core

in vec3 FragPos;
in vec2 TexCoord;

out vec4 FragColor;

uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform vec3 viewPos;
uniform sampler2D texture0;
uniform float lightModifier;

void main()
{
    // Calculating the normal within the shader
    vec3 normal = normalize(cross(dFdx(FragPos), dFdy(FragPos)));

    // Ambient
    float ambientStrength = 0.1 * lightModifier;
    vec3 ambient = ambientStrength * lightColor;
 
    // Diffuse
    vec3 norm = normalize(normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;
 
    // Specular
    float specularStrength = 0.5 * lightModifier;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;


    // sample texture
    vec4 texColor = texture(texture0, TexCoord);

    // if no texture, fallback to objectColor
    if (texColor.a == 0.0)
        texColor = vec4(objectColor, 1.0);

    // Combine results
    vec3 lighting = (ambient + diffuse + specular);
    FragColor = vec4(lighting, 1.0) * texColor;
}
