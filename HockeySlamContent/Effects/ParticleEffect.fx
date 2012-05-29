float4x4 View;
float4x4 Projection;
float2 ViewportScale;

float CurrentTime;

float Duration;
float DurationRandomness;
float3 Gravity;
float EndVelocity;
float4 MinColor;
float4 MaxColor;

float2 RotateSpeed;
float2 StartSize;
float2 EndSize;

texture Texture;

sampler Sampler = sampler_state
{
	Texture = (Texture);

	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Point;

	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float2 Corner : POSITION0;
    float4 Position : POSITION1;
	float3 Velocity : NORMAL0;
	float4 Random : COLOR0;
	float Time : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : COLOR1;
};

float4 ComputeParticlePosition(float3 position, float3 velocity, float age, float normalizedAge)
{
	float startVelocity = length(velocity);

	float endVelocity = startVelocity * EndVelocity;

	// Our particles have constant acceleration, so given a starting velocity
    // S and ending velocity E, at time T their velocity should be S + (E-S)*T.
    // The particle position is the sum of this velocity over the range 0 to T.
    // To compute the position directly, we must integrate the velocity
    // equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

	float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge/2;

	position += normalize(velocity)*velocityIntegral*Duration;

	position += Gravity * age * normalizedAge;

	return mul(mul(float4(position,1), View), Projection);
}

float ComputeParticleSize(float randomValue, float normalizedAge)
{
	float startSize = lerp(StartSize.x, StartSize.y, randomValue);
	float endSize = lerp(EndSize.x, EndSize.y, randomValue);

	float size = lerp(startSize, endSize, normalizedAge);

	return size*Projection._m11;
}

float4 ComputeParticleColor(float4 projectedPosition, float randomValue, float normalizedAge)
{
	float4 color = lerp(MinColor, MaxColor, randomValue);

	// Fade the alpha based on the age of the particle. This curve is hard coded
    // to make the particle fade in fairly quickly, then fade out more slowly:
    // plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
    // this looks like. The 6.7 scaling factor normalizes the curve so the alpha
    // will reach all the way up to fully solid.

	color.a *= normalizedAge*(1-normalizedAge)*(1-normalizedAge)*6.7;

	return color;
}

float2x2 ComputeParticleRotation(float randomValue, float age)
{
	float rotateSpeed = lerp(RotateSpeed.x, RotateSpeed.y, randomValue);

	float rotation = rotateSpeed * age;

	float c = cos(rotation);
	float s = sin(rotation);

	return float2x2(c, -s, s, c);
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float age = CurrentTime - input.Time;

	age *= 1 + input.Random.x * DurationRandomness;

	float normalizedAge = saturate(age / Duration);

	output.Position = ComputeParticlePosition(input.Position, input.Velocity, age, normalizedAge);

	float size = ComputeParticleSize(input.Random.y, normalizedAge);
	float2x2 rotation = ComputeParticleRotation(input.Random.w, age);

	output.Position.xy += mul(input.Corner, rotation)*size*ViewportScale;

	output.Color = ComputeParticleColor(output.Position, input.Random.z, normalizedAge);
	output.TextureCoordinate = (input.Corner + 1)/2;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
