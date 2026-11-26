namespace MyFathersWorkApp;

public record LogBookEntry(string Title, string Location, string Summary);

// Log Book, ported from the official app (ViewLogBook.cs, Cost of Disease only there): an event is logged the first time one
// of its screens is shown, and the book lists the events in that order. Data: wwwroot/localization/<Scenario>_LogBook.csv
// (Cost of Disease from the official app, the other scenarios fan-written). Pages/Gameplay.razor notes the screen after it
// is rendered, never scenario code (mirror sandbox, undo replays). The logged titles live in ScenarioVars.LogBook: undo
// forgets the screens it goes back over, load and New Game behave like the rest of the game state (Unity cleared the log
// book on New Game too).
public class LogBookService(HttpClient http)
{
    private class ScenarioBook
    {
        public Dictionary<string, List<LogBookEntry>> ByScreen { get; } = new();
        public Dictionary<string, LogBookEntry>       ByTitle  { get; } = new();

        // Translations (AppLanguages.cs): language -> English title -> translated entry. The game state keeps the English
        // titles, so a change of language shows the same events.
        public Dictionary<string, Dictionary<string, LogBookEntry>> Translated { get; } = new();
    }

    private readonly Dictionary<ScenarioId, Task<ScenarioBook>> _Books = new();
    private readonly Dictionary<ScenarioId, ScenarioBook>       _Loaded = new();

    private static string? CsvPath(ScenarioId scenario) => scenario switch
    {
        ScenarioId.CostOfDisease => "localization/TheCostOfDisease_LogBook.csv",
        ScenarioId.FearOfUnknown => "localization/FearOfTheUnknown_LogBook.csv",
        ScenarioId.TimeOfWar     => "localization/ATimeOfWar_LogBook.csv",
        _                        => null
    };

    public static bool HasLogBook(GlobalData globalData) => CsvPath(globalData.ScenarioId) != null;

    public async Task LoadAsync(ScenarioId scenario)
    {
        string? path = CsvPath(scenario);
        if (path == null) return;
        if (!_Books.TryGetValue(scenario, out Task<ScenarioBook>? loading)) _Books[scenario] = loading = LoadCoreAsync(scenario, path);
        _Loaded[scenario] = await loading;
    }

    private async Task<ScenarioBook> LoadCoreAsync(ScenarioId scenario, string path)
    {
        ScenarioBook book = new();

        // A translation: localization/<code>/<same file name>, "Title;<translated title>;Location;Summary", keyed by the
        // English title
        foreach (AppLanguage translation in AppLanguages.Translations.Where(translation => translation.Scenarios.Contains(scenario)))
        {
            try
            {
                string csv = await http.GetStringAsync(translation.File(Path.GetFileNameWithoutExtension(path)));
                Dictionary<string, LogBookEntry> entries = new();

                foreach (string rawLine in csv.Split('\n'))
                {
                    string line = rawLine.TrimEnd('\r').TrimStart('\uFEFF');
                    if (line.Length == 0 || line.StartsWith("//")) continue;
                    string[] parts = line.Split(';', 4);
                    if (parts.Length < 4 || parts[0] == "Title") continue;
                    entries[parts[0]] = new LogBookEntry(parts[1], parts[2], parts[3]);
                }

                book.Translated[translation.Name] = entries;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Log Book ({translation.Name}): {e.Message}");
            }
        }

        try
        {
            string csv = await http.GetStringAsync(path);

            foreach (string rawLine in csv.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r').TrimStart('﻿');
                if (line.Length == 0 || line.StartsWith("//")) continue;

                // Screens;Title;Location;Summary (the header line is skipped)
                string[] parts = line.Split(';', 4);
                if (parts.Length < 4 || parts[0] == "Screens") continue;

                LogBookEntry entry = new(parts[1], parts[2], parts[3]);
                book.ByTitle[entry.Title] = entry;
                foreach (string screen in parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!book.ByScreen.TryGetValue(screen, out List<LogBookEntry>? entries)) book.ByScreen[screen] = entries = [];
                    entries.Add(entry);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Log Book: {e.Message}");
        }

        return book;
    }

    private ScenarioBook? Book(GlobalData globalData) => _Loaded.GetValueOrDefault(globalData.ScenarioId);

    // The entry of the screen on display (a window or popup, not a hub): its screen method (ResumeMethod, the caller of
    // SaveToUndo). A screen with several entries (the first / second night of the hunt) logs the one named in the window title.
    private LogBookEntry? CurrentEntry(GlobalData globalData, ScenarioBook book)
    {
        if (globalData.ActiveWindow == null && globalData.ActivePopup == null && globalData.ActiveInputPopup == null) return null;
        if (!book.ByScreen.TryGetValue(globalData.ResumeMethod, out List<LogBookEntry>? entries)) return null;
        if (entries.Count == 1) return entries[0];

        string title = globalData.ActiveWindow?.Title ?? string.Empty;
        return entries.FirstOrDefault(entry => title.Contains(entry.Title, StringComparison.OrdinalIgnoreCase)
                                            || title.Contains(Shown(globalData, book, entry).Title, StringComparison.OrdinalIgnoreCase)) ?? entries[0];
    }

    // The entry in the language the story is shown in
    private static LogBookEntry Shown(GlobalData globalData, ScenarioBook book, LogBookEntry entry) =>
        !globalData.IsStoryInEnglish && book.Translated.TryGetValue(globalData.ScenarioLanguage, out Dictionary<string, LogBookEntry>? translated)
                                     && translated.TryGetValue(entry.Title, out LogBookEntry? shown)
            ? shown
            : entry;

    // Logs the screen on display; each event once, as in Unity
    public void Note(GlobalData globalData)
    {
        if (globalData.ActiveMirrorQuestion != null || Book(globalData) is not { } book) return;

        LogBookEntry? entry = CurrentEntry(globalData, book);
        if (entry == null) return;

        ScenarioVars vars = globalData.ScenarioVars;
        if (!vars.LogBook.Contains(entry.Title)) vars.LogBook = [..vars.LogBook, entry.Title];
    }

    private static LogBookEntry WithVillage(LogBookEntry entry, string town) =>
        entry with { Location = entry.Location.Replace("Xvillage", town), Summary = entry.Summary.Replace("Xvillage", town) };

    // The logged events, oldest first. Like Unity, the event on display is left out while it is the newest one.
    public List<LogBookEntry> Entries(GlobalData globalData)
    {
        if (Book(globalData) is not { } book) return [];

        List<LogBookEntry> entries = globalData.ScenarioVars.LogBook
                                               .Where(title => book.ByTitle.ContainsKey(title))
                                               .Select(title => book.ByTitle[title])
                                               .ToList();

        if (entries.Count > 0 && CurrentEntry(globalData, book)?.Title == entries[^1].Title) entries.RemoveAt(entries.Count - 1);
        return entries.Select(entry => WithVillage(Shown(globalData, book, entry), globalData.TownName)).ToList();
    }
}
