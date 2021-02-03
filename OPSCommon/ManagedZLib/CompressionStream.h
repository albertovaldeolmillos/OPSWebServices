// CompressionStream.h

#pragma once

#include <stdlib.h>
#include <malloc.h>

#include "ZLibSource\zlib.h"
#include "ZLibException.h"

#using <mscorlib.dll>
using namespace System;

namespace ManagedZLib
{
    typedef unsigned char byte;

    const int COMPRESS_FLAG         = 0x0010;
    const int DECOMPRESS_FLAG       = 0x0020;
    const int COMPRESS_LEVEL_MASK   = 0x000F;

    // Enumeration of Compression options
    public __value enum CompressionOptions
    {
        Decompress              = DECOMPRESS_FLAG,
        Compress                = COMPRESS_FLAG | 0x000F,
        CompressNone            = COMPRESS_FLAG,
        CompressFast            = COMPRESS_FLAG | 0x0001,
        CompressLevelZero       = COMPRESS_FLAG,
        CompressLevelOne        = COMPRESS_FLAG | 0x0001,
        CompressLevelTwo        = COMPRESS_FLAG | 0x0002,
        CompressLevelThree      = COMPRESS_FLAG | 0x0003,
        CompressLevelFour       = COMPRESS_FLAG | 0x0004,
        CompressLevelFive       = COMPRESS_FLAG | 0x0005,
        CompressLevelSix        = COMPRESS_FLAG | 0x0006,
        CompressLevelSeven      = COMPRESS_FLAG | 0x0007,
        CompressLevelEight      = COMPRESS_FLAG | 0x0008,
        CompressLevelNine       = COMPRESS_FLAG | 0x0009,
        CompressBest            = COMPRESS_FLAG | 0x0009
    };

