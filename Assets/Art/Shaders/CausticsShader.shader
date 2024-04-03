// This shader fills the mesh shape with a color predefined in the code.
Shader "Custom/CausticsVolume"
{
    Properties
    { 
        _MainTex("Caustics Texture",2D) = "white"     
        _Tiling("Tiling", float) = 0.5  
    }
    
    SubShader
    {
        
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" "RenderQueue"="Transparent"}

        Pass
        {
            Cull Front
            ZWrite Off
            ZTest Always
            
            Blend SrcAlpha DstAlpha
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _Tiling;
            CBUFFER_END
            
            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;                 
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
            };            

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half2 TileAndOffset(half2 uv, half2 offset, half2 tiling)
            {
                return (uv + offset) * tiling;
            }
            
            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                float2 positionNDC = IN.positionHCS.xy / _ScaledScreenParams.xy;

                #if UNITY_REVERSED_Z
                float depth = SampleSceneDepth(positionNDC);
                #else
                float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif
                
                float3 positionWS = ComputeWorldSpacePosition(positionNDC, depth, UNITY_MATRIX_I_VP);

                #if UNITY_REVERSED_Z
                if(depth < 0.0001) return half4(0,0,0,1);
                #else
                if(depth < 0.9999) return half4(0,0,0,1);
                #endif

                float3 positionOS = TransformWorldToObject(positionWS);

                float boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));

                Light light = GetMainLight();
                float3 forward = light.direction;
                float3 right = normalize(cross(forward, float3(0,1,0)));
                float3 up = cross(right, forward);
                float3x3 lightMatrix = float3x3(right,up,forward);
                
                half2 projUV = mul(positionWS, lightMatrix).xy;
                                
                half4 caustics = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,TileAndOffset(projUV, _Time.yy * 0.1, _Tiling));
                half4 caustics2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,TileAndOffset(projUV, -_Time.yy * 0.02, _Tiling * 0.9));
                
                half luminance = Luminance(SampleSceneColor(positionNDC));
                luminance = smoothstep(0.05,0.2,luminance);
                
                return min(caustics,caustics2) * luminance * boundingBoxMask;
            }
            ENDHLSL
        }
    }
}