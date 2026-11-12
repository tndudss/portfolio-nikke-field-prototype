Shader "Prototype/OccluderSoft" {
 Properties { _MainTex ("Base",2D)="white" {} _Color ("Color", Color)=(1,1,1,1) _Opacity ("Opacity",Range(0,1))=.22 _BlurAmount ("Texture softening",Float)=.004 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" } ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Back
  Pass { HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   sampler2D _MainTex;float4 _MainTex_ST;half4 _Color;float _Opacity;float _BlurAmount;
   struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
   v2f vert(appdata v){v2f o;o.pos=TransformObjectToHClip(v.vertex.xyz);o.uv=(v.uv*_MainTex_ST.xy+_MainTex_ST.zw);return o;}
   half4 frag(v2f i):SV_Target{
    float2 d=float2(_BlurAmount,_BlurAmount);half4 c=tex2D(_MainTex,i.uv)*.28;
    c+=(tex2D(_MainTex,i.uv+float2(d.x,0))+tex2D(_MainTex,i.uv-float2(d.x,0))+tex2D(_MainTex,i.uv+float2(0,d.y))+tex2D(_MainTex,i.uv-float2(0,d.y)))*.18;
    return half4(c.rgb*_Color.rgb,_Opacity);
   }
  ENDHLSL }
 }
}
