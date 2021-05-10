using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class ArticlesDefData : IParseable
    {
        public long lArticleDef { get; set; }
        public bool bInsertPlate { get; set; }
        public long lSlotManagingType { get; set; }
        public long lElectricRechargeManageType { get; set; }
        public long lListBehavior { get; set; }
        public long lTariffBehavior { get; set; }
        public long lBaseArticle { get; set; }
        public string szArticleDefDesc { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            lArticleDef = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            bInsertPlate = !reader.IsDBNull(1) && reader.GetBoolean(1);
            lSlotManagingType = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            lElectricRechargeManageType = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            lListBehavior = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
            lTariffBehavior = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
            lBaseArticle = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
            szArticleDefDesc = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
        }
    }
}
