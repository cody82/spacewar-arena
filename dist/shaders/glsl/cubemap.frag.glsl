uniform samplerCube EnvironmentMap;

varying vec3 normal;
varying vec3 view;
//vec4 world;
//vec3 worldn;

void main()
{
//world=gl_ModelViewMatrixInverse*view;
//worldn=normalize((gl_ModelViewMatrixInverse*vec4(normal,1.0)).xyz);

	gl_FragColor = textureCube(EnvironmentMap, normalize(reflect(normalize(view),normalize(normal))));
gl_FragColor.a=1.0;
}