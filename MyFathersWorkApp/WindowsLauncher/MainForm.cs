using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WindowsLauncher;

// The app window: WebView2 filling the client area, the app's dark background while it loads, size and position
// remembered between starts (window.json), F11 / the page's fullscreen request for full screen. Links to other sites
// open in the default browser.
internal sealed class MainForm : Form
{
    private static readonly Color Background = Color.FromArgb(0x11, 0x11, 0x11);

    private readonly string _Url;
    private readonly string _UserData;
    private readonly string _BoundsFile;
    private readonly WebView2 _View = new() { Dock = DockStyle.Fill };

    private FormWindowState _StateBeforeFullScreen;
    private bool _FullScreen;

    public MainForm(string title, string url, string userData, string dataDir)
    {
        _Url = url;
        _UserData = userData;
        _BoundsFile = Path.Combine(dataDir, "window.json");

        Text = title;
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        BackColor = Background;
        MinimumSize = new Size(480, 400);
        KeyPreview = true;
        _View.DefaultBackgroundColor = Background;
        Controls.Add(_View);
        RestoreBounds_();

        Load += async (_, _) => await StartAsync();
        FormClosing += (_, _) => SaveBounds();
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F11) { SetFullScreen(!_FullScreen); e.Handled = true; }
        };
    }

    public void BringToFrontNow()
    {
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    private async Task StartAsync()
    {
        try
        {
            var options = new CoreWebView2EnvironmentOptions { AreBrowserExtensionsEnabled = false };
            var environment = await CoreWebView2Environment.CreateAsync(null, _UserData, options);
            await _View.EnsureCoreWebView2Async(environment);
        }
        catch (Exception exception)
        {
            MessageBox.Show($"The app window could not start WebView2:\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        var core = _View.CoreWebView2;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.AreDevToolsEnabled = Debugger.IsAttached || Environment.GetEnvironmentVariable("MFW_DEVTOOLS") == "1";
        core.Settings.IsPasswordAutosaveEnabled = false;
        core.Settings.IsGeneralAutofillEnabled = false;
        core.DocumentTitleChanged += (_, _) => { }; // the window keeps the app's own name
        core.NewWindowRequested += (_, e) =>
        {
            e.Handled = true;
            OpenOutside(e.Uri);
        };
        core.NavigationStarting += (_, e) =>
        {
            if (!e.Uri.StartsWith(_Url, StringComparison.OrdinalIgnoreCase) && !e.Uri.StartsWith("blob:") && !e.Uri.StartsWith("data:"))
            {
                e.Cancel = true;
                OpenOutside(e.Uri);
            }
        };
        core.ContainsFullScreenElementChanged += (_, _) => SetFullScreen(core.ContainsFullScreenElement);
        core.Navigate(_Url);
    }

    private static void OpenOutside(string uri)
    {
        if (!uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return;
        try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
        catch { /* no default browser */ }
    }

    private void SetFullScreen(bool on)
    {
        if (on == _FullScreen) return;
        _FullScreen = on;
        if (on)
        {
            _StateBeforeFullScreen = WindowState;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal; // re-maximizing without a border covers the taskbar
            WindowState = FormWindowState.Maximized;
        }
        else
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = _StateBeforeFullScreen;
        }
    }

    private sealed record Saved(int X, int Y, int Width, int Height, bool Maximized);

    private void RestoreBounds_()
    {
        StartPosition = FormStartPosition.Manual;
        var screen = Screen.PrimaryScreen!.WorkingArea;
        var width = Math.Min(1280, screen.Width);
        var height = Math.Min(860, screen.Height);
        Bounds = new Rectangle(screen.X + (screen.Width - width) / 2, screen.Y + (screen.Height - height) / 2, width, height);
        try
        {
            if (!File.Exists(_BoundsFile)) return;
            var saved = JsonSerializer.Deserialize<Saved>(File.ReadAllText(_BoundsFile));
            if (saved == null || saved.Width < MinimumSize.Width || saved.Height < MinimumSize.Height) return;
            var rect = new Rectangle(saved.X, saved.Y, saved.Width, saved.Height);
            // Only where a screen still shows it (a monitor may have been unplugged)
            if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(rect)))
                Bounds = rect;
            if (saved.Maximized)
                WindowState = FormWindowState.Maximized;
        }
        catch { /* first start or unreadable: centred default */ }
    }

    private void SaveBounds()
    {
        try
        {
            var normal = WindowState == FormWindowState.Normal && !_FullScreen ? Bounds : RestoreBounds;
            Directory.CreateDirectory(Path.GetDirectoryName(_BoundsFile)!);
            File.WriteAllText(_BoundsFile, JsonSerializer.Serialize(new Saved(normal.X, normal.Y, normal.Width, normal.Height,
                                                                              WindowState == FormWindowState.Maximized || _FullScreen)));
        }
        catch { /* not important */ }
    }
}
