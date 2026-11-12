Shader "Prototype/RapiCapture" {
 Properties { _MainTex ("Capture", 2D) = "white" {} _Crop ("Crop", Vector) = (0,0.253,1,0.554) _Front ("Front", Float) = 0 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Cull Back ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   sampler2D _MainTex; float4 _Crop; float _Front;
   struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
   struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
   v2f vert(appdata v) { v2f o; o.pos=TransformObjectToHClip(v.vertex.xyz); o.uv=v.uv; return o; }
   half4 frag(v2f i) : SV_Target {
    float2 uv=_Crop.xy+i.uv*_Crop.zw;
    half4 c=tex2D(_MainTex,uv);
    float blue=distance(c.rgb,float3(.431,.463,.890));
    float floorColor=distance(c.rgb,float3(.14,.145,.14));
    float blueMask=smoothstep(.035,.09,blue);
    // Preserve the character's legs and dark clothing; key only the near-flat floor colour.
    float floorMask=smoothstep(.012,.035,floorColor);
    float whiteMask=1.0-step(.70,min(c.r,min(c.g,c.b)))*step(max(c.r,max(c.g,c.b))-min(c.r,min(c.g,c.b)),.10)*step(i.uv.y,.30)*saturate(step(i.uv.x,.18)+step(.82,i.uv.x));
    float baseEdge=1.0-step(i.uv.y,.30)*saturate(step(i.uv.x,lerp(.22,.18,_Front))+step(lerp(.78,.82,_Front),i.uv.x));
    c.a*=blueMask*floorMask*whiteMask*baseEdge;
    return c;
   }
   ENDHLSL
  }
 }
}


