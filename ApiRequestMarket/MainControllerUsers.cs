using System.Text;

namespace ApiRequestMarket;

public class MainControllerUsers
{
    public HashSet<User> userList = new HashSet<User>();
    private static string apiKey;
    private static string apiUrl;

    public MainControllerUsers()
    {
        var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json");
        IConfigurationRoot config = builder.Build();
        apiKey = config.GetValue<string>("ApiKey:Key");
        apiUrl = config.GetValue<string>("ApiUrl:Url");
    }
    public async Task addUserToList(string email)
    {
        var user = new User(email);
        userList.Add(user);
    }
    public async Task deleteUserFromList(string email)
    {
        var user = userList.FirstOrDefault(u => u.email == email);
        userList.Remove(user);
    }
    public async Task<User> getUserInfo(string email)
    {
        var user = userList.FirstOrDefault(u => u.email == email);
        if (user == null)
        {
            await addUserToList(email);
            user = userList.FirstOrDefault(u => u.email == email);
        }
        return user;
    }

    public static async Task sendReloadToApi()
    {
        string requestBody = "{\"command\": \"reload\"}";
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            content.Headers.ContentType.MediaType = "application/json";
            await client.PostAsync(apiUrl, content);
        }
    }
}