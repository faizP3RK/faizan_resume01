using Newtonsoft.Json;

namespace Company.Function
{
    public class Counter
    {
        public string id { get; set; }
        public string PartitionKey { get; set; }
        public int Count { get; set; }
    }
   
}