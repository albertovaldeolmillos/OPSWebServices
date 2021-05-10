// Compress.h

#pragma once

#include "CompressionStream.h"

#using <mscorlib.dll>
using namespace System;

namespace ManagedZLib
{
    // Wrapper around BinaryWriter class
    public __gc class Compress :
        public System::IO::BinaryWriter
    {
    private:

        CompressionStream* pStream;

    public:

        Compress( System::IO::Stream* writeStream ) :
            System::IO::BinaryWriter( pStream = new CompressionStream( writeStream, CompressionOptions::Compress ) )
        {
        }

        Compress( System::IO::Stream* writeStream, System::Text::Encoding* encoding ) :
            System::IO::BinaryWriter( pStream = new CompressionStream( writeStream, CompressionOptions::Compress ), encoding )
        {
        }

        Compress( System::IO::Stream* writeStream, CompressionOptions option ) :
            System::IO::BinaryWriter( pStream = new CompressionStream( writeStream, option ) )
        {
        }

        Compress( System::IO::Stream* writeStream, System::Text::Encoding* encoding, CompressionOptions option ) :
            System::IO::BinaryWriter( pStream = new CompressionStream( writeStream, option ), encoding )
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
