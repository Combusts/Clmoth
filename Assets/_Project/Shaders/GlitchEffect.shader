Shader "Custom/GlitchEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Glitch Settings)]
        _Intensity ("Glitch Intensity", Range(0, 0.1)) = 0.02
        _BlockSize ("Block Size", Vector) = (10, 10, 0, 0)
        _Speed ("Speed", Range(0, 10)) = 1.0
        [Header(Color Shift)]
        _ColorShiftIntensity ("Color Shift", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
        }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            float _Intensity;
            float2 _BlockSize;
            float _Speed;
            float _ColorShiftIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // 简单的伪随机函数
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 创建不规则区块
                float2 blockUV = floor(uv * _BlockSize);
                float noise = random(blockUV + _Time.y * _Speed *0.0001);
                
                // 只在某些块中应用glitch效果
                float glitchFactor = step(0.7, noise);
                
                // 计算偏移量
                float2 offset = float2(
                    (noise - 0.5) * 2.0 * _Intensity * glitchFactor,
                    (random(blockUV + float2(1.0, 0.0)) - 0.5) * 2.0 * _Intensity * glitchFactor
                );
                
                // 对RGB通道分别偏移，营造chromatic aberration效果
                float r = tex2D(_MainTex, uv + offset + float2(_ColorShiftIntensity, 0)).r;
                float g = tex2D(_MainTex, uv + offset).g;
                float b = tex2D(_MainTex, uv + offset - float2(_ColorShiftIntensity, 0)).b;
                
                fixed4 col = fixed4(r, g, b, 1.0);
                
                // 添加一些颜色噪声
                float colorNoise = random(blockUV + _Time.y * _Speed * 0.5);
                col.rgb += (colorNoise - 0.5) * 0.1 * glitchFactor;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

