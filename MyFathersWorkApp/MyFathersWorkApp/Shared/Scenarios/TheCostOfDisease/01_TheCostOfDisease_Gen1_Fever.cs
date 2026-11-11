namespace MyFathersWorkApp;

public static partial class TheCostOfDisease
{
    public static void Fever(GlobalData globalData)
    {
        // Round 1 -> Fever1
        // Round 2 -> Fever2
        // Round 3 -> Fever3
        globalData.TheCostOfDiseaseVars.HubId = CostOfDiseaseHubId.Fever;
        globalData.ActiveHub                  = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       hospitalSection = "Hospital";
        GameplayHubSection hospital        = globalData.ActiveHub.AddSection(hospitalSection, true);
        hospital.AddDefaultContent(hospitalSection);
        hospital.AddNextContent(1, hospitalSection);

        const string       bankSection = "Bank";
        GameplayHubSection bank        = globalData.ActiveHub.AddSection(bankSection);
        bank.ReplaceShouldShow(() => globalData.TheCostOfDiseaseVars.Building == BankOrLibrary.Bank);
        bank.AddDefaultContent(bankSection);

        const string       librarySection = "Library";
        GameplayHubSection library        = globalData.ActiveHub.AddSection(librarySection);
        library.ReplaceShouldShow(() => globalData.TheCostOfDiseaseVars.Building == BankOrLibrary.Library);
        library.AddDefaultContent(librarySection);

        const string       creepySection = "Creepy";
        GameplayHubSection creepy        = globalData.ActiveHub.AddSection(creepySection);
        creepy.ReplaceShouldShow(() => !globalData.TheCostOfDiseaseVars.Creepy4);
        creepy.AddDefaultContent(creepySection);
        creepy.AddClickHere(Gen1CreepyTrack);

        const string       saneSection = "Sane";
        GameplayHubSection sane        = globalData.ActiveHub.AddSection(saneSection);
        sane.ReplaceShouldShow(() => !globalData.TheCostOfDiseaseVars.Sane3);
        sane.AddDefaultContent(saneSection);
        sane.AddClickHere(Gen1SanityTrack);

        const string       endSection     = "End";
        GameplayHubSection endEarlyMiddle = globalData.ActiveHub.AddSection(endSection);
        endEarlyMiddle.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        endEarlyMiddle.AddDefaultContent(endSection);
        endEarlyMiddle.AddClickHereContinueNextRound(Fever_0, true);

        globalData.ActiveHub.AddEndOfGenerationSection(Fever_0);
    }

