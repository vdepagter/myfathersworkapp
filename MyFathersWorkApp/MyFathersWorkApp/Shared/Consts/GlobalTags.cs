// ReSharper disable InconsistentNaming

namespace MyFathersWorkApp;

public static class GlobalTags
{
    // Main Menu
    public const string MainMenu_NewGame           = "MainMenu_NewGame";
    public const string MainMenu_LoadGame          = "MainMenu_LoadGame";
    public const string MainMenu_Continue          = "MainMenu_Continue";
    public const string MainMenu_Language          = "MainMenu_Language";
    public const string MainMenu_Back              = "MainMenu_Back";
    public const string MainMenu_PasteSaveDataHere = "MainMenu_PasteSaveDataHere";
    public const string MainMenu_FanMade           = "MainMenu_FanMade";

    // Players Select Screen
    public const string PlayersSelect_Title    = "PlayersSelect_Title";
    public const string PlayersSelect_SubTitle = "PlayersSelect_SubTitle";
    public const string PlayersSelect_Two      = "PlayersSelect_Two";
    public const string PlayersSelect_Three    = "PlayersSelect_Three";
    public const string PlayersSelect_Four     = "PlayersSelect_Four";

    // Players Initial Screen
    public const string PlayersInitial_Content = "PlayersInitial_Content";

    // Players Name Entry Screen
    public const string PlayersNameEntry_Title       = "PlayersNameEntry_Title";
    public const string PlayersNameEntry_SubTitle    = "PlayersNameEntry_SubTitle";
    public const string PlayersNameEntry_Placeholder = "PlayersNameEntry_Placeholder";
    public const string PlayersNameEntry_PopUp       = "PlayersNameEntry_PopUp";
    public const string PlayersNameEntry_PopUp2      = "PlayersNameEntry_PopUp2";

    // Players Welcome Screen
    public const string PlayersWelcome_First  = "PlayersWelcome_First";
    public const string PlayersWelcome_Second = "PlayersWelcome_Second";
    public const string PlayersWelcome_Third  = "PlayersWelcome_Third";
    public const string PlayersWelcome_Fourth = "PlayersWelcome_Fourth";

    // Village Screen
    public const string Village_Title       = "Village_Title";
    public const string Village_Subtitle    = "Village_Subtitle";
    public const string Village_Placeholder = "Village_Placeholder";
    public const string Village_PopUp       = "Village_PopUp";
    public const string Village_Remembered  = "Village_Remembered";

    // Village Confirm Screen
    public const string VillageConfirm_Content = "VillageConfirm_Content";

    // Scenario Screen
    public const string Scenario_Title                 = "Scenario_Title";
    public const string Scenario_Subtitle              = "Scenario_Subtitle";
    public const string Scenario_TheCostOfDisease      = "Scenario_TheCostOfDisease";
    public const string Scenario_TheCostOfDisease_Desc = "Scenario_TheCostOfDisease_Desc";
    public const string Scenario_FearOfTheUnknown      = "Scenario_FearOfTheUnknown";
    public const string Scenario_FearOfTheUnknown_Desc = "Scenario_FearOfTheUnknown_Desc";
    public const string Scenario_ATimeOfWar            = "Scenario_ATimeOfWar";
    public const string Scenario_ATimeOfWar_Desc       = "Scenario_ATimeOfWar_Desc";

    // Scenario Language Screen
    public const string ScenarioLanguage_Title    = "ScenarioLanguage_Title";
    public const string ScenarioLanguage_Subtitle = "ScenarioLanguage_Subtitle";
    public const string ScenarioLanguage_Mirror     = "ScenarioLanguage_Mirror";
    public const string ScenarioLanguage_MirrorDesc = "ScenarioLanguage_MirrorDesc";

    // Mirror Mode Popup
    public const string Mirror_Title       = "Mirror_Title";
    public const string Mirror_Question    = "Mirror_Question";
    public const string Mirror_Hidden      = "Mirror_Hidden";
    public const string Mirror_Number      = "Mirror_Number";
    public const string Mirror_NumberError = "Mirror_NumberError";
    public const string Mirror_Nothing     = "Mirror_Nothing";
    public const string Mirror_Roll        = "Mirror_Roll";

