
using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_Articles : InMemoryTable<Articles>
    {
        public IMT_Articles(ILoggerManager loggerManager, IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            try
            {
                ArticlesSelectCommand command = new ArticlesSelectCommand(Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                IsLoaded = false;
            }
        }

        public bool LoadArticle(long article, string date) {
            try
            {
                ArticlesFindByIdCommand command = new ArticlesFindByIdCommand(article, date, Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception)
            {
                IsLoaded = false;
            }

            return IsLoaded;
        }

        public bool FindArticle(long lArticle,ref bool bFind,ref COPSDate dtArtIni,ref COPSDate dtArtEnd,ref long plArtDef, ref COPSPlate pstrVehicle, ref long plCusID)
        {
            Articles articulo = null;
            bool fnResult = false;
            try
            {
                articulo = Data.SingleOrDefault<Articles>(w => w.ART_ID == lArticle);

                dtArtIni = new COPSDate(articulo.ART_INIDATE);
                dtArtEnd = new COPSDate(articulo.ART_ENDDATE);
                plArtDef = articulo.ART_DART_ID;
                pstrVehicle = new COPSPlate(articulo.ART_VEHICLEID);
                plCusID = articulo.ART_CUS_ID;

                fnResult = true;
            }
            catch (Exception)
            {
                fnResult = false;
            }

            bFind = fnResult;

            return fnResult;
        }
    }
}
