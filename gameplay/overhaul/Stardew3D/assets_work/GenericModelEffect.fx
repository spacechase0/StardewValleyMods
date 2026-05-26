#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

matrix World;
matrix WorldViewProj;
sampler2D Texture : register(s0);
float4 Color;

#define LIGHT_COUNT 4

float4 AmbientLightColor = float4(1, 1, 1, 1);
float4 PointLightPositions[LIGHT_COUNT];
float4 PointLightColors[LIGHT_COUNT];

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float4 Color : COLOR0;
    float3 Normal : NORMAL0;
};

struct InstanceInput
{
    float4 MatRow1 : TEXCOORD1;
    float4 MatRow2 : TEXCOORD2;
    float4 MatRow3 : TEXCOORD3;
    float4 MatRow4 : TEXCOORD4;
    float4 Color : COLOR1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 OriginalPosition : TEXCOORD1;
};

VertexShaderOutput MainInstancedVS(VertexShaderInput input, InstanceInput instance)
{
    float4x4 instTransform = float4x4(instance.MatRow1, instance.MatRow2, instance.MatRow3, instance.MatRow4);
    matrix transform = instTransform * World;
    
    VertexShaderOutput ret;
    ret.Position = mul(input.Position, mul(instTransform, WorldViewProj));
    ret.TextureCoordinates = input.TextureCoordinates;
    ret.Color = input.Color * instance.Color;
    ret.Normal = normalize(mul(input.Normal, (float3x3) transform));
    ret.OriginalPosition = (float3) mul(input.Position, transform);
    return ret;
}

VertexShaderOutput MainSingleVS(VertexShaderInput input)
{
    VertexShaderOutput ret;
    ret.Position = mul(input.Position, WorldViewProj);
    ret.TextureCoordinates = input.TextureCoordinates;
    ret.Color = input.Color;
    ret.Normal = normalize(mul(input.Normal, (float3x3) World));
    ret.OriginalPosition = (float3) mul(input.Position, World);
    return ret;
}

float4 MainPS_Common(VertexShaderOutput input)
{
    float4 baseCol = tex2D(Texture, input.TextureCoordinates) * input.Color * Color;

    float4 lighting = AmbientLightColor;
    
    [unroll]
    for (int i = 0; i < LIGHT_COUNT; i += 1)
    {
        float amount = dot(-normalize(input.OriginalPosition - PointLightPositions[i].xyz), input.Normal);
        amount = saturate(amount);

        lighting.r = max(lighting.r, PointLightColors[i].r * amount * PointLightColors[i].a);
        lighting.g = max(lighting.g, PointLightColors[i].g * amount * PointLightColors[i].a);
        lighting.b = max(lighting.b, PointLightColors[i].b * amount * PointLightColors[i].a);
    }

    lighting = AmbientLightColor; // most lights aren't implemented yet, and so we don't actually want anything but ambient lights
    
    return baseCol * lighting;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 ret = MainPS_Common(input);

    clip(ret.a - 0.01);
    
    return ret;
}

// Transparent portions are discarded
float4 MainPS_Transparent_1(VertexShaderOutput input) : COLOR
{
    float4 ret = MainPS_Common(input);

    clip(ret.a - 0.95);
    
    return ret;
}

// Non-transparent portions are discarded - intended to be drawn with depth buffer write disabled
// This is separate from the above because I can't find a way to say "keep the pixel, but don't write to the depth buffer"
float4 MainPS_Transparent_2(VertexShaderOutput input) : COLOR
{
    float4 ret = MainPS_Common(input);
    
    clip(-ret.a + 0.95);
    clip(ret.a - 0.05);
    
    return ret;
}

technique InstancedDrawing
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainInstancedVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};

technique InstancedDrawing_Transparent_1
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainInstancedVS();
        PixelShader = compile ps_3_0 MainPS_Transparent_1();
    }
};

technique InstancedDrawing_Transparent_2
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainInstancedVS();
        PixelShader = compile ps_3_0 MainPS_Transparent_2();
    }
};

technique SingleDrawing
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainSingleVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};

technique SingleDrawing_Transparent_1
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainSingleVS();
        PixelShader = compile ps_3_0 MainPS_Transparent_1();
    }
};


technique SingleDrawing_Transparent_2
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainSingleVS();
        PixelShader = compile ps_3_0 MainPS_Transparent_2();
    }
};
