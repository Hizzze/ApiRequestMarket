using MySqlConnector;

namespace ApiRequestMarket;

public class Database
{
    private static string connectionString;
    private static string connectionString2;
    static Database()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot config = builder.Build();
        connectionString = config.GetConnectionString("DefaultConnection");
        connectionString2 = config.GetConnectionString("APIConnection");
    }

    public static async Task DeleteItemFromDatabase(int id)
    {
        using (var connection = new MySqlConnection(connectionString2))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM items WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public static async Task<Dictionary<long,string>> GetCategories()
    {
        Dictionary<long, string> dict = new Dictionary<long, string>();
        using (var connection = new MySqlConnection(connectionString2))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT category_id, category_name FROM categories";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dict[reader.GetInt64(0)] = reader.GetString(1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return dict;
    }

    public static async Task<bool> AddNewItem(string name, decimal price, int count, string path, string description, long categoryId)
    {
        using (var connection = new MySqlConnection(connectionString2))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO items (name, price, count, path, description, category_id) " +
                                          "VALUES (@name, @price, @count, @path, @description, @category_id)";
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@count", count);
                    command.Parameters.AddWithValue("@path", path);
                    command.Parameters.AddWithValue("@description", description);
                    command.Parameters.AddWithValue("@category_id", categoryId);
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
                throw;
            }
        }
    }

    public static async Task<Item> GetItemById(long id)
    {
        using (var connection = new MySqlConnection(connectionString2))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT id, name, price, count, path, description, category_id FROM items WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return new Item(reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2),
                            reader.GetInt32(3),
                            reader.GetString(4), reader.GetString(5), reader.GetInt64(6));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return null;
    }

    public static async Task<bool> UpdateItem(int id, string name, decimal price, int count, string path, string description, long category_id)
    {
        using (var connection = new MySqlConnection(connectionString2))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE items SET name = @name, price = @price, count = @count, path = @path," +
                        " description = @description, category_id = @category_id WHERE id = @id";
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@count", count);
                    command.Parameters.AddWithValue("@path", path);
                    command.Parameters.AddWithValue("@description", description);
                    command.Parameters.AddWithValue("@category_id", category_id);
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
                Console.WriteLine(e);
                throw;
            }
        }
    }
    public static async Task<bool> IsUserRegistered(string email)
    {

        using (var connection = new MySqlConnection(connectionString))
        {
            bool exists = false;
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(1) FROM accounts WHERE email = @value1";
                    command.Parameters.AddWithValue("@value1", email);
                    exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on check registration: " + ex.Message, Logger.LogLevel.Error);
            }

            return exists;
        }
    }
    public static List<Item> getItemsList()
    {
        List<Item> items = new List<Item>();
        using (var connection = new MySqlConnection(connectionString2))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, price, count, path, description, category_id FROM items";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new Item(reader.GetInt32(0),reader.GetString(1), reader.GetDecimal(2), reader.GetInt32(3),
                                reader.GetString(4), reader.GetString(5), reader.GetInt64(6)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error on get items: " + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }

        return items;
    }
    public static async Task RegisterInDatabase(string email, string password)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO accounts (email, password) VALUES(@value1, @value2)";
                    command.Parameters.AddWithValue("@value1", email);
                    command.Parameters.AddWithValue("@value2", password);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on registration: " + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
    }

    public static int getUserAccessLevel(string email)
    {
        int access_level;
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT access_level FROM accounts WHERE email = @email";
                    command.Parameters.AddWithValue("@email", email);
                    var result = command.ExecuteScalar();
                    access_level = Convert.ToInt32(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return access_level;
    }

    public static string getUserSavedApiKey(string email)
    {
        string apiKey = "null";
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT api_key FROM accounts WHERE email = @email";
                    command.Parameters.AddWithValue("@email", email);
                    var result = command.ExecuteScalar();
                    apiKey = result == null ? "null" : result.ToString();
                }
            }
            catch (Exception e)
            {
                
            }
        }
        return apiKey;
    }
    public static async Task<bool> verifyUserData(string email, string password)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT email, password FROM accounts WHERE email = @value1";
                    command.Parameters.AddWithValue("@value1", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string temp = reader["email"].ToString();
                            string pass = reader["password"].ToString();
                            if (email.Equals(temp) && pass.Equals(Hash.HashPassword(password)))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on verify user: " + ex.Message, Logger.LogLevel.Error);
                return false;
            }
        }

        return false;
    }
}