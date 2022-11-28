// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Ice_Tunnel_Standard"
{
	Properties
	{
		_BaseColorMap("Base Map", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "white" {}
		_MaskMap("MaskMap", 2D) = "white" {}
		_EmissiveColorMap("Emissive Map", 2D) = "white" {}
		_EmissiveIntensity("Emissive Intensity", Range( 0 , 5)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _BaseColorMap;
		uniform float4 _BaseColorMap_ST;
		uniform sampler2D _EmissiveColorMap;
		uniform float4 _EmissiveColorMap_ST;
		uniform float _EmissiveIntensity;
		uniform sampler2D _MaskMap;
		uniform float4 _MaskMap_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_BaseColorMap = i.uv_texcoord * _BaseColorMap_ST.xy + _BaseColorMap_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseColorMap, uv_BaseColorMap );
			o.Albedo = tex2DNode1.rgb;
			float2 uv_EmissiveColorMap = i.uv_texcoord * _EmissiveColorMap_ST.xy + _EmissiveColorMap_ST.zw;
			o.Emission = ( ( tex2DNode1 * tex2D( _EmissiveColorMap, uv_EmissiveColorMap ) ) * _EmissiveIntensity ).rgb;
			float2 uv_MaskMap = i.uv_texcoord * _MaskMap_ST.xy + _MaskMap_ST.zw;
			float4 tex2DNode3 = tex2D( _MaskMap, uv_MaskMap );
			o.Smoothness = tex2DNode3.a;
			o.Occlusion = tex2DNode3.g;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
-1913;402;1632;985;1484.408;666.4058;1.675373;True;True
Node;AmplifyShaderEditor.SamplerNode;1;-708.9825,-239.1857;Inherit;True;Property;_BaseColorMap;Base Map;0;0;Create;False;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-792.7513,177.9822;Inherit;True;Property;_EmissiveColorMap;Emissive Map;3;0;Create;False;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-368.8818,-76.6745;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-402.3892,110.9673;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;4;0;Create;True;0;0;False;0;1;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-764.2697,-26.41325;Inherit;True;Property;_NormalMap;NormalMap;1;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-556.5234,449.3926;Inherit;True;Property;_MaskMap;MaskMap;2;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-1114.423,183.0082;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-139.3555,-61.59615;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;445.5969,-8.376863;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Ice_Tunnel_Standard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;1;0
WireConnection;6;1;4;0
WireConnection;9;0;6;0
WireConnection;9;1;8;0
WireConnection;0;0;1;0
WireConnection;0;1;2;0
WireConnection;0;2;9;0
WireConnection;0;4;3;4
WireConnection;0;5;3;2
ASEEND*/
//CHKSM=257179545DD54F7FB97E505766821FB0D4A304BB