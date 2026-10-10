#version 4
III3MazePicked = eval("/:III3MazePicked");
if (III3MazePicked != "1") {
    NumberOfScreens = "9";
    NumberOfDirections = "4";
    Crashcount = "0";
    PickingLeftSide = "1";
    PickingRightSide = "1";
    NumberOfSets = NumberOfScreens * NumberOfDirections / "2";
    GotSet = "0";
    RN1 = "0";
    Set_0_0 = "1";
    Link_0_0 = "III.1-N.swf";
    while ("1" < NumberOfSets) {
        while (PickingLeftSide) {
            RL1 = random(NumberOfDirections);
            SetLeft = "_" add RN1 add "_" add RL1;
            if (eval("Set" add SetLeft) != "1") {
                PickingLeftSide = "0";
                set("Set" add SetLeft, "1");
                while (PickingRightSide) {
                    RN2 = random(NumberOfScreens);
                    RL2 = random(NumberOfDirections);
                    SetRight = "_" add RN2 add "_" add RL2;
                    if (eval("Set" add SetRight) != "1") {
                        PickingRightSide = "0";
                        set("Set" add SetRight, "1");
                        set("Set" add SetLeft, "1");
                        set("Link" add SetLeft, RN2);
                        set("Link" add SetRight, RN1);
                        set("LinkTo" add SetLeft, RL2);
                        set("LinkTo" add SetRight, RL1);
                        RN1 = RN2;
                    }
                }
            }
        }
        PickingLeftSide = "1";
        PickingRightSide = "1";
        NumberOfSets = NumberOfSets - "1";
    }
    RL1 = "0";
    LastSet = "0";
    while (LastSet < "1") {
        SetLeft = "_" add RN1 add "_" add RL1;
        if (eval("Set" add SetLeft) eq "1") {
            RL1 = RL1 + "1";
        } else {
            LastSet = "1";
        }
    }
    set("Link" add SetLeft, "IV.1-N.swf");
    tellTarget ("/") {
        III3_ExitScreen = eval("/holder/:RN1");
        III3_ExitDir = eval("/holder/:RL1");
    }
    KapuraIsAt = random(NumberOfScreens - "1") + "1";
    setTarget("/");
    HighQuality = "1";
    Link_0_0 = eval("/holder/:Link_0_0");
    Link_0_1 = eval("/holder/:Link_0_1");
    Link_0_2 = eval("/holder/:Link_0_2");
    Link_0_3 = eval("/holder/:Link_0_3");
    Link_1_0 = eval("/holder/:Link_1_0");
    Link_1_1 = eval("/holder/:Link_1_1");
    Link_1_2 = eval("/holder/:Link_1_2");
    Link_1_3 = eval("/holder/:Link_1_3");
    Link_2_0 = eval("/holder/:Link_2_0");
    Link_2_1 = eval("/holder/:Link_2_1");
    Link_2_2 = eval("/holder/:Link_2_2");
    Link_2_3 = eval("/holder/:Link_2_3");
    Link_3_0 = eval("/holder/:Link_3_0");
    Link_3_1 = eval("/holder/:Link_3_1");
    Link_3_2 = eval("/holder/:Link_3_2");
    Link_3_3 = eval("/holder/:Link_3_3");
    Link_4_0 = eval("/holder/:Link_4_0");
    Link_4_1 = eval("/holder/:Link_4_1");
    Link_4_2 = eval("/holder/:Link_4_2");
    Link_4_3 = eval("/holder/:Link_4_3");
    Link_5_0 = eval("/holder/:Link_5_0");
    Link_5_1 = eval("/holder/:Link_5_1");
    Link_5_2 = eval("/holder/:Link_5_2");
    Link_5_3 = eval("/holder/:Link_5_3");
    Link_6_0 = eval("/holder/:Link_6_0");
    Link_6_1 = eval("/holder/:Link_6_1");
    Link_6_2 = eval("/holder/:Link_6_2");
    Link_6_3 = eval("/holder/:Link_6_3");
    Link_7_0 = eval("/holder/:Link_7_0");
    Link_7_1 = eval("/holder/:Link_7_1");
    Link_7_2 = eval("/holder/:Link_7_2");
    Link_7_3 = eval("/holder/:Link_7_3");
    Link_8_0 = eval("/holder/:Link_8_0");
    Link_8_1 = eval("/holder/:Link_8_1");
    Link_8_2 = eval("/holder/:Link_8_2");
    Link_8_3 = eval("/holder/:Link_8_3");
    LinkTo_0_1 = eval("/holder/:LinkTo_0_1");
    LinkTo_0_2 = eval("/holder/:LinkTo_0_2");
    LinkTo_0_3 = eval("/holder/:LinkTo_0_3");
    LinkTo_1_0 = eval("/holder/:LinkTo_1_0");
    LinkTo_1_1 = eval("/holder/:LinkTo_1_1");
    LinkTo_1_2 = eval("/holder/:LinkTo_1_2");
    LinkTo_1_3 = eval("/holder/:LinkTo_1_3");
    LinkTo_2_0 = eval("/holder/:LinkTo_2_0");
    LinkTo_2_1 = eval("/holder/:LinkTo_2_1");
    LinkTo_2_2 = eval("/holder/:LinkTo_2_2");
    LinkTo_2_3 = eval("/holder/:LinkTo_2_3");
    LinkTo_3_0 = eval("/holder/:LinkTo_3_0");
    LinkTo_3_1 = eval("/holder/:LinkTo_3_1");
    LinkTo_3_2 = eval("/holder/:LinkTo_3_2");
    LinkTo_3_3 = eval("/holder/:LinkTo_3_3");
    LinkTo_4_0 = eval("/holder/:LinkTo_4_0");
    LinkTo_4_1 = eval("/holder/:LinkTo_4_1");
    LinkTo_4_2 = eval("/holder/:LinkTo_4_2");
    LinkTo_4_3 = eval("/holder/:LinkTo_4_3");
    LinkTo_5_0 = eval("/holder/:LinkTo_5_0");
    LinkTo_5_1 = eval("/holder/:LinkTo_5_1");
    LinkTo_5_2 = eval("/holder/:LinkTo_5_2");
    LinkTo_5_3 = eval("/holder/:LinkTo_5_3");
    LinkTo_6_0 = eval("/holder/:LinkTo_6_0");
    LinkTo_6_1 = eval("/holder/:LinkTo_6_1");
    LinkTo_6_2 = eval("/holder/:LinkTo_6_2");
    LinkTo_6_3 = eval("/holder/:LinkTo_6_3");
    LinkTo_7_0 = eval("/holder/:LinkTo_7_0");
    LinkTo_7_1 = eval("/holder/:LinkTo_7_1");
    LinkTo_7_2 = eval("/holder/:LinkTo_7_2");
    LinkTo_7_3 = eval("/holder/:LinkTo_7_3");
    LinkTo_8_0 = eval("/holder/:LinkTo_8_0");
    LinkTo_8_1 = eval("/holder/:LinkTo_8_1");
    LinkTo_8_2 = eval("/holder/:LinkTo_8_2");
    LinkTo_8_3 = eval("/holder/:LinkTo_8_3");
    III3MazePicked = "1";
    KapuraIsAt = eval("/holder/:KapuraIsAt");
    if (III3Entry eq "IV1N") {
        CurrentScreen = III3_ExitScreen;
        LastDir = III3_ExitDir;
    } else {
        CurrentScreen = "0";
        LastDir = "0";
    }
    setTarget("");
}
