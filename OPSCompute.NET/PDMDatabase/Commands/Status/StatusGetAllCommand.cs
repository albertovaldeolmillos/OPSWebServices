////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Repositories
{
    public class StatusGetAllCommand : BaseCommand
    {
        public StatusGetAllCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = @"SELECT  STA_ID, STA_PSTA_ID, STA_DGRP_ID, STA_GRP_ID, STA_DDAY_ID, STA_DAY_ID, STA_TIM_ID, 
                                        STA_UNI_ID, STA_DSTA_ID
                                FROM STATUS 
                                where STA_VALID = 1 and STA_DELETED = 0",
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Status> Execute()
        {
            return connection.Query<Status>(this);
        }
    }
}