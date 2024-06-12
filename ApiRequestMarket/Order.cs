
using WebApplication2;

public class Order
{
    public int id;
    public HashSet<Item> items = new HashSet<Item>();
    public string email;
    public string name;
    public string lastName;
    public string phone;
    public string postalCode;
    public string address;
    public string APM;
    public decimal totalPrice;
    public string status;
    

    public Order(int id, string email, string name, string lastName, string phone, string postalCode, string address, string APM,
        decimal totalPrice, string status, HashSet<Item> items)
    {
        this.id = id;
        this.email = email;
        this.name = name;
        this.lastName = lastName;
        this.phone = phone;
        this.postalCode = postalCode;
        this.address = address;
        this.APM = APM;
        this.totalPrice = totalPrice;
        this.status = status;
        this.items = items;
    }

    public Order(HashSet<Item> items, string email, string name, string lastName, string phone, string postalCode, string address,
        string APM,
        decimal totalPrice, string status)
    {
        this.items = items;
        this.email = email;
        this.name = name;
        this.lastName = lastName;
        this.phone = phone;
        this.postalCode = postalCode;
        this.address = address;
        this.APM = APM;
        this.totalPrice = totalPrice;
        this.status = status;
    }
}
