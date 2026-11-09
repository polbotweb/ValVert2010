// Val Vert: equirectangular sky that cross-fades a clear-day panorama and an overcast cloud deck (gradual cloud cover).
Shader "ValVert/SkyBlend" {
Properties {
    _TexA ("Clear panorama", 2D) = "grey" {}
    _TexB ("Overcast panorama", 2D) = "grey" {}
    _Blend ("Overcast blend", Range(0,1)) = 0
    _Tint ("Tint", Color) = (.5,.5,.5,1)
    [Gamma] _Exposure ("Exposure", Range(0,8)) = 1
    _Rotation ("Rotation", Range(0,360)) = 0
}
SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off
    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        sampler2D _TexA, _TexB; half _Blend, _Exposure, _Rotation; half4 _Tint;
        struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };
        v2f vert (float4 v : POSITION) {
            v2f o; float a = _Rotation * UNITY_PI / 180.0; float s = sin(a), c = cos(a);
            float3 p = float3(c * v.x - s * v.z, v.y, s * v.x + c * v.z);
            o.pos = UnityObjectToClipPos(float4(p, 1)); o.dir = v.xyz; return o; }
        half4 frag (v2f i) : SV_Target {
            float3 d = normalize(i.dir);
            float2 uv = float2(0.5 + atan2(d.x, d.z) / (2 * UNITY_PI), 0.5 + asin(clamp(d.y, -1, 1)) / UNITY_PI);
            half3 a = tex2Dlod(_TexA, float4(uv, 0, 0)).rgb; half3 b = tex2Dlod(_TexB, float4(uv, 0, 0)).rgb;
            half3 col = lerp(a, b, _Blend) * _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;
            return half4(col, 1); }
        ENDCG
    }
}
Fallback Off
}
