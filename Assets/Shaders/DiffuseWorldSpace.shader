Shader "Custom/WorldSpace" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
    _Scale("Texture Scale", Float) = 1.0
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }

        SubShader{
        Tags{ "RenderType" = "Opaque" }

        CGPROGRAM
    #pragma surface surf Standard fullforwardshadows
    #pragma target 3.0

        sampler2D _MainTex;
    float4 _MainTex_ST;
    fixed4 _Color;
    float _Scale;

    half _Glossiness;
    half _Metallic;

    struct Input {
        float3 worldPos;
        float3 worldNormal;
    };

    void surf(Input IN, inout SurfaceOutputStandard o) {
        float2 uv;
        fixed4 color;

        // RIGHT NOW USES SAME TEXTURE FOR ALL SIDES
        if (abs(IN.worldNormal.x) > 0.5) {
            uv = IN.worldPos.yz; // left right
        } else if (abs(IN.worldNormal.z) > 0.5) {
            uv = IN.worldPos.xy; // front back
        } else {
            uv = IN.worldPos.xz; // top bottom
        }
        uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
        color = tex2D(_MainTex, uv * _Scale);

        //color = fixed4(0,0,0,0);
        //if (abs(IN.worldNormal.x) > 0.1) {
        //    uv = IN.worldPos.yz * _MainTex_ST.xy + _MainTex_ST.zw;
        //    color = tex2D(_MainTex, uv * _Scale);
        //}
        //if (abs(IN.worldNormal.z) > 0.1) {
        //    uv = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zw;
        //    color += tex2D(_MainTex, uv * _Scale);
        //}
        //if (abs(IN.worldNormal.y) > 0.1) {
        //    uv = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zw;
        //    color += tex2D(_MainTex, uv * _Scale);
        //}

        o.Albedo = color.rgb * _Color;

        // Metallic and smoothness come from slider variables
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = color.a;
    }
    ENDCG
    }
        Fallback "Diffuse"
}