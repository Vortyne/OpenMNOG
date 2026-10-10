#version 4
PanelNum = "4";
xmin = "-100";
xmax = "870";
FGwidth = "1155";
MGwidth = "770";
BGwidth = "577.5";
FGxOffset = "0" - FGwidth;
MGxOffset = "0" - MGwidth;
BGxOffset = "0" - BGwidth;
moveFG = "15";
moveMG = "10";
moveBG = "7.5";
FGLeftPanelNum = LastDir + "2";
if ("4" < FGLeftPanelNum) {
    FGLeftPanelNum = "1";
}
FGRightPanelNum = LastDir + "1";
MGLeftPanelNum = FGLeftPanelNum;
MGRightPanelNum = FGRightPanelNum;
BGLeftPanelNum = FGLeftPanelNum;
BGRightPanelNum = FGRightPanelNum;
DebugLink0 = eval("Link_" add CurrentScreen add "_0");
DebugLink1 = eval("Link_" add CurrentScreen add "_1");
DebugLink2 = eval("Link_" add CurrentScreen add "_2");
DebugLink3 = eval("Link_" add CurrentScreen add "_3");
XLink0 = eval("X_" add CurrentScreen add "_0");
XLink1 = eval("X_" add CurrentScreen add "_1");
XLink2 = eval("X_" add CurrentScreen add "_2");
XLink3 = eval("X_" add CurrentScreen add "_3");
