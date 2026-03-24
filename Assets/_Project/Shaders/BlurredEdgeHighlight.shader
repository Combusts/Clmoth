Shader "Custom/BlurredEdgeHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Edge Detection)]
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.2
        _EdgeSharpness ("Edge Sharpness", Range(0, 10)) = 5.0
        
        [Header(Blur Settings)]
        _BlurRadius ("Blur Radius", Range(0, 10)) = 2.0
        _BlurIntensity ("Blur Intensity", Range(0, 1)) = 0.5
        
        [Header(Highlight Settings)]
        _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _HighlightIntensity ("Highlight Intensity", Range(0, 5)) = 2.0
        _HighlightWidth ("Highlight Width", Range(0, 1)) = 0.3
        
        [Header(Edge Glow)]
        _GlowColor ("Glow Color", Color) = (0.5, 0.8, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0
        _GlowWidth ("Glow Width", Range(0, 2)) = 0.5
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
            float4 _MainTex_TexelSize;
            float4 _Color;
            
            float _EdgeThreshold;
            float _EdgeSharpness;
            float _BlurRadius;
            float _BlurIntensity;
            float4 _HighlightColor;
            float _HighlightIntensity;
            float _HighlightWidth;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowWidth;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Sobel 边缘检测
            float SampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            float EdgeDetection(float2 uv)
            {
                float2 texel = _MainTex_TexelSize.xy;
                
                // Sobel 算子
                float tl = SampleAlpha(uv + float2(-texel.x, -texel.y));
                float l = SampleAlpha(uv + float2(-texel.x, 0));
                float bl = SampleAlpha(uv + float2(-texel.x, texel.y));
                
                float t = SampleAlpha(uv + float2(0, -texel.y));
                float c = SampleAlpha(uv + float2(0, 0));
                float b = SampleAlpha(uv + float2(0, texel.y));
                
                float tr = SampleAlpha(uv + float2(texel.x, -texel.y));
                float r = SampleAlpha(uv + float2(texel.x, 0));
                float br = SampleAlpha(uv + float2(texel.x, texel.y));
                
                // Sobel Gx 和 Gy
                float gx = -tl + tr - 2.0 * l + 2.0 * r - bl + br;
                float gy = -tl - 2.0 * t - tr + bl + 2.0 * b + br;
                
                float gradient = sqrt(gx * gx + gy * gy);
                
                return gradient;
            }

            // 高斯模糊采样
            float4 BlurSample(float2 uv, float blurRadius)
            {
                float2 texel = _MainTex_TexelSize.xy * blurRadius;
                
                float4 color = float4(0, 0, 0, 0);
                float weight = 0;
                
                // 3x3 采样
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * texel;
                        float w = 1.0 / (1.0 + x * x + y * y); // 简单的权重
                        color += tex2D(_MainTex, uv + offset) * w;
                        weight += w;
                    }
                }
                
                return color / weight;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 原始颜色
                float4 originalColor = tex2D(_MainTex, uv);
                
                // 边缘检测
                float edge = EdgeDetection(uv);
                
                // 将边缘值转换为 0-1 的范围
                float edgeFactor = saturate(edge / _EdgeThreshold);
                edgeFactor = pow(edgeFactor, 1.0 / _EdgeSharpness);
                
                // 模糊效果（仅在边缘区域）
                float4 blurredColor = originalColor;
                if (_BlurIntensity > 0)
                {
                    blurredColor = BlurSample(uv, _BlurRadius * edgeFactor);
                    blurredColor = lerp(originalColor, blurredColor, _BlurIntensity * edgeFactor);
                }
                
                // 高光效果
                float highlight = 0;
                if (_HighlightIntensity > 0)
                {
                    // 创建一个渐变的高光
                    float highlightMask = smoothstep(0, _HighlightWidth, edgeFactor) * 
                                          smoothstep(1, 1 - _HighlightWidth, edgeFactor);
                    highlight = highlightMask * _HighlightIntensity;
                }
                
                // 光晕效果（更强的外发光）
                float glow = 0;
                if (_GlowIntensity > 0)
                {
                    // 向外扩展采样
                    float2 offset1 = float2(_MainTex_TexelSize.x * _GlowWidth, 0);
                    float2 offset2 = float2(0, _MainTex_TexelSize.y * _GlowWidth);
                    float alpha1 = SampleAlpha(uv + offset1);
                    float alpha2 = SampleAlpha(uv - offset1);
                    float alpha3 = SampleAlpha(uv + offset2);
                    float alpha4 = SampleAlpha(uv - offset2);
                    
                    float avgAlpha = (alpha1 + alpha2 + alpha3 + alpha4) * 0.25;
                    float currentAlpha = SampleAlpha(uv);
                    
                    // 在边缘外部采样
                    glow = saturate(avgAlpha - currentAlpha) * _GlowIntensity * edgeFactor;
                }
                
                // 组合最终颜色
                float4 finalColor = blurredColor * _Color;
                
                // 添加高光
                finalColor.rgb += _HighlightColor.rgb * highlight * _HighlightColor.a;
                
                // 添加光晕
                finalColor.rgb += _GlowColor.rgb * glow * _GlowColor.a;
                
                // 边缘增强（可选）
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (1 + edgeFactor * 0.5), 0.3);
                
                return finalColor;
            }
            ENDCG
        }
    }
}

