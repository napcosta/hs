float fade;

texture playerPosition;
texture trace;

sampler2D playerPositionSampler = sampler_state {
	texture = <playerPosition>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler = sampler_state {
	texture = <trace>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};


float4 PixelShaderFunction(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 lessTrace = tex2D(traceSampler, texCoord) - float4(fade, fade, fade, 0);
	
	if(lessTrace.x < 0)
		lessTrace.x = 0;
	if(lessTrace.y < 0)
		lessTrace.y = 0;
	if(lessTrace.z < 0)
		lessTrace.z = 0;

	float4 updatedTrace = lessTrace + tex2D(playerPositionSampler, texCoord);

	if(updatedTrace.x > 255)
		updatedTrace.x = 255;
	if(updatedTrace.y > 255)
		updatedTrace.y = 255;
	if(updatedTrace.z > 255)
		updatedTrace.z = 255;

	updatedTrace.w = 1;

	return updatedTrace;

}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
