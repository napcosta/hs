float4x4 World;
float4x4 View;
float4x4 Projection;

float viewportWidth;
float viewportHeight;

float fade;

texture playerPosition;

sampler2D playerPositionSampler = sampler_state {
	texture = <playerPosition>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

float2 postProjToScreen(float4 position)
{
	float2 screenPos = position.xy / position.w;
	return 0.5f * (float2(screenPos.x, -screenPos.y) + 1);
}

float2 halfPixel()
{
	return 0.5 / float2(viewportWidth, viewportHeight);
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV: TEXCOORD0;
	float4 ReflectionPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);
	output.ReflectionPosition = output.Position;

	output.UV = input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 reflectionUV = postProjToScreen(input.ReflectionPosition) + halfPixel();

	return tex2D(playerPositionSampler, reflectionUV);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
