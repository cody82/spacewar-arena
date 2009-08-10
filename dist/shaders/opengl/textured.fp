!!ARBfp1.0

# Original Input...
ATTRIB OriginalTexCoord = fragment.texcoord;
ATTRIB OriginalTexColor = fragment.color.primary;

# Pixel Output...
OUTPUT OutColor = result.color;

# Get the texel color from the texture image.
TEMP TexelColor;
TXP TexelColor, OriginalTexCoord, texture[0], 2D;

# Modulate the texture color with the pixel color.
#MOV OutColor, TexelColor;
MUL OutColor, TexelColor, OriginalTexColor;

END