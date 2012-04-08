float4x4 World;
float4x4 View;
float4x4 Projection;

// TODO: add effect parameters here.
float3 AmbientLightColor;
float3 DiffuseColor;
float3 LightDirection;
float3 DiffuseLightColor;

struct VertexShaderInput
{
    float4 Position	: POSITION0;
    float3 Normal	: NORMAL;
    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position	: POSITION0;
    float3 Normal	: TEXCOORD0;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Normal = mul(input.Normal, World);
    // TODO: add your vertex shader code here.

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.
    
    // Normalize the interpolated normal
    float3 normal = normalize(input.Normal);

    //Store the final color of the pixel
    float3 finalColor = float3(0, 0, 0);

    // Start with ambient light color
    float3 diffuse = AmbientLightColor;

    // Calculate diffuse lightning
    float NdotL = saturate(dot(normal, LightDirection));
    diffuse += NdotL*DiffuseLightColor;

    // Add in diffuse color value
    finalColor += DiffuseColor*diffuse;

			//finalColor += AmbientLightColor*DiffuseColor;
    return float4(finalColor, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
