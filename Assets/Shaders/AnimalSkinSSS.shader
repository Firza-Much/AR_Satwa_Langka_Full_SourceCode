Shader "SatwaLangka/AnimalSkinPBR_SSS"
{
    Properties
    {
        [Header(Base Appearance)]
        _BaseColor("Color Tint", Color) = (1,1,1,1)
        _BaseMap("Base Color (Albedo / Palette)", 2D) = "white" {}

        [Header(Surface Detail)]
        [Normal] _BumpMap("Normal Map (Skin Wrinkles)", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0.0, 3.0)) = 1.2

        [Header(Ambient Occlusion)]
        _OcclusionMap("Ambient Occlusion Map", 2D) = "white" {}
        _OcclusionStrength("AO Strength", Range(0.0, 1.0)) = 0.85

        [Header(PBR Roughness and Metallic)]
        _MetallicGlossMap("Metallic (R) / Smoothness (A)", 2D) = "white" {}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness (1 - Roughness)", Range(0.0, 1.0)) = 0.26

        [Header(Subsurface Scattering (Skin SSS))]
        _SSSColor("Subsurface Tint (Flesh / Blood Bleed)", Color) = (0.92, 0.48, 0.36, 1.0)
        _SSSStrength("SSS Intensity", Range(0.0, 2.0)) = 0.70
        _SSSDistortion("SSS Wrap / Distortion", Range(0.1, 0.8)) = 0.35
        _SSSPower("SSS Transmission Power", Range(1.0, 16.0)) = 5.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD3;
                float4 tangentWS    : TEXCOORD4;
                float2 uv           : TEXCOORD0;
                float4 shadowCoord  : TEXCOORD5;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _BumpMap_ST;
                float4 _OcclusionMap_ST;
                float4 _MetallicGlossMap_ST;
                float4 _SSSColor;
                float _BumpScale;
                float _OcclusionStrength;
                float _Metallic;
                float _Smoothness;
                float _SSSStrength;
                float _SSSDistortion;
                float _SSSPower;
            CBUFFER_END

            TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap);        SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_MetallicGlossMap);    SAMPLER(sampler_MetallicGlossMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, input.tangentOS.w);

                output.shadowCoord = GetShadowCoord(vertexInput);
                return output;
            }

            half3 CalculateSSS(Light light, half3 normalWS, half3 viewDirWS, half3 sssColor, half sssStrength, half distortion, half power)
            {
                // Wrap Lighting (Half-Lambert diffuse wrap)
                half nDotL_Wrap = saturate((dot(normalWS, light.direction) + distortion) / (1.0 + distortion));
                
                // Back-scattering / Translucency (light shining through thin skin/ears)
                half3 backLightDir = light.direction + normalWS * distortion;
                half transDot = saturate(dot(viewDirWS, -backLightDir));
                half forwardTrans = pow(transDot, power) * sssStrength;

                half3 sssTerm = (nDotL_Wrap * 0.5 + forwardTrans) * sssColor * light.color * light.distanceAttenuation * light.shadowAttenuation;
                return sssTerm;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Albedo & Alpha
                half4 albedoSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half3 albedo = albedoSample.rgb;

                // 2. Normal Mapping with Tangent Space
                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * input.tangentWS.w;
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);

                half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
                half3 normalTS = UnpackNormalScale(normalSample, _BumpScale);
                normalWS = normalize(mul(normalTS, TBN));

                // 3. Ambient Occlusion (AO)
                half aoSample = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.uv).r;
                half ao = lerp(1.0, aoSample, _OcclusionStrength);

                // 4. Metallic & Smoothness / Roughness
                half4 mgSample = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv);
                half metallic = _Metallic * mgSample.r;
                half smoothness = _Smoothness * mgSample.a;
                half roughness = 1.0 - smoothness;

                // View direction
                half3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                // 5. Main Light Calculation (PBR Direct Lighting + SSS)
                Light mainLight = GetMainLight(input.shadowCoord);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 directDiffuse = albedo * mainLight.color * (NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation);

                // Specular (Blinn-Phong / Roughness curve)
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specPower = exp2(10.0 * smoothness + 1.0);
                half specIntensity = pow(NdotH, specPower) * smoothness * (1.0 - roughness * 0.5);
                half3 specular = specIntensity * mainLight.color * mainLight.shadowAttenuation * lerp(0.04, albedo, metallic);

                // SSS Lighting for Main Light
                half3 sssLighting = CalculateSSS(mainLight, normalWS, viewDirWS, _SSSColor.rgb, _SSSStrength, _SSSDistortion, _SSSPower);

                // 6. Ambient Indirect Lighting (Spherical Harmonics + AO)
                half3 bakedGI = SampleSH(normalWS) * ao;
                half3 indirectDiffuse = albedo * bakedGI;

                // 7. Additional Lights (Point / Spot lights in scene)
                half3 additionalLighting = half3(0, 0, 0);
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light addLight = GetAdditionalLight(lightIndex, input.positionWS);
                    half addNdotL = saturate(dot(normalWS, addLight.direction));
                    half3 addDiff = albedo * addLight.color * (addNdotL * addLight.distanceAttenuation * addLight.shadowAttenuation);
                    half3 addSSS = CalculateSSS(addLight, normalWS, viewDirWS, _SSSColor.rgb, _SSSStrength * 0.6, _SSSDistortion, _SSSPower);
                    additionalLighting += addDiff + addSSS;
                }

                // Final Combined Surface Color
                half3 finalRGB = (directDiffuse + specular + sssLighting + indirectDiffuse + additionalLighting) * ao;

                return half4(finalRGB, albedoSample.a);
            }
            ENDHLSL
        }

        // Shadow Caster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Depth Only Pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
