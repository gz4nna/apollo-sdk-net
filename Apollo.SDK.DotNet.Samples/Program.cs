namespace Apollo.SDK.DotNet.Samples;

public class Program
{
    public static void Main()
    {
        ApolloClient client = new();

        var path = Path.Combine(Environment.CurrentDirectory, "toggles");
        client.SetTogglesPath(path);

        var user_id = "user_123";

        var context = new Dictionary<string, object>
        {
            { "user_id", user_id},
            { "city", "Beijing"}
        };

        if (client.IsToggleAllow("smart_recommender_v2", user_id, context))
        {
            Console.WriteLine("is allow");
        }
    }
}


