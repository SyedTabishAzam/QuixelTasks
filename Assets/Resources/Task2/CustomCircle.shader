Shader "Unlit/CustomCircle"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "red" {}
		
	//Color for Circle
		_SCL("Scnd Color", Color) = (0.2,1,1,1)


		//Properties for line width, radius and screen size	
		_LineWidth("LineWidth", float) = 10
		_Radius("Radius", float) = 100
		_ScreenHeight("ScreenHeight", int) = 500
		_ScreenWidth("ScreenWidth", int) = 500
	}

	SubShader
	{
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag Lambert alpha
		
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			
			
			fixed4 _SCL;
			float _LineWidth;
			float _Radius;
			int _ScreenHeight;
			int _ScreenWidth;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			float4 _MainTex_ST;
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex,i.uv);
				float x = i.uv.x;
				float y = i.uv.y;
				
			
				//since we know r^2 = x^2 + y^2, we calculate r at every point
				//translation of coordinates is also done here (converting (1,1) to (0,0))

				float pointVal = sqrt(pow((-1 * (x - 0.5))*_ScreenWidth, 2) + pow((-1 * (y - 0.5))*_ScreenHeight, 2));

				//This function works but only to check if point is equal to radius
				//c = int(1 - (sqrt(pow(_Radius - pointVal, 2)))) * _SCL;
				
				//For every point that lies between radius and its width, color it
				c = ((0 < pointVal- _Radius) && (pointVal- _Radius < _LineWidth))*_SCL;
				
			
				return c;
			}
		ENDCG
		}
	}
}
