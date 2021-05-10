using System.Data;
using System.Collections.Generic;
using PDMDatabase.Models;

namespace IMT_DaysDefTest
{
    public class FakeDaysDefRepository : PDMDatabase.Repositories.DaysDefRepository
    {
        public FakeDaysDefRepository(IDbConnection connection) : base(connection) { }

        public override IEnumerable<DaysDef> GetAll() {
            return new List<DaysDef>() {
                { new DaysDef() { DDAY_ID = 10  , DDAY_CODE ="0000010", DDAY_DESC = "Sábado" } },
                { new DaysDef() { DDAY_ID = 11  , DDAY_CODE ="0000001", DDAY_DESC = "Domingo" } },
                { new DaysDef() { DDAY_ID = 0   , DDAY_CODE ="1111111", DDAY_DESC = "Todos"} },
                { new DaysDef() { DDAY_ID = 2   , DDAY_CODE ="1111100", DDAY_DESC = "Laborables"} },
                { new DaysDef() { DDAY_ID = 3   , DDAY_CODE ="0000000", DDAY_DESC = "Semifestivos"} },
                { new DaysDef() { DDAY_ID = 4   , DDAY_CODE ="0000000", DDAY_DESC = "Festivos"} },
                { new DaysDef() { DDAY_ID = 5   , DDAY_CODE ="1000000", DDAY_DESC = "Lunes"} },
                { new DaysDef() { DDAY_ID = 6   , DDAY_CODE ="0100000", DDAY_DESC = "Martes"} },
                { new DaysDef() { DDAY_ID = 7   , DDAY_CODE ="0010000", DDAY_DESC = "Miércoles"} },
                { new DaysDef() { DDAY_ID = 8   , DDAY_CODE ="0001000", DDAY_DESC = "Jueves"} },
                { new DaysDef() { DDAY_ID = 9   , DDAY_CODE ="0000100", DDAY_DESC = "Viernes"} },
                { new DaysDef() { DDAY_ID = 1   , DDAY_CODE ="0010000", DDAY_DESC = "Mercado popular"} },
                { new DaysDef() { DDAY_ID = 12  , DDAY_CODE ="1111110", DDAY_DESC = "No Domingos Festivos"} },
                { new DaysDef() { DDAY_ID = 13  , DDAY_CODE ="0000011", DDAY_DESC = "Fines de Semana"} }
            }.AsReadOnly();
        }
        
    }
}
