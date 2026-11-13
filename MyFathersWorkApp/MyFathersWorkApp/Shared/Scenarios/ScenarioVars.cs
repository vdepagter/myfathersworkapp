using System.Security.Cryptography;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace MyFathersWorkApp;

// State shared by every scenario: the pre-rolled random values and the final scoring. Serialized with the scenario vars.
public abstract class ScenarioVars(int randomCount)
{
    // -1 = not drawn yet (mirror mode asks the players what the official app shows)
    public int[] RandomArray { get; set; } = new int[randomCount];

    // Seed for flavor text (letter authors, dates, alternative sentences) - never asked in mirror mode
    public int FlavorSeed { get; set; }

    // Final Scoring
    public Dictionary<string, int> Scores       { get; set; } = new();
    public string[]                TiedPlayers  { get; set; } = [];
    public string[]                TieMasterworks { get; set; } = [];
    public int                     TieIndex     { get; set; }
    public int[]                   TieUpgrades  { get; set; } = []; // Estate Upgrades per TieMasterworks entry
    public bool                    FamilyWinner { get; set; }
    public string                  Winner       { get; set; } = string.Empty;
    public string[]                Masterworks  { get; set; } = []; // players who completed their Masterwork (ticked at score entry)

    // Log Book: titles of the logged events in the order they were first shown. Written by Shared/LogBook.cs after a screen
    // is rendered, never by scenario code; kept here so undo forgets the screens it goes back over and load keeps the rest.
    public string[] LogBook { get; set; } = [];

    protected GlobalData _GlobalData = null!;

    // Names the players typed in during play; the page marks them translate="no" (GlobalData.ProtectedNames)
    public virtual IEnumerable<string> TypedNames() => [];

    public bool RandomBool(int index)
    {
        return RandomSlot(index, 2, value => value == 1 ? "true" : "false") == 1;
    }

    public T RandomElement<T>(List<T> list, int index)
    {
        return list[RandomSlot(index, list.Count, value => MirrorValueLabel(list[value]))];
    }

    public T RandomElement<T>(List<T> list, string playerName, int startIndex)
    {
        int index                                        = startIndex;
        if (playerName == _GlobalData.PlayerBName) index += 1;
        if (playerName == _GlobalData.PlayerCName) index += 2;
        if (playerName == _GlobalData.PlayerDName) index += 3;

        return RandomElement(list, index);
    }

    public int RandomInRange(int min, int max, int index)
    {
        return min + RandomSlot(index, max - min + 1, value => (min + value).ToString(), min);
    }

    // Returns RandomArray[index] % count. In mirror mode an unanswered slot (-1) asks the players what the official app shows.
    private int RandomSlot(int index, int count, Func<int, string> valueLabel, int? numericMin = null)
    {
        if (count <= 1) return 0;

        int value = RandomArray[index];
        if (value >= 0) return value % count;

        if (!_GlobalData.MirrorMode)
        {
            RandomArray[index] = RandomNumberGenerator.GetInt32(int.MaxValue);
            return RandomArray[index] % count;
        }

        if (_GlobalData.IsMirrorSimulating) return _GlobalData.MirrorSimulationValue % count;
        throw new MirrorQuestionException(index, count, valueLabel, numericMin);
    }

    private static string MirrorValueLabel<T>(T value)
    {
        return value is Delegate callback ? callback.Method.Name : value?.ToString() ?? string.Empty;
    }

    // Deterministic pick for flavor text: stable for undo / load, independent of RandomArray and mirror mode
    public int Flavor(string key, int count)
    {
        if (count <= 1) return 0;

        uint hash = 2166136261;

        foreach (char c in FlavorSeed + ":" + key)
        {
            hash ^= c;
            hash *= 16777619;
        }

        return (int)(hash % (uint)count);
    }

    public T FlavorElement<T>(List<T> list, string key)
    {
        return list[Flavor(key, list.Count)];
    }

    protected void ResetBase(GlobalData globalData)
    {
        _GlobalData = globalData;

        TiedPlayers    = [];
        TieMasterworks = [];
        TieIndex       = 0;
        TieUpgrades    = [];
        FamilyWinner   = false;
        Winner         = string.Empty;
        Masterworks    = [];
        LogBook        = [];

        foreach (string player in AllPlayerSlots(globalData)) Scores[player] = 0;

        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        for (int x = 0; x < RandomArray.Length; ++x)
        {
            byte[] box = new byte[4];
            rng.GetBytes(box);
            RandomArray[x] = globalData.MirrorMode ? -1 : BitConverter.ToInt32(box, 0) & int.MaxValue;
        }

        FlavorSeed = RandomNumberGenerator.GetInt32(int.MaxValue);
    }

    // Per-player dictionaries are keyed by the four name slots (unused slots are empty strings)
    protected static string[] AllPlayerSlots(GlobalData globalData)
    {
        return [globalData.PlayerAName, globalData.PlayerBName, globalData.PlayerCName, globalData.PlayerDName];
    }
}
