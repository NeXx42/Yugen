namespace Yugen.Domain.Data;

public class SearchCriteria
{
    public required string[] genres { get; set; }
    public required LookupPair[] tags { get; set; }

    public class LookupPair
    {
        public int id { get; set; }
        public required string name { get; set; }
    }
}
