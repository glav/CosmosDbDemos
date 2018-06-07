using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator
{
    public class CosmosDbConfig
    {
        public CosmosDbConfig(string databaseId)
        {
            this.DatabaseId = databaseId;
        }
        public string DatabaseId { get; set; }
        public string SmallCollectionId { get; set; }
        public string LargeCollectionId { get; set; }

        public string Endpoint { get; set; }
        public string Key { get; set; }

        public int SmallThroughput { get; set; }
        public int LargeThroughput { get; set; }
    }
}
