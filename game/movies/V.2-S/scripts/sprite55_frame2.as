#version 4
NumRocks = "8";
NumPlaced = "0";
ImpactCount = "20";
BasketAccel = "0";
CWAccel = "0";
CWTrue = getProperty("CounterWeight", _y);
Holding = "0";
count = "0";
while (count < NumRocks) {
    count = count + "1";
    set("RockAccel" add count, "0");
    duplicateMovieClip("RockBase" add count, "Rock" add count, "16384" + (count + "130"));
    setProperty("RockBase" add count, _x, "2000");
    tellTarget ("Rock" add count) {
        WhichRock = eval("../:count");
    }
    set("StartHeightRock" add count, getProperty("Rock" add count, _y));
}
duplicateMovieClip("BasketFrontBase", "BasketFront", "16514");
duplicateMovieClip("BasketBackBase", "BasketBack", "16494");
duplicateMovieClip("Overmask", "Overmask2", "16495");
setProperty("BasketBackBase", _x, "2000");
setProperty("BasketFrontBase", _x, "2000");
setProperty("Rocks", _x, "2000");
startDrag("mousedrag", "1");
BasketBaseHeight = getProperty("BasketFront", _y);
CWBaseHeight = getProperty("CounterWeight", _y);
DisplacedHeight = "0";
setProperty("Dial", _rotation, "-155");
