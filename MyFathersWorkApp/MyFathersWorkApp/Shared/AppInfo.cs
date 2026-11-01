namespace MyFathersWorkApp;

// The app version (from the assembly, set at build), shown at the bottom of Settings, the pause menu and Help
public static class AppInfo
{
    public static readonly string Version = Format(typeof(AppInfo).Assembly.GetName().Version);

    private static string Format(Version? v) => v is null ? "" : $"v{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
}
