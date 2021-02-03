// ManagedZLib.h

#pragma once

#include <windows.h>
#include "_vcclrit.h"

#using <mscorlib.dll>
using namespace System;

namespace ManagedZLib
{
    //
    // These methods must be called to initialize and terminate the .NET side of things,
    // which is unsafe to do in DllMain().
    //
    //  See:
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dv_vstechart/html/vcconMixedDLLLoadingProblem.asp
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/vcmex/html/vcconconvertingmanagedextensionsforcprojectsfrompureintermediatelanguagetomixedmode.asp
    //
    public __gc class ManagedZLib
    {
    public:
        static void Initialize()
        {
            __crt_dll_initialize();
        }
        static void Terminate()
        {
            __crt_dll_terminate();
        }
    };
}
