using System.Text.Json;

namespace DatabaseLibrary;

public class LoginData
{
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
}

public static class AutoLogin
{
    private static string FilePath =>
        Path.Combine(AppContext.BaseDirectory, "user.json");

    public static void Save(string name, string password)
    {
        string json = JsonSerializer.Serialize(new LoginData { Name = name, Password = password });
        File.WriteAllText(FilePath, json);
    }

    public static LoginData? Load()
    {
        if (!File.Exists(FilePath))
            return null;
        return JsonSerializer.Deserialize<LoginData>(File.ReadAllText(FilePath));
    }

    public static void Logout()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
}
