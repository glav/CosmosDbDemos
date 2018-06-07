using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator
{
    public class CosmosDbConfig
    {
        public CosmosDbConfig(string collectionId, string databaseId)
        {
            this.CollectionId = collectionId;
            this.DatabaseId = databaseId;
        }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }

        public string Endpoint { get; set; }
        public string Key { get; set; }

        public int Throughput { get; set; }
    }
}
