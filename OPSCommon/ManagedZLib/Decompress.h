// Decompress.h

#pragma once

#include "CompressionStream.h"

#using <mscorlib.dll>
using namespace System;

namespace ManagedZLib
{
    // Wrapper around the BinaryReader class
    public __gc class Decompress :
        public System::IO::BinaryReader
    {
    private:

        CompressionStream* pStream;

    public:

        Decompress( System::IO::Stream* readStream ) :
            System::IO::BinaryReader( pStream = new CompressionStream( readStream, CompressionOptions::Decompress ) )
        {
        }

        Decompress( System::IO::Stream* readStream, System::Text::Encoding* encoding ) :
            System::IO::BinaryReader( pStream = new CompressionStream( readStream, CompressionOptions::Decompress ), encoding )
        {
        }

        __property unsigned long get_CRC()
        {
            return pStream->CRC;
        }

        __property unsigned __int64 get_BytesIn()
        {
            return pStream->BytesIn;
        }

        __property unsigned __int64 get_BytesOut()
        {
            return pStream->BytesOut;
        }

        __property double get_CompressionRatio()
        {
            return pStream->CompressionRatio;
        }
    };
}
