float4x4 World;
float4x4 View;
float4x4 Projection;

float viewportWidth;
float viewportHeight;

texture trace0;
texture trace1;
texture trace2;
texture trace3;
texture trace4;
texture trace5;
texture trace6;
texture trace7;
texture trace8;
texture trace9;
texture trace10;
texture trace11;


sampler2D traceSampler0 = sampler_state {
	texture = <trace0>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler1 = sampler_state {
	texture = <trace1>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler2 = sampler_state {
	texture = <trace2>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler3 = sampler_state {
	texture = <trace3>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler4 = sampler_state {
	texture = <trace4>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler5 = sampler_state {
	texture = <trace5>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler6 = sampler_state {
	texture = <trace6>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler7 = sampler_state {
	texture = <trace7>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};
sampler2D traceSampler8 = sampler_state {
	texture = <trace8>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler9 = sampler_state {
	texture = <trace9>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler10 = sampler_state {
	texture = <trace10>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D traceSampler11 = sampler_state {
	texture = <trace11>;
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

    return ((0.08*tex2D(traceSampler11, reflectionUV)) + 
			(0.16*tex2D(traceSampler10, reflectionUV)) +
			(0.24*tex2D(traceSampler9, reflectionUV)) + 
			(0.32*tex2D(traceSampler8, reflectionUV)) + 
			(0.4*tex2D(traceSampler7, reflectionUV)) + 
			(0.48*tex2D(traceSampler6, reflectionUV)) + 
			(0.56*tex2D(traceSampler5, reflectionUV)) + 
			(0.64*tex2D(traceSampler4, reflectionUV)) + 
			(0.72*tex2D(traceSampler3, reflectionUV)) + 
			(0.8*tex2D(traceSampler2, reflectionUV)) + 
			(0.88*tex2D(traceSampler1, reflectionUV)) + 
			tex2D(traceSampler0, reflectionUV));
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