    // Audio Toggles
    public const string Audio_SoundEffects = "Audio_SoundEffects";
    public const string Audio_Music        = "Audio_Music";

    // Volume sliders (pause menu)
    public const string Audio_Volume = "Audio_Volume";
    public const string Audio_Speech = "Audio_Speech";

    // Voice-over: setting (scenario language screen, pause menu) and the indicator on narrated windows
    public const string Audio_Narration       = "Audio_Narration";
    public const string Audio_NarrationFemale = "Audio_NarrationFemale";
    public const string Audio_NarrationMale   = "Audio_NarrationMale";
    public const string Audio_NarrationOff    = "Audio_NarrationOff";
    public const string Audio_NarrationOliver = "Audio_NarrationOliver";
    public const string Narration_Narrated    = "Narration_Narrated";
    public const string Narration_Available   = "Narration_Available";
    public const string Narration_Starting    = "Narration_Starting";
    public const string Narration_Play        = "Narration_Play";
    public const string Narration_Replay      = "Narration_Replay";
    public const string Narration_Stop        = "Narration_Stop";
    public const string Narration_Back        = "Narration_Back";
    public const string Narration_Forward     = "Narration_Forward";
    public const string Narration_Position    = "Narration_Position";
    public const string Audio_NarrationToggle = "Audio_NarrationToggle";

    // Vote / bid countdown (Shared/RevealPanel.razor) and the special event banner
    public const string Reveal_StartVote   = "Reveal_StartVote";
    public const string Reveal_StartBid    = "Reveal_StartBid";
    public const string Reveal_DetailsVote = "Reveal_DetailsVote";
    public const string Reveal_DetailsBid  = "Reveal_DetailsBid";
    public const string Reveal_ShowVote    = "Reveal_ShowVote";
    public const string Reveal_ShowBid     = "Reveal_ShowBid";
    public const string Reveal_Word        = "Reveal_Word";
    public const string SpecialEvent_Title = "SpecialEvent_Title";

    // Ending page after THE END? (Pages/Gameplay.razor)
    public const string Ending_Next          = "Ending_Next";
    public const string Ending_Unlocked      = "Ending_Unlocked";
    public const string Ending_Recorded      = "Ending_Recorded";
    public const string Ending_UnlockedCount = "Ending_UnlockedCount";

    // Rankings podium (Shared/RankingsView.razor)
    public const string Rankings_Victorious     = "Rankings_Victorious";
    public const string Rankings_Family         = "Rankings_Family";
    public const string Rankings_Place          = "Rankings_Place";
    public const string Rankings_Points         = "Rankings_Points";
    public const string Rankings_TieMasterwork  = "Rankings_TieMasterwork";
    public const string Rankings_TieUpgrades    = "Rankings_TieUpgrades";
    public const string Rankings_FamilyNote     = "Rankings_FamilyNote";
    public const string Rankings_Continue       = "Rankings_Continue";

