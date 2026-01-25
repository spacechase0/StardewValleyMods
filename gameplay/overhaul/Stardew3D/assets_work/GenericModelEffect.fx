matrix WorldViewProj;
sampler2D Texture : register(s0);
float4 Color;

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
};

VertexShaderOutput MainInstancedVS(VertexShaderInput input, InstanceInput instance)
{
    float4x4 instTransform = float4x4(instance.MatRow1, instance.MatRow2, instance.MatRow3, instance.MatRow4);
    
    VertexShaderOutput ret;
    ret.Position = mul(input.Position, mul(instTransform, WorldViewProj));
    ret.TextureCoordinates = input.TextureCoordinates;
    ret.Color = input.Color * instance.Color;
    ret.Normal = input.Normal; // TOOD: Should this should be rotated with the instance transform?
    return ret;
}

VertexShaderOutput MainSingleVS(VertexShaderInput input)
{
    VertexShaderOutput ret;
    ret.Position = mul(input.Position, WorldViewProj);
    ret.TextureCoordinates = input.TextureCoordinates;
    ret.Color = input.Color;
    ret.Normal = input.Normal;
    return ret;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 ret = tex2D(Texture, input.TextureCoordinates) * input.Color * Color;

    clip(ret.a - 0.01);
    
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

technique SingleDrawing
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainSingleVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};
