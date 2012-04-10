float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;
float4x4 ReflectedView;

texture ReflectionMap;

float3 BaseColor = float3(0.2, 0.2, 0.8);
float BaseColorAmount = 0.3;

float3 LightDirection = float3(1,1,1);

sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

float viewportWidth;
float viewportHeight;

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

};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 ReflectionPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);

	float4x4 rwvp = mul(World, mul(ReflectedView, Projection));
	output.ReflectionPosition = mul(input.Position, rwvp);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 reflectionUV = postProjToScreen(input.ReflectionPosition) + halfPixel();
	float3 reflection = tex2D(reflectionSampler, reflectionUV);

	return float4(reflection, 1);
}

technique Technique1
{
	pass Pass1
	{
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
