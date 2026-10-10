#version 4
while (Counter < PanelNum) {
    Counter = Counter + "1";
    setProperty("/holder/Panel" add Counter, _y, "212.5");
    setProperty("/holder/Panel" add Counter, _x, PanelWidth / "2" + xOffset);
    xOffset = xOffset + PanelWidth;
}
