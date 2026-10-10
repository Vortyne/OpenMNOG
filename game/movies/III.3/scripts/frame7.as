#version 4
if (getProperty("/Mousedrag", _x) < "120") {
    if (getProperty("/Mousedrag", _x) < "20") {
        moveFG = "45";
        moveMG = "30";
        moveBG = "22.5";
    } else if (getProperty("/Mousedrag", _x) < "60") {
        moveFG = "30";
        moveMG = "20";
        moveBG = "15";
    } else {
        moveFG = "15";
        moveMG = "10";
        moveBG = "7.5";
    }
    Counter = "0";
    FGLeftPanel = "/holder/Fore-Ground/Panel" add FGLeftPanelNum;
    FGRightPanel = "/holder/Fore-Ground/Panel" add FGRightPanelNum;
    if (getProperty(FGLeftPanel, _x) + moveFG >= xmin) {
        setProperty(FGRightPanel, _x, getProperty(FGLeftPanel, _x) - FGWidth);
        FGLeftPanelNum = FGRightPanelNum;
        FGRightPanelNum = FGRightPanelNum - "1";
        if (FGRightPanelNum < "1") {
            FGRightPanelNum = PanelNum;
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Fore-Ground/Panel" add Counter, _x, getProperty("/holder/Fore-Ground/Panel" add Counter, _x) + moveFG);
    }
    Counter = "0";
    MGLeftPanel = "/holder/Middle-Ground/Panel" add MGLeftPanelNum;
    MGRightPanel = "/holder/Middle-Ground/Panel" add MGRightPanelNum;
    if (getProperty(MGLeftPanel, _x) + moveMG >= xmin) {
        setProperty(MGRightPanel, _x, getProperty(MGLeftPanel, _x) - MGWidth);
        MGLeftPanelNum = MGRightPanelNum;
        MGRightPanelNum = MGRightPanelNum - "1";
        if (MGRightPanelNum < "1") {
            MGRightPanelNum = PanelNum;
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Middle-Ground/Panel" add Counter, _x, getProperty("/holder/Middle-Ground/Panel" add Counter, _x) + moveMG);
    }
    Counter = "0";
    BGLeftPanel = "/holder/Back-Ground/Panel" add BGLeftPanelNum;
    BGRightPanel = "/holder/Back-Ground/Panel" add BGRightPanelNum;
    if (getProperty(BGLeftPanel, _x) + moveBG >= xmin) {
        setProperty(BGRightPanel, _x, getProperty(BGLeftPanel, _x) - BGWidth);
        BGLeftPanelNum = BGRightPanelNum;
        BGRightPanelNum = BGRightPanelNum - "1";
        if (BGRightPanelNum < "1") {
            BGRightPanelNum = PanelNum;
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Back-Ground/Panel" add Counter, _x, getProperty("/holder/Back-Ground/Panel" add Counter, _x) + moveBG);
    }
} else if ("650" < getProperty("/Mousedrag", _x)) {
    if ("750" < getProperty("/Mousedrag", _x)) {
        moveFG = "45";
        moveMG = "30";
        moveBG = "22.5";
    } else if ("710" < getProperty("/Mousedrag", _x)) {
        moveFG = "30";
        moveMG = "20";
        moveBG = "15";
    } else {
        moveFG = "15";
        moveMG = "10";
        moveBG = "7.5";
    }
    Counter = "0";
    FGLeftPanel = "/holder/Fore-Ground/Panel" add FGLeftPanelNum;
    FGRightPanel = "/holder/Fore-Ground/Panel" add FGRightPanelNum;
    if (xmax >= getProperty(FGRightPanel, _x) - moveFG) {
        setProperty(FGLeftPanel, _x, getProperty(FGRightPanel, _x) + FGWidth);
        FGRightPanelNum = FGLeftPanelNum;
        FGLeftPanelNum = FGLeftPanelNum + "1";
        if (PanelNum < FGLeftPanelNum) {
            FGLeftPanelNum = "1";
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Fore-Ground/Panel" add Counter, _x, getProperty("/holder/Fore-Ground/Panel" add Counter, _x) - moveFG);
    }
    Counter = "0";
    MGLeftPanel = "/holder/Middle-Ground/Panel" add MGLeftPanelNum;
    MGRightPanel = "/holder/Middle-Ground/Panel" add MGRightPanelNum;
    if (xmax >= getProperty(MGRightPanel, _x) - moveMG) {
        setProperty(MGLeftPanel, _x, getProperty(MGRightPanel, _x) + MGWidth);
        MGRightPanelNum = MGLeftPanelNum;
        MGLeftPanelNum = MGLeftPanelNum + "1";
        if (PanelNum < MGLeftPanelNum) {
            MGLeftPanelNum = "1";
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Middle-Ground/Panel" add Counter, _x, getProperty("/holder/Middle-Ground/Panel" add Counter, _x) - moveMG);
    }
    Counter = "0";
    BGLeftPanel = "/holder/Back-Ground/Panel" add BGLeftPanelNum;
    BGRightPanel = "/holder/Back-Ground/Panel" add BGRightPanelNum;
    if (xmax >= getProperty(BGRightPanel, _x) - moveBG) {
        setProperty(BGLeftPanel, _x, getProperty(BGRightPanel, _x) + BGWidth);
        BGRightPanelNum = BGLeftPanelNum;
        BGLeftPanelNum = BGLeftPanelNum + "1";
        if (PanelNum < BGLeftPanelNum) {
            BGLeftPanelNum = "1";
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Back-Ground/Panel" add Counter, _x, getProperty("/holder/Back-Ground/Panel" add Counter, _x) - moveBG);
    }
}
DebugBGLeftPanelX = getProperty(eval("Back-Ground/Panel" add BGLeftPanelNum), _x);
DebugBGRightPanelX = getProperty(eval("Back-Ground/Panel" add BGRightPanelNum), _x);
