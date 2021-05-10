using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class Timetables : IParseable
    {
        public long TIM_ID { get; set; }
        public long TIM_INI { get; set; }
        public long TIM_END { get; set; } 

        public void Parse(IDataReader reader, long tableVersion)
        {
            TIM_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            TIM_INI = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            TIM_END = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
        }
    }
}
