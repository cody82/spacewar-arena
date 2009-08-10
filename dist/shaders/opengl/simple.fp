!!ARBfp1.0

# Original Input...
#ATTRIB OriginalTexCoord = fragment.texcoord;
ATTRIB OriginalTexColor = fragment.color.primary;

OUTPUT OutColor = result.color;

MOV OutColor, OriginalTexColor;

END