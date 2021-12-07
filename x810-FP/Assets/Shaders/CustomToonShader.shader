Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NumLayers ("Num Layers", Int) = 5
        _StartingBound ("Starting Bound", Range(0.1,2.0)) = 0.8
        _IntensityPerStep ("Intensity Per Step", Range(0.01,2.0)) = 0.15
        _MinLight ("Min Light", Range(0,1)) = 0
        _MaxLight ("Max Light", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
                
            }

            fixed4 _Color;
            half _NumLayers;
            half _StartingBound;
            half _IntensityPerStep;
            half _MinLight;
            half _MaxLight;
            
            
            fixed4 frag (v2f i) : SV_Target
            {
                float intensity;
                float3 light = normalize(_WorldSpaceLightPos0.xyz);
                float3 vNormal = normalize(i.worldNormal);
                intensity = dot(light,vNormal);
                // float4 color;
                fixed4 color = _Color;
                fixed4 mainColor = _Color;
                _MaxLight = max(_MinLight, _MaxLight);

                
                //// the number of steps on the texture
                int stepNum = _NumLayers; 
                //// the float version of steps
                float stepNum_float = float(stepNum);
                //// save the value of the previous step to compare the next step to 
                float stepOld = 0.0;
                //// calculates the amount of intensity to subtract on each subsequent iteration
                float step = 2.0/stepNum_float;
                //// the start value of the intensity to check against
                float start = _StartingBound;
                //// how much to decrease the color by (how much darker to make it each step)
                float colorIntensity = _IntensityPerStep;
                //// save the color of the previous step
                float4 oldColor = float4(0,0,0, 1.0);
                //// used to calculate the very end color so its not just black
                float i_float = 0.0;
                
                // // set the darkest end's color 
                //if (intensity < 0.8) {
                     //color = float4(0,1,1,1);
                     //olor = float4(0,0,0, 1.0); // make the end always black
                     //color = float4((mainColor/(1.0+(stepNum_float*(colorIntensity * (2.0*stepNum_float))))));
                // } // if
                
                // iterate over the number of steps to add to the toon shader
                for (int i = 0; i < stepNum; i++) {
                    // cast i to a float
                    i_float = float(i);
                    // calculate the intensity step for the lower bound
                    float intensityStep = (start-(i_float*step));
                    // if the intensity is less than the old step's upper bound 
                    if (intensity < stepOld) {
                         // if the intensity is more than the current lower bound  
                         if (intensity > intensityStep) {
                             // save the current color to the old color value
                             oldColor = float4((mainColor/(1.0+(i_float*colorIntensity))));
                             // set the color to the current color
                             color = oldColor;
                         } // if 
                    } // if 
                    colorIntensity = colorIntensity * 2.0;
                    stepOld = intensityStep;
                } // if
                
                if (intensity < stepOld)
                    color = oldColor;

                //fixed4 darkColor = fixed4(1,1,1,1);
                //float4 blendCol = lerp(color, darkColor, 0.1);
                //float4 blendCol = (color + darkColor) * 0.5;
               
                return color;
                
                

                /*
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
                */
            }
            ENDCG
        }
    }
}
