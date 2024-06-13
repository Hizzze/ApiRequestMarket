namespace ApiRequestMarket.Models;

public class UpdateViewModel
{
    public int id { get; set; }
    public string name { get; set; }
    public decimal price { get; set; }
    public int count { get; set; }
    public string path { get; set; }
    public string description { get; set; }
    public long categoryId { get; set; }
    public Dictionary<long, string> categories = new Dictionary<long, string>();
}