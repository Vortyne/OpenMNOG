#version 4
Link_0_0 = eval("/:Link_0_0");
Link_0_1 = eval("/:Link_0_1");
Link_0_2 = eval("/:Link_0_2");
Link_0_3 = eval("/:Link_0_3");
Link_1_0 = eval("/:Link_1_0");
Link_1_1 = eval("/:Link_1_1");
Link_1_2 = eval("/:Link_1_2");
Link_1_3 = eval("/:Link_1_3");
Link_2_0 = eval("/:Link_2_0");
Link_2_1 = eval("/:Link_2_1");
Link_2_2 = eval("/:Link_2_2");
Link_2_3 = eval("/:Link_2_3");
Link_3_0 = eval("/:Link_3_0");
Link_3_1 = eval("/:Link_3_1");
Link_3_2 = eval("/:Link_3_2");
Link_3_3 = eval("/:Link_3_3");
Link_4_0 = eval("/:Link_4_0");
Link_4_1 = eval("/:Link_4_1");
Link_4_2 = eval("/:Link_4_2");
Link_4_3 = eval("/:Link_4_3");
Link_5_0 = eval("/:Link_5_0");
Link_5_1 = eval("/:Link_5_1");
Link_5_2 = eval("/:Link_5_2");
Link_5_3 = eval("/:Link_5_3");
Link_6_0 = eval("/:Link_6_0");
Link_6_1 = eval("/:Link_6_1");
Link_6_2 = eval("/:Link_6_2");
Link_6_3 = eval("/:Link_6_3");
Link_7_0 = eval("/:Link_7_0");
Link_7_1 = eval("/:Link_7_1");
Link_7_2 = eval("/:Link_7_2");
Link_7_3 = eval("/:Link_7_3");
Link_8_0 = eval("/:Link_8_0");
Link_8_1 = eval("/:Link_8_1");
Link_8_2 = eval("/:Link_8_2");
Link_8_3 = eval("/:Link_8_3");
CurrentScreen = eval("/:CurrentScreen");
set("/:TempScreenDir", eval("../:ScreenDir"));
TempScreenDir = eval("../:ScreenDir");
if (eval("Link_" add CurrentScreen add "_" add TempScreenDir) eq "IV.1-N.swf" or eval("Link_" add CurrentScreen add "_" add TempScreenDir) eq "III.1-N.swf") {
} else {
    tellTarget ("/") {
        set("X_" add CurrentScreen add "_" add TempScreenDir, "1");
    }
    gotoAndPlay(2);
}
