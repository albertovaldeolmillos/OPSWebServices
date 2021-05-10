using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMHelpers
{
    public class OperationDat
    {
        public const int DEF_OPERTYPE_UNDEF = -1;
        public const int DEF_OPERTYPE_PARK = 1;	// PARKING
        public const int DEF_OPERTYPE_AMP = 2;	// PARKING AMPLIATION
        public const int DEF_OPERTYPE_RETN = 3;	// REFUND
        public const int DEF_OPERTYPE_FINE = 4;	// FINE
        public const int DEF_OPERTYPE_RECH = 5;	// CARD RECHARGE
        public const int DEF_OPERTYPE_SHIP = 6;	// SHIP BOOK
        public const int DEF_OPERTYPE_VIGI_FILE = 100;	// GUARD CLOCK IN
        public const int DEF_OPERTYPE_RES_PAYMENT = 101;// RESIDENT PAYMENT
        public const int DEF_OPERTYPE_POWER_RECH = 102;// POWER RECHARGE
        public const int DEF_OPERTYPE_BYCING = 103;	// BYCING
        public const int DEF_OPERTYPE_LOAD_UNLOAD = 104;// LOAD UNLOAD
        public const int DEF_OPERTYPE_UPLOCK_OPEN = 105;// Up Lock opened
        public const int DEF_OPERTYPE_DOWNLOCK_OPEN = 106;// Down Lock opened
        public const int DEF_OPERTYPE_PARK_REFUND = 107;// Refund the parking operation
        public const int DEF_OPERTYPE_FINE_REFUND = 108;// Refund the fine operation

        public const int DEF_OPERPAY_UNDEF = -1;
        public const int DEF_OPERPAY_CHIPCARD	= 1;
        public const int DEF_OPERPAY_CREDITCARD	= 2;
        public const int DEF_OPERPAY_COINS  = 3;
        public const int DEF_OPERPAY_MOBILE	= 4;

    }
}
