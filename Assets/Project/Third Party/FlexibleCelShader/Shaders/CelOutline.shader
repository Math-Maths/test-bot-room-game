Shader "FlexibleCelShader/URP Cel Outline"
{
    Properties
    {
        _Color("Global Color Modifier", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _NormalTex("Normal", 2D) = "bump" {}
        _EmmisTex("Emission", 2D) = "black" {}

        _RampLevels("Ramp Levels", Range(2,50)) = 2
        _LightScalar("Light Scalar", Range(0,10)) = 1

        _HighColor("High Light Color", Color) = (1,1,1,1)
        _HighIntensity("High Light Intensity", Range(0,10)) = 1.5
        _LowColor("Low Light Color", Color) = (1,1,1,1)
        _LowIntensity("Low Light Intensity", Range(0,10)) = 1

        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineSize("Outline Size", Float) = 10

        _RimColor("Hard Edge Light Color", Color) = (1,1,1,1)
        _RimAlpha("Hard Edge Light Brightness", Range(0,1)) = 0
        _RimPower("Hard Edge Light Size", Range(0,1)) = 0
        _RimDropOff("Hard Edge Light Dropoff", Range(0,1)) = 0

        _FresnelColor("Soft Edge Light Color", Color) = (1,1,1,1)
        _FresnelBrightness("Soft Edge Light Brightness", Range(0,1)) = 0
        _FresnelPower("Soft Edge Light Size", Range(0,1)) = 0
        _FresnelShadowDropoff("Soft Edge Light Dropoff", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        // =========================
        // MAIN PASS
        // =========================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;          // ✅ Vertex Color
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                float4 color       : TEXCOORD6;     // ✅ Vertex Color
            };

            sampler2D _MainTex;
            sampler2D _NormalTex;
            sampler2D _EmmisTex;

            float4 _MainTex_ST, _NormalTex_ST, _EmmisTex_ST;
            float4 _Color, _HighColor, _LowColor;
            float4 _RimColor, _FresnelColor;

            int _RampLevels;
            float _LightScalar;
            float _HighIntensity, _LowIntensity;

            float _RimPower, _RimAlpha, _RimDropOff;
            float _FresnelBrightness, _FresnelPower, _FresnelShadowDropoff;

            Varyings vert (Attributes v)
            {
                Varyings o;

                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.worldPos);

                o.normalWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                o.tangentWS = normalize(TransformObjectToWorldDir(v.tangentOS.xyz));
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * v.tangentOS.w;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shadowCoord = TransformWorldToShadowCoord(o.worldPos);

                o.color = v.color; // ✅ Pass vertex color

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                _RampLevels = max(_RampLevels - 1, 1);

                float3 viewDir = normalize(GetCameraPositionWS() - i.worldPos);

                Light light = GetMainLight(i.shadowCoord);
                float3 lightDir = normalize(light.direction);

                float3 normalTS = UnpackNormal(tex2D(_NormalTex, i.uv));
                float3 normalWS =
                    normalize(
                        i.tangentWS * normalTS.x +
                        i.bitangentWS * normalTS.y +
                        i.normalWS * normalTS.z
                    );

                float NdotL = saturate(dot(normalWS, lightDir));
                float intensity = saturate(NdotL * _LightScalar) * light.shadowAttenuation;

                float ramp = round(intensity * _RampLevels);

                float lightMul = lerp(_LowIntensity, _HighIntensity, ramp / _RampLevels);
                float4 rampColor =
                    lerp(_LowColor, _HighColor, ramp / _RampLevels);

                half4 col = tex2D(_MainTex, i.uv);
                col *= lightMul * rampColor * _Color;

                // Fresnel
                float fresnel = 1 - saturate(dot(viewDir, normalWS));
                float softFresnel =
                    pow(fresnel, (1 - _FresnelPower) * 10);

                float rampFresnel =
                    1 - ((1 - ramp / _RampLevels) * (1 - _FresnelShadowDropoff));

                col.rgb +=
                    _FresnelColor.rgb *
                    (_FresnelBrightness * 10 - softFresnel * _FresnelBrightness * 10) *
                    rampFresnel;

                // Hard rim
                float rimMask =
                    step(dot(viewDir, normalWS), _RimPower);

                float rimAlpha =
                    _RimAlpha * (1 - ((1 - ramp / _RampLevels) * (1 - _RimDropOff)));

                col.rgb = lerp(col.rgb, _RimColor.rgb, rimMask * rimAlpha);

                // Emission
                half4 emi = tex2D(_EmmisTex, i.uv);
                float e = max(max(emi.r, emi.g), emi.b);
                col.rgb = lerp(col.rgb, emi.rgb, e);

                // ✅ Vertex color as FINAL tint
                col.rgb *= i.color.rgb;

                return col;
            }
            ENDHLSL
        }

        // =========================
        // OUTLINE PASS (unchanged)
        // =========================
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _OutlineSize;
            float4 _OutlineColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                worldPos += normalWS * (_OutlineSize * 0.001);
                o.positionHCS = TransformWorldToHClip(worldPos);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
