Shader "WordChompers/GridLines"
{
    Properties
    {
        _BaseColor    ("Line Base Color",  Color) = (0.20, 0.05, 0.35, 1)
        _CrestColor   ("Line Crest Color", Color) = (0.70, 0.45, 0.95, 1)
        _Cols         ("Columns",          Float) = 6
        _Rows         ("Rows",             Float) = 6
        _CellWidthPx  ("Cell Width px",    Float) = 160
        _CellHeightPx ("Cell Height px",   Float) = 100
        _LineWidthPx  ("Line Width px",    Float) = 8

        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color            ("Tint",               Color) = (1,1,1,1)
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull      Off
        Lighting  Off
        ZWrite    Off
        ZTest     [unity_GUIZTestMode]
        Blend     SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _BaseColor;
            fixed4 _CrestColor;
            float  _Cols;
            float  _Rows;
            float  _CellWidthPx;
            float  _CellHeightPx;
            float  _LineWidthPx;
            fixed4 _Color;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex        = UnityObjectToClipPos(v.vertex);
                OUT.texcoord      = v.texcoord;
                OUT.color         = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Position within each cell (wraps 0-1 per cell)
                float2 cellUV = frac(uv * float2(_Cols, _Rows));

                // Distance from nearest cell edge: 0 = on a line, 0.5 = cell centre
                float2 dCell  = min(cellUV, 1.0 - cellUV);

                // Convert to logical pixels
                float dxPx = dCell.x * _CellWidthPx;
                float dyPx = dCell.y * _CellHeightPx;

                float halfW = _LineWidthPx * 0.5;
                bool  onV   = dxPx < halfW;
                bool  onH   = dyPx < halfW;

                if (!onV && !onH)
                    return fixed4(0, 0, 0, 0);

                // Normalised distance from line centre (0 = crest, 1 = edge)
                float nx = saturate(dxPx / halfW);
                float ny = saturate(dyPx / halfW);

                // sqrt gives a true circular (hemisphere) cross-section
                float t;
                if (onV && onH)
                    t = sqrt(saturate(1.0 - nx * nx - ny * ny)); // dome at crossing
                else if (onV)
                    t = sqrt(saturate(1.0 - nx * nx));            // vertical cylinder
                else
                    t = sqrt(saturate(1.0 - ny * ny));            // horizontal cylinder

                fixed4 col  = lerp(_BaseColor, _CrestColor, t);
                col        *= IN.color;
                col.a      *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                return col;
            }
            ENDCG
        }
    }
}
