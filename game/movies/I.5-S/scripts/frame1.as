#version 4
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder", "GET");
TaVisit = eval("/:TaVisit");
GaKoro = eval("/:GaKoro");
GaKoroVisited = eval("/:GaKoroVisited");
if (TaVisit eq "0") {
    setProperty("Middle-Ground/Boat", _x, "3000");
    setProperty("Middle-Ground/Maku", _x, "3000");
}
if (GaKoro eq "1" or GaKoroVisited eq "1") {
    setProperty("Middle-Ground/Maku", _x, "3000");
}
