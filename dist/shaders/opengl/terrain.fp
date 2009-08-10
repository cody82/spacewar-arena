!!ARBfp1.0

# Original Input...
ATTRIB OriginalTexCoord = fragment.texcoord;
ATTRIB OriginalTexColor = fragment.color.primary;
ATTRIB pos = fragment.position;

ATTRIB fogCoord = fragment.fogcoord;

# Pixel Output...
OUTPUT OutColor = result.color;

PARAM fogColor = state.fog.color;

# Get the texel color from the texture image.
TEMP TexelColor1;
TEMP TexelColor2;
TEMP temp;
TEMP temp2;
TEMP fogFactor;
TEMP p;
#MOV p,{-0.0025, 1.25, 0, 0};
#MOV p,{-1/(END-START), END/(END-START), 0, 0};
MOV p,{-0.001282, 1.02564, 0, 0};

MUL temp,OriginalTexCoord,{256,256,1,1};
#MUL temp,OriginalTexCoord,{1,1,1,1};

TXP TexelColor1, OriginalTexCoord, texture[0], 2D;
TXP TexelColor2, temp, texture[1], 2D;

#MUL temp,TexelColor1,TexelColor2;
MUL temp,TexelColor1,{1,1,1,1};
MUL temp,temp,OriginalTexColor;

# Modulate the texture color with the pixel color.

MAD_SAT fogFactor.x, p.x, fogCoord.x, p.y;
#MOV fogFactor.x,{0,1,1,1};
LRP OutColor.rgb, fogFactor.x, temp, fogColor;
#MOV OutColor,{1,1,1,1};

#greyscale
#ADD temp2,temp,temp.yzxw;
#ADD temp2,temp2,temp.zxyw;
#MOV temp2.w,{1,1,1,1};
#MUL temp2,temp2,{0.333,0.333,0.333,1};
#MOV OutColor,temp;

#cut
#SUB temp,pos,{100,100,0,0};
#KIL temp;

#mod2
#MUL temp,pos,{0.5,0.5,1,1};
#FRC temp,temp;
#SUB temp,temp,{0.0,0.3,0,0};
#MOV temp.zw,{1,1,1,1};
#KIL temp;

#MOV OutColor, temp;



END