    // Gameplay
    public const string Gameplay_Yes                        = "Gameplay_Yes";
    public const string Gameplay_No                         = "Gameplay_No";
    public const string Gameplay_GameSaved                  = "Gameplay_GameSaved";
    public const string Gameplay_PauseMenu                  = "Gameplay_PauseMenu";
    public const string Gameplay_Resume                     = "Gameplay_Resume";
    public const string Gameplay_ReturnToMainMenu           = "Gameplay_ReturnToMainMenu";
    public const string Gameplay_LocalSaveGame              = "Gameplay_LocalSaveGame";
    public const string Gameplay_SaveGame                   = "Gameplay_SaveGame";
    public const string Gameplay_Copy                       = "Gameplay_Copy";
    public const string Gameplay_DownloadFile               = "Gameplay_DownloadFile";
    public const string Gameplay_ClickHereToContinue        = "Gameplay_ClickHereToContinue";
    public const string Gameplay_ClickHereContinueNextRound = "Gameplay_ClickHereContinueNextRound";
    public const string Gameplay_ClickHere                  = "Gameplay_ClickHere";
    public const string Gameplay_ClickHereForReward         = "Gameplay_ClickHereForReward";
    public const string Gameplay_OnceYouAreReady            = "Gameplay_OnceYouAreReady";
    public const string Gameplay_Setup                      = "Gameplay_Setup";
    public const string Gameplay_SpecialSetup               = "Gameplay_SpecialSetup";
    public const string Gameplay_SpecialRule                = "Gameplay_SpecialRule";
    public const string Gameplay_Accept                     = "Gameplay_Accept";
    public const string Gameplay_Confirm                    = "Gameplay_Confirm";
    public const string Gameplay_Choose                     = "Gameplay_Choose";
    public const string Gameplay_HandStorybookTo            = "Gameplay_HandStorybookTo";
    public const string Gameplay_DoNotOtherPlayersSee       = "Gameplay_DoNotOtherPlayersSee";
    public const string Gameplay_EarlyYears                 = "Gameplay_EarlyYears";
    public const string Gameplay_MiddleYears                = "Gameplay_MiddleYears";
    public const string Gameplay_LateYears                  = "Gameplay_LateYears";
    public const string Gameplay_Generation_I               = "Gameplay_Generation_I";
    public const string Gameplay_Generation_II              = "Gameplay_Generation_II";
    public const string Gameplay_Generation_III             = "Gameplay_Generation_III";
    public const string Gameplay_EndOfRound_Title           = "Gameplay_EndOfRound_Title";
    public const string Gameplay_EndOfRound_Content         = "Gameplay_EndOfRound_Content";
    public const string Gameplay_ClickHereAfterBid          = "Gameplay_ClickHereAfterBid";
    public const string Gameplay_ClickHereAfterVote         = "Gameplay_ClickHereAfterVote";
    public const string Gameplay_EndOfGeneration_Title      = "Gameplay_EndOfGeneration_Title";
    public const string Gameplay_ClickAtTheEndOfGeneration  = "Gameplay_ClickAtTheEndOfGeneration";
    public const string Gameplay_DrJr                       = "Gameplay_DrJr";
    public const string Gameplay_Dr                         = "Gameplay_Dr";
    public const string Gameplay_Third                      = "Gameplay_Third";

    // Endings Gallery
    public const string MainMenu_Gallery            = "MainMenu_Gallery";
    public const string PlayersNameEntry_Remembered = "PlayersNameEntry_Remembered";
    public const string PlayersNameEntry_SameTable  = "PlayersNameEntry_SameTable";
    public const string PlayersNameEntry_UseTable   = "PlayersNameEntry_UseTable";
    public const string Gallery_Players             = "Gallery_Players";
    public const string Gallery_Endings             = "Gallery_Endings";
    public const string Gallery_Player              = "Gallery_Player";
    public const string Gallery_Wins                = "Gallery_Wins";
    public const string Gallery_GamesPlayed         = "Gallery_GamesPlayed";
    public const string Gallery_Family              = "Gallery_Family";
    public const string Gallery_FamilyNote          = "Gallery_FamilyNote";
    public const string Gallery_RecentGames         = "Gallery_RecentGames";
    public const string Gallery_NoGames             = "Gallery_NoGames";
    public const string Gallery_Forget              = "Gallery_Forget";
    public const string Gallery_EndingsCount        = "Gallery_EndingsCount";
    public const string Gallery_ShowAll             = "Gallery_ShowAll";
    public const string Gallery_HideSpoilers        = "Gallery_HideSpoilers";
    public const string Gallery_Reveal              = "Gallery_Reveal";
    public const string Gallery_LockedEnding        = "Gallery_LockedEnding";
    public const string Gallery_LockedBranch        = "Gallery_LockedBranch";
    public const string Gallery_Ending              = "Gallery_Ending";
    public const string Gallery_Generation          = "Gallery_Generation";
    public const string Gallery_Played              = "Gallery_Played";
    public const string Gallery_TickedByHand        = "Gallery_TickedByHand";
    public const string Gallery_NotReached          = "Gallery_NotReached";
    public const string Gallery_Reached             = "Gallery_Reached";
    public const string Gallery_AlsoElsewhere       = "Gallery_AlsoElsewhere";
    public const string Gallery_TickOff             = "Gallery_TickOff";
    public const string Gallery_Untick              = "Gallery_Untick";
    public const string Gallery_Legend              = "Gallery_Legend";
    public const string Gallery_Backup              = "Gallery_Backup";
    public const string Gallery_BackupNote          = "Gallery_BackupNote";
    public const string Gallery_Export              = "Gallery_Export";
    public const string Gallery_Import              = "Gallery_Import";
    public const string Gallery_PasteHere           = "Gallery_PasteHere";
    public const string Gallery_ImportDone          = "Gallery_ImportDone";
    public const string Gallery_ImportFailed        = "Gallery_ImportFailed";
    public const string Gallery_NewEnding           = "Gallery_NewEnding";
    public const string Gallery_EndingRecorded      = "Gallery_EndingRecorded";
    public const string Gallery_ScenarioProgress    = "Gallery_ScenarioProgress";
    public const string Gallery_WinFor              = "Gallery_WinFor";
    public const string Gallery_FamilyWin           = "Gallery_FamilyWin";
    public const string Gallery_RecordedNote        = "Gallery_RecordedNote";

