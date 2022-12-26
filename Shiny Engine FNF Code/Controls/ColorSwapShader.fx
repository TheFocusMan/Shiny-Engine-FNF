sampler2D input : register(S0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 color;
	color = tex2D(input, uv);
	return color;
}

