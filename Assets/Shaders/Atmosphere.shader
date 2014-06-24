Shader "Custom/Atmosphere" {
	SubShader {
		Pass {
			CGPROGRAM

	        #pragma vertex vert
	        #pragma fragment frag
	        
	        #include "UnityCG.cginc"
	        
	        uniform float3 _SunlightPos;
	        
	        struct v2f 
	        {
	        	float4 pos : SV_POSITION;
	        	float2 col : TEXCOORD1;
	        	float2 uv : TEXCOORD0;
	        };
	        
	        v2f vert(appdata_full v) 
	        {
	            v2f o;
	            o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	            float3 viewDir = _WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz;
	            
	            o.col = float2(pow(dot(normalize(viewDir), v.normal), 8.0), 0.0);
	            o.uv = v.texcoord - 0.5;
	            
	            return o;
	        }

	        fixed4 frag(v2f v) : COLOR 
	        {
	            return v.col.xxxx;
	        }
	        
	        ENDCG
        }
	}
}
