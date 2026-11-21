namespace WindowsLauncher;

// Talking to an instance that already runs (a second start of the exe)
internal static class Launcher
{
    public const string PingPath  = "/__mfw-launcher";
    public const string ShowPath  = "/__mfw-launcher/show";
    public const string PingReply = "my-fathers-work";

    public static async Task<bool> AskRunningToShowAsync(int port)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            if (await http.GetStringAsync($"http://127.0.0.1:{port}{PingPath}") != PingReply)
                return false;
            await http.PostAsync($"http://127.0.0.1:{port}{ShowPath}", null);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
