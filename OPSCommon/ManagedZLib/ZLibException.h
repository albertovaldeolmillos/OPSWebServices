// ZLibException.h

#pragma once

#using <mscorlib.dll>
using namespace System;

namespace ManagedZLib
{
    // Simple exception thrown for errors from ZLib functions
    public __gc class ZLibException :
        public System::Exception
    {
    private:

        int zlibErrorCode;

    public:

        ZLibException( int zlibError ) :
            System::Exception( System::String::Format( "ZLib Error: {0}", __box(zlibError) ) )
        {
            zlibErrorCode = zlibError;
        }

        __property int get_ZLibError()
        {
            return zlibErrorCode;
        }
    };
}
