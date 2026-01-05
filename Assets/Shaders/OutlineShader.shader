Shader "Custom/OutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 1, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            float4 _OutlineColor;
            float _OutlineWidth;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Expand in object space with scale compensation
                float3 norm = normalize(v.normal);
                float3 expandedPos = v.vertex.xyz + norm * _OutlineWidth;
                
                // Transform to clip space
                float4 clipPos = TransformObjectToHClip(expandedPos);
                
                // Push slightly back in depth to ensure it renders behind
                clipPos.z += 0.001;
                
                o.pos = clipPos;
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            
            ENDHLSL
        }
    }
}
