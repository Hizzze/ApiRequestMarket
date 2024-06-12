namespace ApiRequestMarket;

public class MainControllerUsers
{
    public HashSet<User> userList = new HashSet<User>();

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
}