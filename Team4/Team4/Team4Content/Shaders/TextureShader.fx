float4x4 World;
float4x4 View;
float4x4 Projection;
//uniform extern 
texture textureImage;

// filter (like a brush) for showing texture
sampler textureSampler  = sampler_state
{
	Texture				= <textureImage>;
	magfilter			= LINEAR;	// magfilter - bigger than actual
	minfilter			= LINEAR;	// minfilter - smaller than actual
	mipfilter			= LINEAR;
};
// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position 	: POSITION0;
    float4 Color	: COLOR0;
    float2 uv		:TEXCOORD0;

};

struct VertexShaderOutput
{
    float4 Position 	: POSITION0;
    float4 Color	: COLOR0;
    float2 uv		:TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;    
    output.uv = input.uv;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input):COLOR0
{  
    // use texture for coloring object
    input.Color  *= tex2D(textureSampler, input.uv);
    return input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.
		sampler[0]		= (textureSampler);
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
