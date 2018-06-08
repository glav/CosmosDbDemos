using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator.CosmosDb
{
    public class DataDocument
    {
        public string id { get; set; }
        public bool useGoodPartitionKey { get; set; }
        public string partitionKey { get { return useGoodPartitionKey ? Guid.NewGuid().ToString() : "12345"; } }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int age { get; set; }
        public Address address { get; set; }
    }

    public class Address
    {
        public string streetAddress { get; set; }
        public string suburb { get; set; }
        public string state { get; set; }
        public int postcode { get; set; }
    }

    public static class SampleDocumentCreator
    {
        private readonly static string[] FirstNames = new string[]
        {
            "Mary", "Bob", "Jane", "Jess", "Paul", "Pennywise", "Donald", "Freddy", "Angus", "Aristotle", "PeeWee", "Michele"
        };
        private readonly static string[] LastNames = new string[]
        {
            "Magnolia","Jones","Smith","ChumbaWumba","Duck","Krueger","Dumbledore","Cucumber","Unicorn","Herman","RandomPerson","Beetlejuice"
        };
        public readonly static string[] States = new string[]
        {
            "NSW","TAS","VIC","SA","WA","QLD","ACT"
        };
        public readonly static string[] Streets = new string[]
        {
            "Brown st", "Tarmac rd", "Two way st","Pothole rd","MyTaxesPaidForThis st","Some stupid name cr","Hells highway","Ridiculous rd","Whatever way"
        };
        public readonly static string[] Suburbs = new string[]
        {
            "Sydney", "Suburbia", "FreeVille","Woop Woop","Back of Bourke","Outback","The Bush","The Footpath","Somewhere"
        };
        public static List<DataDocument> Generate(int count, bool useGoodPartitionKey, int startIdCountFrom = 0)
        {
            var data = new List<DataDocument>();
            var rnd = new Random(DateTime.Now.Millisecond);
            for (var cnt = 0; cnt < count; cnt++)
            {
                data.Add(new DataDocument
                {
                    id = (cnt + 1 + startIdCountFrom).ToString(),
                    age = rnd.Next(10, 99),
                    firstName = FirstNames[rnd.Next(0, FirstNames.Length - 1)],
                    lastName = LastNames[rnd.Next(0, LastNames.Length - 1)],
                    useGoodPartitionKey = useGoodPartitionKey,
                    address = new Address
                    {
                        postcode = rnd.Next(2000, 3000),
                        state = States[rnd.Next(0, States.Length - 1)],
                        streetAddress = $"{rnd.Next(1, 50)} {Streets[rnd.Next(0, Streets.Length - 1)]}",
                        suburb = Suburbs[rnd.Next(0, Suburbs.Length - 1)]
                    }
    ,
                });
            }
            return data;
        }
    }
}
