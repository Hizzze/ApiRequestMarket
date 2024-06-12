using ApiRequestMarket.Controllers;
using MySqlConnector;
namespace WebApplication2;

public class Database
{
    private static string connectionString;

    static Database()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot config = builder.Build();
        connectionString = config.GetConnectionString("DefaultConnection");
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

    public static async Task DeleteUserCart(string email)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM users_carts WHERE email = @email";
                    command.Parameters.AddWithValue("@email", email);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on deleting user cart DB: " + ex.Message);
                throw;
            }
        }
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

    public static async Task<bool> CreateOrderWithItemsAsync(string email, string name, string lastName, string phone,
        string postalCode, string address, string APM, decimal totalPrice, string status, HashSet<Item> items)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                int orderId;
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "INSERT INTO orders (email, user_first_name, user_last_name, " +
                                                  "phone, postal_code, address, APM, total_price, status) " +
                                                  "VALUES (@value1, @value2, @value3, @value4, @value5, @value6, @value7, @value8, @value9)";
                            command.Parameters.AddWithValue("@value1", email);
                            command.Parameters.AddWithValue("@value2", name);
                            command.Parameters.AddWithValue("@value3", lastName);
                            command.Parameters.AddWithValue("@value4", phone);
                            command.Parameters.AddWithValue("@value5", postalCode);
                            command.Parameters.AddWithValue("@value6", address);
                            command.Parameters.AddWithValue("@value7", APM);
                            command.Parameters.AddWithValue("@value8", totalPrice);
                            command.Parameters.AddWithValue("@value9", status);
                            await command.ExecuteNonQueryAsync();
                            orderId = (int)command.LastInsertedId;
                        }


                        foreach (var item in items)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = @"INSERT INTO orders_items (order_id, item_id, item_count) 
                                                VALUES (@orderId, @itemId, @itemCount)";
                                command.Parameters.AddWithValue("@orderId", orderId);
                                command.Parameters.AddWithValue("@itemId", item.getId());
                                command.Parameters.AddWithValue("@itemCount", item.getUserCount());
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        foreach (var item in items)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = @"UPDATE items SET count = count - @value1 WHERE id = @value2";
                                command.Parameters.AddWithValue("@value1", item.getUserCount());
                                command.Parameters.AddWithValue("@value2", item.getId());
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Logger.LogAsync("Error inside transaction: " + ex.Message, Logger.LogLevel.Error);
                        await transaction.RollbackAsync();
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error creating order with items: " + ex.Message, Logger.LogLevel.Error);
                return false;
            }
        }
    }

    public static async Task removeFromUserCart(string email, int id)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM users_carts WHERE email = @value1 && id = @value2";
                    command.Parameters.AddWithValue("@value1", email);
                    command.Parameters.AddWithValue("@value2", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on removing from cart DB: " + ex.Message, Logger.LogLevel.Error);
            }
        }
    }
    public static async Task<HashSet<Item>> getUserCart(string email)
    {
        HashSet<Item> userCart = new HashSet<Item>();
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, count, price FROM users_carts WHERE email = @value1";
                    command.Parameters.AddWithValue("@value1", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new Item(reader.GetInt32("id"), reader.GetDecimal("price"),
                                reader.GetInt32("count"));
                            userCart.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on getting user cart DB: " + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
        return userCart;
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
                            if (email.Equals(temp) && pass.Equals(Hash.Hash.HashPassword(password)))
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
    public static List<Item> getItemsList()
    {
        List<Item> items = new List<Item>();
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, price, count, path, description FROM items";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new Item(reader.GetInt32(0),reader.GetString(1), reader.GetDecimal(2), reader.GetInt32(3),
                                reader.GetString(4), reader.GetString(5)));
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

    public static async Task addItemToCart(string email, int id, int count, decimal price)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText =
                            "INSERT INTO users_carts (email, id, count, price) VALUES (@value1, @value2,@value3,@value4)";
                        command.Parameters.AddWithValue("@value1", email);
                        command.Parameters.AddWithValue("@value2", id);
                        command.Parameters.AddWithValue("@value3", count);
                        command.Parameters.AddWithValue("@value4", price);
                        await command.ExecuteNonQueryAsync();
                    }
                
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on add item to cart DB:" + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
    }
    
    
    public static async Task updateItemInCart(string email, int id, int count)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE users_carts SET count = @value1 WHERE email = @value2 && id = @value3";
                    command.Parameters.AddWithValue("@value1", count);
                    command.Parameters.AddWithValue("@value2", email);
                    command.Parameters.AddWithValue("@value3", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on update item in cart DB:" + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
    }

    /*public static async Task<HashSet<Order>> getOrdersForUser(string email)
    {
        HashSet<Order> orders = new HashSet<Order>();
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT id,email, user_first_name, user_last_name, phone, postal_code, address, APM, " +
                        "item_name, item_count, total_price, status FROM orders WHERE email = @value1";
                    command.Parameters.AddWithValue("@value1", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order(reader.GetInt32("id"), reader["email"].ToString(),
                                reader["user_first_name"].ToString(), reader["user_last_name"].ToString(),
                                reader["phone"].ToString(),
                                reader["postal_code"].ToString(), reader["address"].ToString(),
                                reader["APM"].ToString(),
                                reader.GetDecimal("total_price"),
                                reader["status"].ToString()), new Item());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on getting orders for user DB: " + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
        return orders;
    }*/
    public static async Task<List<Order>> getOrdersForUser(string email)
{
    List<Order> orders = new List<Order>();
    using (var connection = new MySqlConnection(connectionString))
    {
        try
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT o.id, o.email, o.user_first_name, o.user_last_name, o.phone, " +
                    "o.postal_code, o.address, o.APM, o.total_price, o.status, i.item_id, i.item_count " +
                    "FROM orders o " +
                    "JOIN orders_items i ON o.id = i.order_id " +
                    "WHERE o.email = @value1";
                command.Parameters.AddWithValue("@value1", email);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int orderId = reader.GetInt32("id");
                        string orderEmail = reader.GetString("email");
                        string firstName = reader.GetString("user_first_name");
                        string lastName = reader.GetString("user_last_name");
                        string phone = reader.GetString("phone");
                        string postalCode = reader.GetString("postal_code");
                        string address = reader.GetString("address");
                        string apm = reader.GetString("APM");
                        decimal totalPrice = reader.GetDecimal("total_price");
                        string status = reader.GetString("status");
                        Order order = new Order(orderId, orderEmail, firstName, lastName, phone, postalCode, address, apm, totalPrice, status, new HashSet<Item>());
                        int id = reader.GetInt32("item_id");
                        int itemCount = reader.GetInt32("item_count");
                        Item item = new Item(id, itemCount);
                        item = await MainControllerItems.buildItem(item);
                        order.items.Add(item);
                        orders.Add(order);
                    }
                }
            }
            return orders;
        }
        catch (Exception ex)
        {
            await Logger.LogAsync("Error on getting orders for user DB: " + ex.Message, Logger.LogLevel.Error);
            throw;
        }
    }
    return orders;
}

    public static async Task updateUserInfo(string email, string name, string lastName, string phone, string address,
        string postalCode, string APM)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE users SET name = @name, last_name = @lastName, phone = @phone," +
                                          "postalcode = @postalCode, address = @address, apm = @apm WHERE email = @email ";
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@lastName", lastName);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@postalCode", postalCode);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@apm", APM);
                    command.Parameters.AddWithValue("@email", email);
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on updating user info DB: " + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
    }
    public static async Task<(string?, string?, string?, string?, string?, string?)> getUserInfoDatabase(string email)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name, last_name, phone, address, postalcode, APM FROM users WHERE email = @value1";
                    command.Parameters.AddWithValue("@value1", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return (reader.IsDBNull(reader.GetOrdinal("name")) ? "null" : reader["name"].ToString(),
                                reader.IsDBNull(reader.GetOrdinal("last_name")) ? "null" : reader["last_name"].ToString(),
                                reader.IsDBNull(reader.GetOrdinal("phone")) ? "null" : reader["phone"].ToString(),
                                reader.IsDBNull(reader.GetOrdinal("address")) ? "null" : reader["address"].ToString(),
                                reader.IsDBNull(reader.GetOrdinal("postalcode")) ? "null" : reader["postalcode"].ToString(),
                                reader.IsDBNull(reader.GetOrdinal("APM")) ? "null" : reader["APM"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on getting user info DB: " + ex.Message, Logger.LogLevel.Error);
            }
        }

        return ("null", "null", "null", "null", "null", "null");
    }
    /*public static async Task getAccountsList()
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT "
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync("Error on getting accounts list: " + ex.Message, Logger.LogLevel.Error);
                throw;
            }
        }
    }*/
}