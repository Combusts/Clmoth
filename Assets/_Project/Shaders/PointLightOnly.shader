Shader "Custom/PointLightOnly"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlobalLight ("Global Light", Range(0,1)) = 0.4
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 1.0
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.3
        _FlickerPattern ("Flicker Pattern", Range(0, 1)) = 0.5
        _LightIntensityMultiplier ("Light Intensity Multiplier", Range(0, 50)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _GlobalLight;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _FlickerPattern;
            float _LightIntensityMultiplier;
            
            // 光源参数数组（最多8个）
            #define MAX_LIGHTS 8
            float4 _LightPosArray[MAX_LIGHTS];   // xyz位置
            float4 _LightColorArray[MAX_LIGHTS]; // rgb颜色, a = 强度
            float _LightRadiusArray[MAX_LIGHTS]; // 半径
            int _LightCount;                     // 光源数量

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算闪烁效果
                // 使用不同的波形实现不同的闪烁模式
                float flicker = 1.0;
                float time = _Time.y * _FlickerSpeed;
                
                // _FlickerPattern控制闪烁模式
                // 0-0.33: 正弦波（平滑闪烁）
                // 0.33-0.66: 快速闪烁
                // 0.66-1.0: 不规则闪烁
                if (_FlickerPattern < 0.33)
                {
                    flicker = (sin(time) + 1.0) * 0.5;
                }
                else if (_FlickerPattern < 0.66)
                {
                    flicker = step(0.5, sin(time * 5.0));
                }
                else
                {
                    flicker = (sin(time * 3.0) * sin(time * 7.0) + 1.0) * 0.5;
                }
                
                // 应用到闪烁强度
                flicker = lerp(1.0, flicker, _FlickerIntensity);

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb *= _GlobalLight; // 环境光

                // 先计算光照强度贡献，累积多个光源
                float lightIntensity = 0.0;
                float3 totalLightColor = float3(0, 0, 0);
                
                for (int idx = 0; idx < _LightCount; idx++)
                {
                    float3 lightPos = _LightPosArray[idx].xyz;
                    float3 lightColor = _LightColorArray[idx].rgb;
                    float intensity = _LightColorArray[idx].a;
                    float radius = _LightRadiusArray[idx];

                    float dist = distance(i.worldPos.xy, lightPos.xy);
                    
                    // 使用平方衰减让过渡更平滑
                    float normalizedDist = dist / radius;
                    // 使用 smoothstep 让边缘更柔和，平缓过渡到0
                    float atten = 1.0 - smoothstep(0.0, 1.0, normalizedDist);
                    // 平方衰减，让中心更亮，边缘更快衰减
                    atten *= atten;

                    // 累积光照强度（根据光源强度和闪烁）
                    lightIntensity += atten * intensity * flicker;
                    
                    // 累积光源颜色贡献
                    totalLightColor += lightColor * atten * flicker;
                }

                // 1. 先改变光照强度（乘法操作，让像素更亮）
                col.rgb *= (1 + lightIntensity*_LightIntensityMultiplier);

                // 2. 颜色偏移效果：让物体颜色朝光源色方向偏移
                col.rgb = lerp(col.rgb, col.rgb * (totalLightColor * 0.5 + 0.5), lightIntensity * 0.5);

                return col;
            }
            ENDCG
        }
    }
}

