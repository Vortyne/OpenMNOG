#version 4
if (getProperty("/Mousedrag", _x) < "100") {
    Counter = "0";
    LeftPanel = "/holder/Panel" add LeftPanelNum;
    RightPanel = "/holder/Panel" add RightPanelNum;
    if (getProperty(LeftPanel, _x) + movewidth >= xmin) {
        setProperty(RightPanel, _x, getProperty(LeftPanel, _x) - PanelWidth);
        LeftPanelNum = RightPanelNum;
        RightPanelNum = RightPanelNum - "1";
        if (RightPanelNum < "1") {
            RightPanelNum = PanelNum;
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Panel" add Counter, _x, getProperty("/holder/Panel" add Counter, _x) + movewidth);
    }
} else if ("670" < getProperty("/Mousedrag", _x)) {
    Counter = "0";
    LeftPanel = "/holder/Panel" add LeftPanelNum;
    RightPanel = "/holder/Panel" add RightPanelNum;
    if (xmax >= getProperty(RightPanel, _x) - movewidth) {
        setProperty(LeftPanel, _x, getProperty(RightPanel, _x) + PanelWidth);
        RightPanelNum = LeftPanelNum;
        LeftPanelNum = LeftPanelNum + "1";
        if (PanelNum < LeftPanelNum) {
            LeftPanelNum = "1";
        }
    }
    while (Counter < PanelNum) {
        Counter = Counter + "1";
        setProperty("/holder/Panel" add Counter, _x, getProperty("/holder/Panel" add Counter, _x) - movewidth);
    }
}
