namespace Fringe.Together.Web;

public static class WellKnown
{
    public static class Http
    {
        public const string Availability = "availability";
    }
    
    public static class Cosmos
    {
        public const string Database = "fringe-together";
        
        public static class Containers
        {
            public const string Availability = "availability";
            public const string Shows = "shows";
            public const string Profile = "profiles";
            public const string Interested = "interested";
        }
    }
}
