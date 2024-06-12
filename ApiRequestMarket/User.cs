using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace ApiRequestMarket;

public class User
{
    public string email { get; set; }
    public string password { get; set; }
    public string apiKey { get; set; }
    public int access_level { get; set; }
    public User(string email)
    {
        this.email = email;
        access_level = Database.getUserAccessLevel(email);
        apiKey = Database.getUserSavedApiKey(email);
    }
    public User(string email, string password)
    {
        this.email = email;
        this.password = password;
        access_level = Database.getUserAccessLevel(email);
        apiKey = Database.getUserSavedApiKey(email);
    }
    public override bool Equals(object? obj)
    {
        return obj is User user && email == user.email;
    }
    public override int GetHashCode()
    {
        return email.GetHashCode();
    }

    public async Task registerUser()
    {
        await Database.RegisterInDatabase(email, Hash.HashPassword(password));
    }
}