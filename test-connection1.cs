using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=quizdb;Username=postgres;Password=ТВОЙ_ПАРОЛЬ";

try
{
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    Console.WriteLine("✅ PostgreSQL connection successful!");
    Console.WriteLine($"Server version: {conn.ServerVersion}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Connection failed: {ex.Message}");
}