    private static void Fever_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => FeverServe1,
            Years.Middle => S5Special1,
            Years.Late   => FeverHeart,
            _            => _ => { }
        });
    }

    #region Gen1CreepyTrack

    private static void Gen1CreepyTrack(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(1, true);
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(creepName =>
        {
            globalData.TheCostOfDiseaseVars.Gen1Creep = creepName;
            globalData.TheCostOfDiseaseVars.Creepy4   = true;
            Gen1CreepyTrackRes(globalData);
        });
    }

    private static void Gen1CreepyTrackRes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(globalData.TheCostOfDiseaseVars.Gen1Creep, Gen1CreepyTrackRes_0);
    }

    private static void Gen1CreepyTrackRes_0(GlobalData globalData)
    {
        if (globalData.TheCostOfDiseaseVars.Hunters == Affiliation.Evil) Gen1CreepyTrackRes_1(globalData);
        else Gen1CreepyTrackRes_2(globalData);
    }

    private static void Gen1CreepyTrackRes_1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Creep));
        globalData.ActiveWindow.AddChoose();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Gen1CreepyYes, Gen1CreepyNo], true);
    }

    private static void Gen1CreepyTrackRes_2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Creep));
        globalData.ActiveWindow.AddChoose();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Gen1CreepyYes, Gen1CreepyNo], true);
    }

    private static void Gen1CreepyYes(GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Seedy = ExtendedBool.True;
        if (globalData.TheCostOfDiseaseVars.Hunters == Affiliation.Evil) Gen1CreepyYes_A(globalData);
        else Gen1CreepyYes_B(globalData);
    }

    private static void Gen1CreepyYes_A(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Gen1CreepyYes_A0);
    }

    private static void Gen1CreepyYes_A0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Money_Icon, PopUpButton.Accept, _ => { });
    }

    private static void Gen1CreepyYes_B(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Creep));
        globalData.ActiveWindow.AddClickHereToContinue(Gen1CreepyYes_B0);
    }

    private static void Gen1CreepyYes_B0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.MFWlogo, PopUpButton.Accept, _ => { });
    }

    private static void Gen1CreepyNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Seedy = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Creep));
        globalData.ActiveWindow.AddClickHereToContinue(Gen1CreepyNo_0);
    }

    private static void Gen1CreepyNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Accept, _ => { },
            content => content.FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.RandomBool(3)));
    }

    #endregion Gen1CreepyTrack

    #region Gen1SanityTrack

    private static void Gen1SanityTrack(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(1, true);
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(saneName =>
        {
            globalData.TheCostOfDiseaseVars.Gen1Sane = saneName;
            globalData.TheCostOfDiseaseVars.Sane3    = true;
            Gen1SanityTrackRes(globalData);
        });
    }

    private static void Gen1SanityTrackRes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(globalData.TheCostOfDiseaseVars.Gen1Sane, Gen1SanityTrackRes_0);
    }

    private static void Gen1SanityTrackRes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Sane));
        globalData.ActiveWindow.AddChoose();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Gen1SanityYes, Gen1SanityNo], true);
    }

    private static void Gen1SanityYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Vacation = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Sane));
        globalData.ActiveWindow.AddClickHereToContinue(Gen1SanityYes_0);
    }

    private static void Gen1SanityYes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.AnyKnowledge_Icon, PopUpButton.Accept, _ => { });
    }

    private static void Gen1SanityNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.TheCostOfDiseaseVars.Vacation = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Gen1Sane));
        globalData.ActiveWindow.AddClickHereToContinue(Gen1SanityNo_0);
    }

    private static void Gen1SanityNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Accept, _ => { },
            content => content.FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.RandomBool(4)));
    }

    #endregion Gen1SanityTrack

    #region End of First The Round

    private static void FeverServe1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(FeverServe1_0);
    }

    private static void FeverServe1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.GainServantFromLost, PopUpButton.Accept, _ => { });
    }

    #endregion End of The Round

    #region S5Special1

    private static void S5Special1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(1);
        globalData.ActiveWindow.AddClickHereToContinue(S5Special1A);
    }

    private static void S5Special1A(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterBid(S5Special1B);
    }

    private static void S5Special1B(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddNextContent(1, true);
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(mayor =>
        {
            globalData.TheCostOfDiseaseVars.Mayor = mayor;
            S5SpecialSetup1(globalData);
        });
    }

    private static void S5SpecialSetup1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S1_MayorCoin, PopUpButton.Accept, S5SpecialSetup1B,
            content => content.FormatWithReplacement(0, globalData.TheCostOfDiseaseVars.Mayor));
    }

    private static void S5SpecialSetup1B(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1,
            [data => S5SpecialSetup1B_0(BankOrLibrary.Bank, data),
                data => S5SpecialSetup1B_0(BankOrLibrary.Library, data)],
            true
        );
    }

    private static void S5SpecialSetup1B_0(BankOrLibrary bankOrLibrary, GlobalData globalData)
    {
        globalData.TheCostOfDiseaseVars.Building = bankOrLibrary;
        FeverServe2(globalData);
    }

    private static void FeverServe2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FeverServe2_0);
    }

    private static void FeverServe2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.GainServantFromLost, PopUpButton.Accept, FeverServe2_1);
    }

    private static void FeverServe2_1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Accept, _ => { },
            content => content
                      .FormatWithReplacement(2, globalData.TheCostOfDiseaseVars.Mayor)
                      .FormatWithCondition(1, () => globalData.TheCostOfDiseaseVars.Building == BankOrLibrary.Bank)
                      .FormatWithCondition(0, () => globalData.TheCostOfDiseaseVars.Building == BankOrLibrary.Bank));
    }

    #endregion S5Special1

    #region Resolving Hearts

    private static void FeverHeart(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "0", PopUpButton.Confirm, value =>
        {
            if (!int.TryParse(value, out int x)) return false;
            if (x < 0) return false;
            if (x > 100) return false;
            return true;
        }, value =>
        {
            globalData.TheCostOfDiseaseVars.CharityTotal = int.Parse(value);
            FeverHeart_0(globalData);
        });
    }

    private static void FeverHeart_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FeverHeart2);
    }

    private static void FeverHeart2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_HeartToken, PopUpButton.Accept, FeverHeart2_0);
    }

    private static void FeverHeart2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(playerName =>
        {
            globalData.TheCostOfDiseaseVars.Charity = playerName;
            FeverHeart2_1(globalData);
        });
    }

    private static void FeverHeart2_1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo,
            PopUpButton.Confirm, data => (ForcedFate(data) ?? data.TheCostOfDiseaseVars.RandomElement([S5Fate1, S5Fate2], 5)).Invoke(data));
    }

    // Village-name secret (Help page): true forces the Hospital, false Devastation, null draws as usual.
    private static bool? ForcedHospital(GlobalData globalData) => globalData.TownName switch
    {
        TheCostOfDiseaseVars.HOSPITAL_TOWN_NAME    => true,
        TheCostOfDiseaseVars.DEVASTATION_TOWN_NAME => false,
        _                                          => null,
    };

    // With a village-name secret the fate screen is not drawn: S5Fate1 when the building agrees (Bank -> Hospital,
    // Library -> Devastation), else S5Fate2, which picks its number to agree when it can.
    private static Action<GlobalData>? ForcedFate(GlobalData globalData)
    {
        if (ForcedHospital(globalData) is not bool hospital) return null;
        BankOrLibrary building = globalData.TheCostOfDiseaseVars.Building;
        return building == (hospital ? BankOrLibrary.Bank : BankOrLibrary.Library) ? S5Fate1 : S5Fate2;
    }

    private static void S5Fate1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();

        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));

        bool bank     = globalData.TheCostOfDiseaseVars.Building == BankOrLibrary.Bank;
        bool hospital = ForcedHospital(globalData) ?? bank;

        globalData.ActiveWindow.AddNextContent(bank ? 1 : 2, true);
        globalData.ActiveWindow.AddClickHereToContinue(hospital ? HospitalIntro : DevastationIntro);
    }

    private static void S5Fate2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();

        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;

        int heartNeeded = globalData.PlayersNum switch
        {
            2 => 3,
            3 => 6,
            _ => 8 // 4 players
        };

        List<int> rolls  = [1, 2, 3, 4, 5, 6];
        bool?     forced = ForcedHospital(globalData);

        // A village-name secret picks the number among those that agree with it (flavor: nothing is asked in mirror
        // mode). When none does, the number is drawn as usual and the text is slightly off (the forced link stays).
        List<int> agreeing = forced is bool hospitalForced
            ? rolls.Where(roll => (vars.CharityTotal > heartNeeded + roll) == hospitalForced).ToList()
            : [];

        heartNeeded += agreeing.Count > 0 ? vars.FlavorElement(agreeing, "S5Fate2Hearts") : vars.RandomElement(rolls, 6);

        bool enough   = vars.CharityTotal > heartNeeded;
        bool hospital = forced ?? enough;

        globalData.ActiveWindow.AddNextContent(enough ? 1 : 2, true, content => content.FormatWithReplacement(0, heartNeeded.ToString()));
        globalData.ActiveWindow.AddClickHereToContinue(hospital ? HospitalIntro : DevastationIntro);
    }

    #endregion Resolving Hearts
}