using System.Text.RegularExpressions;

namespace MyFathersWorkApp;

// Languages besides English (docs/i18n/README.md). English stays in the original CSVs (wwwroot/localization/*.csv, which
// the narration tools read); a translation lives in its own folder with the same file names and the header "Tag;<Name>",
// e.g. localization/cs/UI_Localization.csv. A tag missing from a translation falls back to English, so a file can be
// translated a bit at a time. Scenarios lists the scenarios whose story is translated: for the others the story stays in
// English while the menus follow the chosen language.
public record AppLanguage(string Name, string Code, ScenarioId[] Scenarios, bool Gender)
{
    public string File(string baseName) => $"localization/{Code}/{baseName}.csv";
}

public static class AppLanguages
{
    public const string ENGLISH = "English";

    // Column name of the English story in the scenario CSVs (GlobalData.ScenarioLanguage)
    public const string ENGLISH_STORY = "English_Source";

    public static readonly AppLanguage[] Translations =
    [
        // Czech: past tense and adjectives agree with the player's gender (PlayersNameEntry asks for it, [[g:...]] tokens)
        new("Czech", "cs", [ScenarioId.CostOfDisease], Gender: true)
    ];

    public static AppLanguage? Get(string name) => Translations.FirstOrDefault(language => language.Name == name);

    public static bool HasStory(string language, ScenarioId scenario) => Get(language)?.Scenarios.Contains(scenario) == true;

    // Browser language -> app language for the first start (no choice stored yet)
    public static string FromBrowser(string? browserLanguage)
    {
        if (string.IsNullOrEmpty(browserLanguage)) return ENGLISH;
        string code = browserLanguage.Split('-')[0].ToLowerInvariant();
        return Translations.FirstOrDefault(language => language.Code == code)?.Name ?? ENGLISH;
    }
}

// Grammar tokens of translated text, resolved when the text is shown (Extensions.ProtectNames), after the scenario code
// has filled in its placeholders:
//   [[pl:one/few/many]]  Czech-style plural (1 / 2-4 / 5+, 0 and 11-14 take "many") by the nearest number before it.
//   [[g0:male/female]]   gender of the player whose name fills {{0=...}} in the same CSV text; FormatWithReplacement
//                        turns it into [[g=<name>:male/female]] when it fills in the name.
//   [[g:male/female]]    gender of the nearest player name before it in the shown text (male when there is none).
// The separator is "/" because "|" belongs to the {{..}} placeholders, so tokens may sit inside placeholder options.
public static class Grammar
{
    public const char NAME_MARK = '\u001F'; // around the name inside [[g=...:...]]

    // [[gN:...]] whose placeholder the code never filled in is treated like [[g:...]]
    private static readonly Regex _Token  = new(@"\[\[(pl|g=\u001F[^\u001F]*\u001F|g\d?):([^\]]*)\]\]");
    private static readonly Regex _Tags   = new("<[^>]*>");
    private static readonly Regex _Number = new(@"\d+");

    // Called by FormatWithReplacement for the placeholder it fills in
    public static string BindGender(string text, int id, string name)
    {
        string token = $"[[g{id}:";
        return text.Contains(token) ? text.Replace(token, $"[[g={NAME_MARK}{name}{NAME_MARK}:") : text;
    }

    public static string Resolve(string text, GlobalData globalData)
    {
        if (!text.Contains("[[")) return text;

        return _Token.Replace(text, match =>
        {
            string[] forms = match.Groups[2].Value.Split('/');
            string   kind  = match.Groups[1].Value;
            string   before = _Tags.Replace(text[..match.Index], " ");

            if (kind == "pl")
            {
                MatchCollection numbers = _Number.Matches(before);
                long n = numbers.Count > 0 && long.TryParse(numbers[^1].Value, out long value) ? value : 5;
                return forms.Length < 3 ? forms[^1] : forms[PluralIndex(n)];
            }

            string? name = kind.StartsWith("g=") ? kind[3..^1] : NearestPlayer(before, globalData);
            bool female = name != null && globalData.IsFemale(name);
            return forms.Length < 2 ? forms[0] : forms[female ? 1 : 0];
        });
    }

    // Czech: 1 -> one, 2-4 -> few, otherwise many
    public static int PluralIndex(long n) => n == 1 ? 0 : n is >= 2 and <= 4 ? 1 : 2;

    private static string? NearestPlayer(string before, GlobalData globalData)
    {
        string? best = null;
        int bestIndex = -1;

        foreach (string name in globalData.GetActivePlayers().Where(name => !string.IsNullOrWhiteSpace(name)))
        {
            int index = before.LastIndexOf(name, StringComparison.Ordinal);
            if (index > bestIndex || (index == bestIndex && index >= 0 && name.Length > best!.Length))
            {
                bestIndex = index;
                best      = name;
            }
        }

        return bestIndex >= 0 ? best ?? ScreenPlayer(globalData) : ScreenPlayer(globalData);
    }

    // [[g:...]] in a text without a player name before it (first-person journals, sworn statements): the first player
    // named anywhere on the screen on display (title, the other paragraphs, the popup)
    private static string? ScreenPlayer(GlobalData globalData)
    {
        IEnumerable<string> texts = globalData.ActivePopup != null
            ? [globalData.ActivePopup.Title, globalData.ActivePopup.Description]
            : globalData.ActiveWindow != null
                ? new[] { globalData.ActiveWindow.Title, globalData.ActiveWindow.Subtitle }.Concat(globalData.ActiveWindow.Elements.Select(element => element.Content))
                : [];
        string screen = string.Join(" ", texts);

        return globalData.GetActivePlayers()
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Select(name => (name, index: screen.IndexOf(name, StringComparison.Ordinal)))
                         .Where(found => found.index >= 0)
                         .OrderBy(found => found.index)
                         .Select(found => found.name)
                         .FirstOrDefault();
    }

    private static readonly Regex _ShortWord = new(@"(?<=^|[\s(„])([vszkouaiVSZKOUAI]) ");

    // Czech typography: no line break after a one-letter preposition or conjunction (v, s, z, k, o, u, a, i); text
    // outside HTML tags only
    public static string NoBreakAfterShortWords(string text)
    {
        if (text.Length == 0) return text;
        string[] parts = Regex.Split(text, "(<[^>]*>)");
        for (int i = 0; i < parts.Length; i++)
        {
            if (!parts[i].StartsWith('<')) parts[i] = _ShortWord.Replace(parts[i], "$1 ");
        }

        return string.Concat(parts);
    }
}
