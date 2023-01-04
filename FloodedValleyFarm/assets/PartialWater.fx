#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
Texture2D WaterTexture;
Texture2D MaskTexture;
float4 WaterColor;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

sampler2D WaterTextureSampler = sampler_state
{
    Texture = <WaterTexture>;
};

sampler2D MaskTextureSampler = sampler_state
{
    Texture = <MaskTexture>;
};


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 edge = float4(1, 1, 1, 1);

	float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
    if (col.a <= 0.01) // Didn't want to do an if in something like this, the code below wasn't handling it for some reason
        return col;

    float4 wcol = tex2D(WaterTextureSampler, input.TextureCoordinates) * WaterColor;
    float4 mask = tex2D(MaskTextureSampler, input.TextureCoordinates);

    float oldA = col.a;
    col = lerp(col, wcol, mask.r); // Add water color
    col = lerp(col, edge, mask.g); // Add edges
    col.a = oldA;

    return col;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
