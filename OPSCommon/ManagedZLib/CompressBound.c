// CompressBound.c

#define ZLIB_INTERNAL
#include "ZLibSource\zlib.h"

// Extracted from original compress.c
uLong ZEXPORT compressBound (sourceLen)
    uLong sourceLen;
{
    return sourceLen + (sourceLen >> 12) + (sourceLen >> 14) + 11;
}

