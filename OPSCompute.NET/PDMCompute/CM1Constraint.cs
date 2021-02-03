using System;

namespace PDMCompute
{
    public class CM1Constraint
    {
        public const int CNSTR_UNDEFINED = -1;
        public const int CNSTR_BASE = 1;
        public const int CNSTR_MAX_ESTANCIA = (CNSTR_BASE + 0);         // 1, 'Tiempo máximo de estancia permitido'
        public const int CNSTR_REENTRY_TIME = (CNSTR_BASE + 1);	        // 2, 'Tiempo mínimo antes de una nueva reentrada'
        public const int CNSTR_CORTESY_TIME	=(CNSTR_BASE + 2);	        // 3, 'Tiempo de cortesia'
        public const int CNSTR_MAX_IMPORT	=(CNSTR_BASE + 3);	        // 4, 'Importe máximo permitido'
        public const int CNSTR_MIN_IMPORT	=(CNSTR_BASE + 4);	        // 5, 'Importe mínimo permitido'
        public const int CNSTR_CORTESY_PAY	=(CNSTR_BASE + 5);	        // 6, 'Cobro o no del tiempo de cortesía'
        public const int CNSTR_HISTORY_EVAL	=(CNSTR_BASE + 6);	        // 7, 'Se aplica o no el histórico'
        public const int CNSTR_RETN_ALLOW	=(CNSTR_BASE + 7);	        // 8, 'Se permite o no devolución'
        public const int CNSTR_MAX_INTERDATE_REE = (CNSTR_BASE + 8);	// 9, 'En caso de salto de día tiempo que máximo
                                                                        //      que se puede alarga al principio de día sin 
                                                                        //      que se tenga en cuenta para cálculo de reentrada
        public const int CNSTR_POSTPAY_TIME		=(CNSTR_BASE + 9);      // 10, 'Tiempo de postpago
        public const int CNSTR_INTRA_ZONE_PARK = (CNSTR_BASE + 10);     // 11, 'Se puede estacionar en un sector hijo si hay aparcamiento
                                                                        //      en otro de los hijos
        public const int CNSTR_INTRA_ZONE_COURTESY = (CNSTR_BASE + 11);	// 12, 'Si el parametro anterior es si, cuanto tiempo hay para a
                                                                        //      en un sector hijo si antes se ha aparcado en otro de los hijos
        public const int CNSTR_AMP_ALLOW = (CNSTR_BASE + 12);	        // 13, 'Se permite o no ampliar'
        public const int CNSTR_INTRA_ZONE_PARK_EXCEPTION    =(CNSTR_BASE + 13);	// 14, 'En caso de no permitir intra zone park
        // permitir una excepcion para el grupo especificado
        public const int CNSTR_REFUND_MINIMUM_VALUE_TO_CHARGE = (CNSTR_BASE + 14);	// 15, 'Valor minimo a cobrar en una devolución
        public const int CNSTR_NUM = 15;

        public long TypeId { get; set; }
        public float Value { get; set; }

        public CM1Constraint()
        {
            Set(CNSTR_UNDEFINED, CNSTR_UNDEFINED);
        }
        public CM1Constraint(long typeId)
        {
            Set(typeId, CNSTR_UNDEFINED);
        }
        public CM1Constraint(long typeId, float value)
        {
            Set(typeId, value);
        }

        public void Set(long typeId, float value)
        {
            TypeId = typeId;
            Value = value;
        }
        public void Set(float value)
        {
            Value = value;
        }

        public bool IsValid()
        {
            return TypeId != CNSTR_UNDEFINED && Value != CNSTR_UNDEFINED;
        }
        public static bool IsValidTypeId(long typeId) {
            return ((typeId) >= CNSTR_BASE && (typeId) <= CNSTR_NUM);
        }

        public bool Merge(float fInValue,ref  float? pfOutValue)
        {
            bool fnResult = true;

            try
            {
                if (TypeId == CNSTR_UNDEFINED) {
                    throw new InvalidOperationException("Type is Undefined");
                }

                if (Value == CNSTR_UNDEFINED)
                {
                    Value = fInValue;
                }
                else {
                    switch (TypeId)
                    {
                        case CNSTR_MAX_ESTANCIA:
                        case CNSTR_REENTRY_TIME:
                        case CNSTR_CORTESY_TIME:
                        case CNSTR_MAX_IMPORT:
                        case CNSTR_MIN_IMPORT:
                            Value = Math.Min(Value, fInValue);
                            break;
                        case CNSTR_CORTESY_PAY:
                            Value = Math.Max(Value, fInValue);
                            break;
                        default:
                            throw new InvalidOperationException("Type out of range");
                    }
                }
            }
            catch (Exception)
            {
                fnResult = false;
            }

            // If output parameter is not null assing it
            if (pfOutValue != default(float))
                pfOutValue = Value;

            return fnResult;
        }
        public bool Merge(CM1Constraint pCnstr, ref float? pfValue)
        {
            bool bRdo = true;
            try
            {
                if (pCnstr == null)
                    throw new InvalidOperationException("pCnstr NULL");

                if (pCnstr.TypeId != TypeId)
                    throw new InvalidOperationException("TypeId different!!");

                Merge(pCnstr.Value, ref pfValue);
            }
            catch (Exception)
	        {
                bRdo = false;
            }
            return bRdo;
        }
        public bool Copy(CM1Constraint pSrcConstraint)
        {
            bool bRdo = true;
            try
            {
                TypeId = pSrcConstraint.TypeId;
                Value = pSrcConstraint.Value;

            }
            catch (Exception)
	        {
                bRdo = false;
            }
            return bRdo;
        }
    }
}