    // Concrete implementation of Stream abstract class.
    public __gc class CompressionStream :
        public System::IO::Stream
    {
    private:

        static const int MAX_BUFFER_SIZE = 4000;

        System::IO::Stream* pStream;
        CompressionOptions compressOpt;
        bool success;
        z_streamp pStreamInfo;
        System::Byte workData __gc[];
        int workDataPos;
        unsigned long crcValue;
        unsigned __int64 bytesIn, bytesOut;

        // Convert enumeration to ZLib compression option
        int CompressionLevel( CompressionOptions opt )
        {
            int nLevel = opt & COMPRESS_LEVEL_MASK;
            if(( nLevel < Z_NO_COMPRESSION ) || ( nLevel > Z_BEST_COMPRESSION ))
                nLevel = Z_DEFAULT_COMPRESSION;
            return nLevel;
        }

        // true if opened for reading compressed data (inflating)
        bool IsReading()
        {
            return(( compressOpt & DECOMPRESS_FLAG ) != 0 );
        }

        // true if opened for writing compressed data (deflating)
        bool IsWriting()
        {
            return(( compressOpt & COMPRESS_FLAG ) != 0 );
        }

        // Compress the bytes
        void WriteBytes( byte* pReadBytes, int inputPos, int inputLen )
        {
            bytesIn += inputLen;

            byte __pin * pWriteBytes = &workData[0];

            pStreamInfo->next_in = &pReadBytes[inputPos];
            pStreamInfo->avail_in = inputLen;
            pStreamInfo->next_out = &pWriteBytes[workDataPos];
            pStreamInfo->avail_out = MAX_BUFFER_SIZE - workDataPos;

            crcValue = crc32( crcValue, &pReadBytes[inputPos], inputLen );

            while( pStreamInfo->avail_in != 0 )
            {
                if( pStreamInfo->avail_out == 0 )
                {
                    pStream->Write( workData, 0, MAX_BUFFER_SIZE );
                    bytesOut += MAX_BUFFER_SIZE;
                    workDataPos = 0;
                    pStreamInfo->next_out = &pWriteBytes[0];
                    pStreamInfo->avail_out = MAX_BUFFER_SIZE;
                }
                int outCount = pStreamInfo->avail_out;
                int zlibError = deflate( pStreamInfo, Z_NO_FLUSH );
                workDataPos += outCount - pStreamInfo->avail_out;
                if( zlibError < Z_OK )
                {
                    success = false;
                    throw new ZLibException( zlibError );
                }
            }
        }

        // Flush any remaining bytes
        void FlushBytes()
        {
            byte __pin * pWriteBytes = &workData[0];

            pStreamInfo->next_in = 0;
            pStreamInfo->avail_in = 0;
            pStreamInfo->next_out = &pWriteBytes[workDataPos];
            pStreamInfo->avail_out = MAX_BUFFER_SIZE - workDataPos;

            int zlibError = Z_OK;
            while( zlibError != Z_STREAM_END )
            {
                if( pStreamInfo->avail_out != 0 )
                {
                    int outCount = pStreamInfo->avail_out;
                    zlibError = deflate( pStreamInfo, Z_FINISH );
                    workDataPos += outCount - pStreamInfo->avail_out;
                    if( zlibError < Z_OK )
                    {
                        success = false;
                        throw new ZLibException( zlibError );
                    }
                }
                pStream->Write( workData, 0, workDataPos );
                bytesOut += workDataPos;
                workDataPos = 0;
                pStreamInfo->next_out = &pWriteBytes[0];
                pStreamInfo->avail_out = MAX_BUFFER_SIZE;
            }
        }

        // Decompress the bytes
        int ReadBytes( byte* pWriteBytes, int outputPos, int outputLen )
        {
            int readLen = 0;
            if( workDataPos != -1 )
            {
                byte __pin * pReadBytes = &workData[0];

                pStreamInfo->next_in = &pReadBytes[workDataPos];
                pStreamInfo->next_out = &pWriteBytes[outputPos];
                pStreamInfo->avail_out = outputLen;

                while( pStreamInfo->avail_out != 0 )
                {
                    if( pStreamInfo->avail_in == 0 )
                    {
                        workDataPos = 0;
                        pStreamInfo->next_in = &pReadBytes[0];
                        pStreamInfo->avail_in = pStream->Read( workData, 0, MAX_BUFFER_SIZE );
                        bytesIn += pStreamInfo->avail_in;
                    }

                    int inCount = pStreamInfo->avail_in;
                    int outCount = pStreamInfo->avail_out;
                    int zlibError = inflate( pStreamInfo, Z_NO_FLUSH );
                    workDataPos += inCount - pStreamInfo->avail_in;
                    readLen += outCount - pStreamInfo->avail_out;
                    if( zlibError < Z_OK )
                    {
                        success = false;
                        throw new ZLibException( zlibError );
                    }
                    else if( zlibError == Z_STREAM_END )
                    {
                        workDataPos = -1;
                        break;
                    }
                }

                crcValue = crc32( crcValue, &pWriteBytes[outputPos], readLen );
                bytesOut += readLen;
            }
            return readLen;
        }

        // Finished, free the resources used.
        void FreeResources()
        {
            if( IsWriting() )
                deflateEnd( pStreamInfo );
            else
                inflateEnd( pStreamInfo );
            delete pStreamInfo;
            pStreamInfo = 0;
            pStream = 0;
        }

    public:

        // Return the version of ZLib being used
        __property static String* get_ZLibVersion()
        {
            return new String( zlibVersion() );
        }

        // Convert a numeric level (0-9) into a CompressionOptions enumerated value
        static CompressionOptions GetLevel( int nLevel )
        {
            CompressionOptions opt;
            if(( nLevel < Z_NO_COMPRESSION ) || ( nLevel > Z_BEST_COMPRESSION ))
                opt = CompressionOptions::Compress;
            else
                opt = (CompressionOptions)( COMPRESS_FLAG | nLevel );
            return opt;
        }

        // Create a compression stream object
        //
        //      stream - the stream to write compress data to or read compressed data from
        //      option - one of the CompressionOptions enumerated values.
        //
        CompressionStream( System::IO::Stream* stream, CompressionOptions option ) :
            pStream(stream),
            compressOpt(option),
            success(false),
            pStreamInfo( (z_streamp)calloc( 1, sizeof(z_stream) ) ),
            workData( new System::Byte __gc[ MAX_BUFFER_SIZE ] ),
            workDataPos(0),
            crcValue(0),
            bytesIn(0), bytesOut(0)
        {
            int zlibError;
            if( IsWriting() )
                zlibError = deflateInit( pStreamInfo, CompressionLevel( compressOpt ) );
            else
                zlibError = inflateInit( pStreamInfo );
            if( zlibError != Z_OK )
                throw new ZLibException( zlibError );
            success = true;
        }

        // Returns the 32-bit CRC of the uncompressed data.
        __property unsigned long get_CRC()
        {
            return crcValue;
        }

        // When compressing, the number of uncompressed bytes read.
        // When decompressing, the number of compressed bytes read.
        __property unsigned __int64 get_BytesIn()
        {
            return bytesIn;
        }

        // When compressing, the number of compressed bytes written.
        // When decompressing, the number of uncompressed bytes written.
        __property unsigned __int64 get_BytesOut()
        {
            return bytesOut;
        }

        // The compression ratio obtained (same for compression/decompression).
        __property double get_CompressionRatio()
        {
            return IsWriting()
                        ? (( bytesIn == 0 ) ? 0.0 : ( 100.0 - ((double)bytesOut * 100.0 / (double)bytesIn )))
                        : (( bytesOut == 0 ) ? 0.0 : ( 100.0 - ((double)bytesIn * 100.0 / (double)bytesOut )));
        }


        //---------------------------------------
        // Stream implementation
        //---------------------------------------

        __property virtual bool get_CanRead()
        {
            return IsReading();
        }

        __property virtual bool get_CanSeek()
        {
            return false;
        }

        __property virtual bool get_CanWrite()
        {
            return IsWriting();
        }

        __property virtual __int64 get_Length()
        {
            throw new System::NotSupportedException( new System::String( "Length property not supported." ) );
            return 0;
        }

        __property virtual __int64 get_Position()
        {
            throw new System::NotSupportedException( new System::String( "Position property not supported." ) );
            return 0;
        }
        __property virtual void set_Position( __int64 value )
        {
            throw new System::NotSupportedException( new System::String( "Position property not supported." ) );
        }

        virtual __int64 Seek( __int64 offset, System::IO::SeekOrigin origin )
        {
            throw new System::NotSupportedException( new System::String( "Seek method not supported." ) );
            return 0;
        }

        virtual void SetLength( __int64 value )
        {
            throw new System::NotSupportedException( new System::String( "SetLength method not supported." ) );
        }

        virtual int Read( byte outputData __gc[], int outputPos, int outputLen )
        {
            if( !IsReading() )
                throw new System::NotSupportedException( new System::String( "Stream is not open for reading." ) );
            byte __pin * pWriteBytes = &outputData[0];
            return ReadBytes( pWriteBytes, outputPos, outputLen );
        }

        virtual int ReadByte()
        {
            if( !IsReading() )
                throw new System::NotSupportedException( new System::String( "Stream is not open for reading." ) );
            byte byteValue;
            return(( ReadBytes( &byteValue, 0, 1 ) == 1 ) ? byteValue : -1 );
        }

        virtual void Write( byte inputData __gc[], int inputPos, int inputLen )
        {
            if( !IsWriting() )
                throw new System::NotSupportedException( new System::String( "Stream is not open for writing." ) );
            if( inputLen != 0 )
            {
                byte __pin * pReadBytes = &inputData[0];
                WriteBytes( pReadBytes, inputPos, inputLen );
            }
        }

        virtual void WriteByte( byte value )
        {
            if( !IsWriting() )
                throw new System::NotSupportedException( new System::String( "Stream is not open for writing." ) );
            WriteBytes( &value, 0, 1 );
        }

        virtual void Flush()
        {
            if( !IsWriting() )
                throw new System::NotSupportedException( new System::String( "Stream is not open for writing." ) );
            FlushBytes();
        }

        virtual void Close()
        {
            if( pStream )
            {
                try
                {
                    if( IsWriting() && success )
                    {
                        Flush();
                        pStream->Flush();
                    }
                    pStream->Close();
                }
                catch( ... )
                {
                    FreeResources();
                    throw;
                }
                FreeResources();
            }
        }

        virtual void System::IDisposable::Dispose()
        {
            Close();
        }
    };
}
