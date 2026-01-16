namespace Apollo.SDK.NET.Samples;

public class Program
{
    public static void Main()
    {
        ApolloClient client = new(new ApolloOptions
        {
            TogglesPath = Path.Combine(Environment.CurrentDirectory, "toggles")
        });

        var context = new ApolloContext("user_123")
            .Set("city", "Beijing");

        if (client.IsToggleAllowed("smart_recommender_v2", context))
        {
            Console.WriteLine("is allow");
        }
    }
}


