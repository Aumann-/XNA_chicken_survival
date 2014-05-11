//this shader is for using billboards whose quad have only two properites:
//position and texture.
//In addition an Alpah test boolean is added along with an alpha test value to
//insure proper blending of multiple billboards according to depth and alpha.

float4x4 WVPMatrix :WORLDVIEWPROJ;	//world view proj matrix


uniform extern texture textureImage;// store texture
float AlphaTestDirection = 1.0;
float AlphaTestValue = 0.8f;
bool AlphaTest = true;
// filter (like a brush) for showing texture
sampler textureSampler  = sampler_state
{
	Texture				= <textureImage>;
	
};

// input to vertex shader
struct VSinput
{									 // input to vertex shader
	float4 position		: POSITION0; // position semantic x,y,z,w
	float2 uv			: TEXCOORD0; // texture semantic u,v
};

// vertex shader output
struct VStoPS
{ 
	// vertex shader output
	float4 position		: POSITION0; // position semantic x,y,z,w
	float2 uv			: TEXCOORD0; // texture semantic u,v
};

// pixel shader output
struct PSoutput
{									// pixel shader output
	float4 color		: COLOR0;   // colored pixel is output
};

// alter vertex inputs
void VertexShaderFunction(in VSinput IN, out VStoPS OUT)
{
	OUT.position = mul(IN.position, WVPMatrix); // transform object
	
	// orient it in camera
	
	OUT.uv     = IN.uv;				// send uv's to p.s.
}

// convert color and texture data from vertex shader to pixels
//check for alpha blending for drawing of billboards in two passes
void PixelShaderFunction(in VStoPS IN, out PSoutput OUT)
{
	// use texture for coloring object
	//color*tex2D(textureSampler, input.uv); 
	float4 color = tex2D(textureSampler, IN.uv);
	// Apply the alpha test.
	clip((color.a - AlphaTestValue) * AlphaTestDirection);
	
		OUT.color  = color;
	
}

// the shader starts here
technique TextureShader
{
	pass p0
	{
		// texture sampler initialized
		sampler[0]		= (textureSampler);
		
		// declare and initialize vs and ps
		  VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
	}
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
