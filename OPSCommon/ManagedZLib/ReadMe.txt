========================================================================
   ReadMe.txt - ManagedZLib Project Overview
========================================================================

ZLib .NET Wrapper - Version 1.1.1, May 31st, 2005

Copyright (C) 2004,2005 Dave F. Baskin

This project wraps version 1.2.2 of the ZLib source in a Microsoft .NET
mixed-mode DLL wrapper.  This should give native performance to the ZLib
functions while wrapping the compression and decompression functions
in a .NET-friendly Stream object.  Additional BinaryReader- and
BinaryWriter-derived classes are also defined to provide thin wrappers
around the compression Stream object.

There were no changes made to the ZLib source (thus the version number
of ZLib was not changed).

This software is released under the same ZLib license as the ZLib
source itself:

-------------------------------------------------------------------------
From zlib.h
-------------------------------------------------------------------------

ZLib - Version 1.2.2, October 3rd, 2004

Copyright (C) 1995-2004 Jean-loup Gailly and Mark Adler

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.

Jean-loup Gailly        Mark Adler
jloup@gzip.org          madler@alumni.caltech.edu


The data format used by the zlib library is described by RFCs (Request for
Comments) 1950 to 1952 in the files http://www.ietf.org/rfc/rfc1950.txt
(zlib format), rfc1951.txt (deflate format) and rfc1952.txt (gzip format).
  
-------------------------------------------------------------------------

