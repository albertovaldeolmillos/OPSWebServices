// Base64Stream.h

#pragma once

#include <stdlib.h>
#include <malloc.h>

#using <mscorlib.dll>
using namespace System;

namespace ManagedZLib
{
    typedef unsigned char byte;

    // Enumeration of Base64 encoding options
    public __value enum Base64Options
    {
        Encode,
        Decode
    };

    // Concrete implementation of Stream abstract class.
    public __gc class Base64Stream :
        public System::IO::Stream
    {
    private:

        static const int MAX_BUFFER_SIZE = 4000;
        static const int MAX_LINE_LEN = 76;
        static const byte charMap __nogc[] = {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/'
        };

        System::IO::Stream* pStream;
        Base64Options base64Opt;
        System::Byte workData __gc[];
        int workDataPos, lineLen, workDataReadLen;
        int bitSelect;
        byte lastBits;
        bool foundEndOfData;

        // true if opened for reading compressed data (inflating)
        bool IsReading()
        {
            return( base64Opt == Base64Options::Decode );
        }

        // true if opened for writing compressed data (deflating)
        bool IsWriting()
        {
            return( base64Opt == Base64Options::Encode );
        }

        // Encode the bytes
        void WriteBytes( byte* pReadBytes, int inputPos, int inputLen )
        {
            if( inputPos < inputLen )
            {
                byte __pin * pWriteBytes = &workData[0];
                byte queueBytes[3];
                int queueCount = 0;
                foundEndOfData = false;
                while( inputPos < inputLen )
                {
                    byte readByte = pReadBytes[inputPos++];
                    switch( bitSelect )
                    {
                    case 0:
                        queueBytes[queueCount++] = charMap[ readByte >> 2 ];
                        lastBits = ( readByte & 0x03 ) << 4;
                        bitSelect = 1;
                        break;
                    case 1:
                        queueBytes[queueCount++] = charMap[ ( readByte >> 4 ) | lastBits ];
                        lastBits = ( readByte & 0x0F ) << 2;
                        bitSelect = 2;
                        break;
                    case 2:
                        queueBytes[queueCount++] = charMap[ readByte & 0x3F ];
                        queueBytes[queueCount++] = charMap[ ( readByte >> 6 ) | lastBits ];
                        lastBits = 0;
                        bitSelect = 0;
                        break;
                    }

                    while( queueCount > 0 )
                    {
                        if( workDataPos == MAX_BUFFER_SIZE )
                        {
                            pStream->Write( workData, 0, workDataPos );
                            workDataPos = 0;
                        }
                        if( lineLen == MAX_LINE_LEN )
                        {
                            queueBytes[queueCount++] = '\n';
                            lineLen = -1;
                        }
                        else
                        {
                            workData[workDataPos++] = queueBytes[--queueCount];
                            ++lineLen;
                        }
                    }
                }
            }
        }

        // Flush any remaining bytes
        void FlushBytes()
        {
            if( foundEndOfData )
                return;

            byte __pin * pWriteBytes = &workData[0];
            byte queueBytes[5];
            int queueCount = 0;
            foundEndOfData = true;

            queueBytes[queueCount++] = '\n';
            switch( bitSelect )
            {
            case 0:
                break;
            case 1:
                queueBytes[queueCount++] = '=';
                queueBytes[queueCount++] = '=';
                queueBytes[queueCount++] = charMap[ lastBits ];
                break;
            case 2:
                queueBytes[queueCount++] = '=';
                queueBytes[queueCount++] = charMap[ lastBits ];
                break;
            }

            while( queueCount > 0 )
            {
                if( workDataPos == MAX_BUFFER_SIZE )
                {
                    pStream->Write( workData, 0, workDataPos );
                    workDataPos = 0;
                }
                if(( lineLen == MAX_LINE_LEN ) && ( queueBytes[queueCount-1] != '\n' ))
                {
                    queueBytes[queueCount++] = '\n';
                    lineLen = -1;
                }
                else
                {
                    workData[workDataPos++] = queueBytes[--queueCount];
                    ++lineLen;
                }
            }

            pStream->Write( workData, 0, workDataPos );
            workDataPos = 0;
            lastBits = 0;
            bitSelect = 0;
            lineLen = 0;
        }

        // Decode the bytes
        int ReadBytes( byte* pWriteBytes, int outputPos, int outputLen )
        {
            int readLen = 0;
            if( !foundEndOfData )
            {
                byte __pin * pReadBytes = &workData[0];
                while( outputPos < outputLen )
                {
                    if( workDataPos == workDataReadLen )
                    {
                        workDataPos = 0;
                        if(( workDataReadLen = pStream->Read( workData, 0, MAX_BUFFER_SIZE )) == 0 )
                        {
                            foundEndOfData = true;
                            break;
                        }
                    }

                    byte readByte = workData[workDataPos++];
                    int charIndex = -1;
                    if(( readByte >= 'A' ) && ( readByte <= 'Z' ))
                        charIndex = readByte - 'A';
                    else if(( readByte >= 'a' ) && ( readByte <= 'z' ))
                        charIndex = readByte - 'a' + 26;
                    else if(( readByte >= '0' ) && ( readByte <= '9' ))
                        charIndex = readByte - '0' + 52;
                    else if( readByte == '+' )
                        charIndex = 62;
                    else if( readByte == '/' )
                        charIndex = 63;
                    else if( readByte == '=' )
                        foundEndOfData = true;
                    if(( charIndex != -1 ) && !foundEndOfData )
                    {
                        switch( bitSelect )
                        {
                        case 0:
                            lastBits = (byte)charIndex << 2;
                            bitSelect = 1;
                            break;
                        case 1:
                            pWriteBytes[outputPos++] = lastBits | ( (byte)charIndex >> 4 );
                            ++readLen;
                            lastBits = ( (byte)charIndex & 0x0F ) << 4;
                            bitSelect = 2;
                            break;
                        case 2:
                            pWriteBytes[outputPos++] = lastBits | ( (byte)charIndex >> 2 );
                            ++readLen;
                            lastBits = ( (byte)charIndex & 0x03 ) << 6;
                            bitSelect = 3;
                            break;
                        case 3:
                            pWriteBytes[outputPos++] = lastBits | (byte)charIndex;
                            ++readLen;
                            lastBits = 0;
                            bitSelect = 0;
                            break;
                        }
                    }
                }
            }
            return readLen;
        }

    public:

        // Create a Base64 stream encoding/decoding object
        //
        //      stream - the stream to write encoded data to or read encoded data from
        //      option - one of the Base64Options enumerated values.
        //
        Base64Stream( System::IO::Stream* stream, Base64Options option ) :
            pStream(stream),
            base64Opt(option),
            workData( new System::Byte __gc[ MAX_BUFFER_SIZE ] ),
            workDataPos(0), lineLen(0), workDataReadLen(0),
            bitSelect(0), lastBits(0), foundEndOfData(false)
        {
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
                if( IsWriting() )
                {
                    Flush();
                    pStream->Flush();
                }
                pStream->Close();
            }
        }

        virtual void System::IDisposable::Dispose()
        {
            Close();
        }
    };
}