    // Saved Games (Pages/SavedGames.razor)
    public const string MainMenu_SavedGames   = "MainMenu_SavedGames";
    public const string SavedGames_Note       = "SavedGames_Note";
    public const string SavedGames_Current    = "SavedGames_Current";
    public const string SavedGames_Earlier    = "SavedGames_Earlier";
    public const string SavedGames_None       = "SavedGames_None";
    public const string SavedGames_Round      = "SavedGames_Round";
    public const string SavedGames_Ended      = "SavedGames_Ended";
    public const string SavedGames_NoHistory  = "SavedGames_NoHistory";
    public const string SavedGames_Open       = "SavedGames_Open";
    public const string SavedGames_SaveCode   = "SavedGames_SaveCode";
    public const string SavedGames_Delete     = "SavedGames_Delete";
    public const string SavedGames_SureDelete = "SavedGames_SureDelete";
    public const string SavedGames_OpenFailed = "SavedGames_OpenFailed";
    public const string SavedGames_Import       = "SavedGames_Import";
    public const string SavedGames_ImportNote   = "SavedGames_ImportNote";
    public const string SavedGames_ImportFile   = "SavedGames_ImportFile";
    public const string SavedGames_ImportFailed = "SavedGames_ImportFailed";

    // Help
    public const string MainMenu_Help = "MainMenu_Help"; // pages: Help_Tab_<Page> (tab label) and Help_<Page>, listed in Pages/Help.razor
    // Header buttons
    public const string Gameplay_Undo = "Gameplay_Undo";
    public const string Gameplay_Redo = "Gameplay_Redo";
    // Log Book (The Cost of Disease)
    public const string LogBook_Title    = "LogBook_Title";
    public const string LogBook_Open     = "LogBook_Open";
    public const string LogBook_Empty    = "LogBook_Empty";
    public const string LogBook_Page     = "LogBook_Page";
    public const string LogBook_Previous = "LogBook_Previous";
    public const string LogBook_Next     = "LogBook_Next";
    public const string LogBook_Close    = "LogBook_Close";

    // Settings (main menu and pause menu, Shared/SettingsPanel.razor)
    public const string Settings_Title          = "Settings_Title";
    public const string Settings_Graphics       = "Settings_Graphics";
    public const string Settings_GraphicsLow    = "Settings_GraphicsLow";
    public const string Settings_GraphicsMedium = "Settings_GraphicsMedium";
    public const string Settings_GraphicsHigh   = "Settings_GraphicsHigh";
    public const string Settings_TextSize       = "Settings_TextSize";
    public const string Settings_TextSizeSample = "Settings_TextSizeSample";
    public const string Settings_Close          = "Settings_Close";
    public const string MainMenu_Settings       = "MainMenu_Settings";
    public const string MainMenu_Endings        = "MainMenu_Endings"; // main menu button (Unity: "Endings"); the page is MainMenu_Gallery

    // Others
    public const string GameplayStartTag = "Gameplay_";
}