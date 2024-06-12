
using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace ApiRequestMarket;

public class User
{
    public string email { get; set; }
    public User(string email)
    {
        this.email = email;
    }
    public override bool Equals(object? obj)
    {
        return obj is User user && email == user.email;
    }
    public override int GetHashCode()
    {
        return email.GetHashCode();
    }
}