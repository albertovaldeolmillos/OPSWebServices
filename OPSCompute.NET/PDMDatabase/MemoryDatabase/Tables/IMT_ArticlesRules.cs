
using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_ArticlesRules : InMemoryTable<ArticlesRules>
    {
        public IMT_ArticlesRules(ILoggerManager loggerManager, IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_ArticlesRules::LoadData");

            try
            {
                ArticlesRulesSelectCommand command = new ArticlesRulesSelectCommand(Version, Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                IsLoaded = false;
            }
        }


        public bool GetConstAndTar(COPSDate pdtWork, long group, long typeOfGroup, long typeOfArticle, long typeOfDay, long timId, out ArticlesRules foundArticleRule)
        {
            trace?.Write(TraceLevel.Debug, "IMT_ArticlesRules::GetConstAndTarGetConstAndTar");
            bool fnResult = true;
            foundArticleRule = default(ArticlesRules);

            try
            {
                if (group == GlobalDefs.DEF_UNDEFINED_VALUE && typeOfGroup == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new InvalidOperationException("lGroup and lTypeOfGroup cannot be both undefined");

                if (typeOfArticle == GlobalDefs.DEF_UNDEFINED_VALUE)
                    throw new InvalidOperationException("Type of article undefined");

                trace?.Write(TraceLevel.Info, $"QUERY : Wich are the Constrants and Tariffs for Type of article={typeOfArticle}, Group={group}, Type of group={typeOfGroup}");

                if (group != GlobalDefs.DEF_UNDEFINED_VALUE)
                {
                    IEnumerable<ArticlesRules> results = Data.Where(w => w.RUL_DART_ID == typeOfArticle && 
                                                                         w.RUL_GRP_ID == group &&
                                                                         w.RUL_INIDATE.Value <= pdtWork.Value &&
                                                                         w.RUL_ENDDATE.Value > pdtWork.Value);
                    if (results.Any())
                    {
                        if (typeOfDay != GlobalDefs.DEF_UNDEFINED_VALUE && timId != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {

                            bool exist = results.Any(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }

                        if (typeOfDay != GlobalDefs.DEF_UNDEFINED_VALUE && timId == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bool exist = results.Any(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (!ar.RUL_TIM_ID.HasValue));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (!ar.RUL_TIM_ID.HasValue));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }

                        if (typeOfDay == GlobalDefs.DEF_UNDEFINED_VALUE && timId != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bool exist = results.Any(ar =>  (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId) && (!ar.RUL_DDAY_ID.HasValue));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId) && (!ar.RUL_DDAY_ID.HasValue));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }

                        if (typeOfDay == GlobalDefs.DEF_UNDEFINED_VALUE && timId == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bool exist = results.Any(ar => (!ar.RUL_TIM_ID.HasValue  && !ar.RUL_DDAY_ID.HasValue));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (!ar.RUL_TIM_ID.HasValue && !ar.RUL_DDAY_ID.HasValue));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }
                    }
                }

                if (foundArticleRule == default(ArticlesRules) &&  typeOfGroup != GlobalDefs.DEF_UNDEFINED_VALUE )
                {
                    IEnumerable<ArticlesRules> results = Data.Where(w => w.RUL_DART_ID == typeOfArticle &&
                                                                         w.RUL_DGRP_ID == typeOfGroup &&
                                                                         w.RUL_INIDATE.Value <= pdtWork.Value &&
                                                                         w.RUL_ENDDATE.Value > pdtWork.Value);
                    if (results.Any())
                    {
                        if (typeOfDay != GlobalDefs.DEF_UNDEFINED_VALUE && timId != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {

                            bool exist = results.Any(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }

                        if (typeOfDay != GlobalDefs.DEF_UNDEFINED_VALUE && timId == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bool exist = results.Any(ar => (ar.RUL_DDAY_ID.HasValue && ar.RUL_DDAY_ID == typeOfDay) && (!ar.RUL_TIM_ID.HasValue));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId) && (!ar.RUL_DDAY_ID.HasValue));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }

                        if (typeOfDay == GlobalDefs.DEF_UNDEFINED_VALUE && timId != GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bool exist = results.Any(ar => (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId) && (!ar.RUL_DDAY_ID.HasValue));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (ar.RUL_TIM_ID.HasValue && ar.RUL_TIM_ID == timId) && (!ar.RUL_DDAY_ID.HasValue));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }

                        if (typeOfDay == GlobalDefs.DEF_UNDEFINED_VALUE && timId == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            bool exist = results.Any(ar => (!ar.RUL_TIM_ID.HasValue && !ar.RUL_DDAY_ID.HasValue));
                            if (exist)
                            {
                                foundArticleRule = results.First(ar => (!ar.RUL_TIM_ID.HasValue && !ar.RUL_DDAY_ID.HasValue));
                                trace?.Write(TraceLevel.Info, $"Article Rule Found :: Tariff({foundArticleRule.RUL_TAR_ID}) Constrants({foundArticleRule.RUL_CON_ID})");
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (fnResult)
            {
                trace?.Write(TraceLevel.Info, $"Constraint ({foundArticleRule?.RUL_CON_ID.Value}) AND TARIFF ({foundArticleRule?.RUL_TAR_ID}) ");
            }
            else {
                trace?.Write(TraceLevel.Error, $"ERROR: NO TARIFF AND CONSTRAINT");
            }

            return fnResult;
        }
    }
}
