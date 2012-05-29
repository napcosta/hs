float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;
float4x4 ReflectedView;

texture ReflectionMap;
texture IceSurfaceTexture;
texture IceSurfaceSnow;

float3 BaseColor = float3(0.2, 0.2, 0.8);
float BaseColorAmount = 0.3;

int blurType;
float blurAmount;
float iceTransparency;

float3 LightDirection = float3(1,1,1);

sampler2D reflectionSampler = sampler_state {
	texture = <ReflectionMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D iceSurfaceTextureSampler = sampler_state {
	texture = <IceSurfaceTexture>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler2D iceSurfaceSnowSampler = sampler_state {
	texture = <IceSurfaceSnow>;
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
	float2 UV: TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 ReflectionPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);

	float4x4 rwvp = mul(World, mul(ReflectedView, Projection));
	output.ReflectionPosition = mul(input.Position, rwvp);

	output.UV = input.UV;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 reflectionUV = postProjToScreen(input.ReflectionPosition) + halfPixel();
	float4 reflection = float4(0.0, 0.0, 0.0, 0.0);

	if (blurType == 0) {

		reflection = tex2D(reflectionSampler, reflectionUV);
		reflection += tex2D(reflectionSampler, reflectionUV+blurAmount);
		reflection += tex2D(reflectionSampler, reflectionUV-blurAmount);
		float2 reflectionUVx = reflectionUV;
		reflectionUVx.x += blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVx);
		reflectionUVx.x -= 2*blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVx);
		float2 reflectionUVy = reflectionUV;
		reflectionUVy.y += blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVy);
		reflectionUVy.y -= 2*blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVy);
		reflectionUVy.y += 2*blurAmount;
		reflectionUVy.x -= blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVy);
		reflectionUVx.x += 2*blurAmount;
		reflectionUVx.y -= blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVx);
		reflection = reflection/9;

	} else if (blurType == 1) {

		reflection = 4*tex2D(reflectionSampler, reflectionUV);
		reflection += tex2D(reflectionSampler, reflectionUV+blurAmount);
		reflection += tex2D(reflectionSampler, reflectionUV-blurAmount);
		float2 reflectionUVx = reflectionUV;
		reflectionUVx.x += blurAmount;
		reflection += 2*tex2D(reflectionSampler, reflectionUVx);
		reflectionUVx.x -= 2*blurAmount;
		reflection += 2*tex2D(reflectionSampler, reflectionUVx);
		float2 reflectionUVy = reflectionUV;
		reflectionUVy.y += blurAmount;
		reflection += 2*tex2D(reflectionSampler, reflectionUVy);
		reflectionUVy.y -= 2*blurAmount;
		reflection += 2*tex2D(reflectionSampler, reflectionUVy);
		reflectionUVy.y += 2*blurAmount;
		reflectionUVy.x -= blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVy);
		reflectionUVx.x += 2*blurAmount;
		reflectionUVx.y -= blurAmount;
		reflection += tex2D(reflectionSampler, reflectionUVx);
		reflection = reflection/14;

	}

	float4 snow = tex2D(iceSurfaceSnowSampler, input.UV);
	float4 ice = tex2D(iceSurfaceTextureSampler, input.UV);
	reflection = reflection*(1-iceTransparency) + ice*iceTransparency;
	reflection = float4(0.3,0.3,0.9,1.0)*0.2 + reflection*0.8;

	//reflection = snow*snow.a + reflection*(1-snow.a);
	
	return reflection;
}

technique Technique1
{
	pass Pass1
	{
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
