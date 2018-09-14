using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosCommon.CosmosDb
{
    public class CosmosDbConfig
    {
        public CosmosDbConfig(string databaseIdPrefix)
        {
            this.DatabaseIdPrefix = databaseIdPrefix;
        }
        public string DatabaseIdSQL { get { return $"{DatabaseIdPrefix}-sql"; } }
        public string DatabaseIdGRAPH { get { return $"{DatabaseIdPrefix}-graph"; } }
        public string DatabaseIdPrefix { get; set; }
        public string SmallCollectionId { get; set; }
        public string LargeCollectionId { get; set; }

        public string CosmosDbGraphEndpoint { get; set; }
        public string CosmosDbSqlEndpoint { get; set; }
        public string KeySql { get; set; }
        public string KeyGraph { get; set; }

        public int SmallThroughput { get; set; }
        public int LargeThroughput { get; set; }

        public int SmallDocumentCount { get; set; }
        public int LargeDocumentCount { get; set; }
    }
}
