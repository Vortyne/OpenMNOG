#version 4
tellTarget ("Guard/TurnStop") {
    WhichBubble = "../../Bubble1";
}
Language = eval("/:Language");
TextToLoad = "Text_IV8_Guard_1_" add Language add ".txt";
loadVariables(TextToLoad, "Bubble1");
xMntForward = getProperty("MntForward", _x);
yMntForward = getProperty("MntForward", _y);
// (Port: once the guard has seen the shield, the way to the cable car is open from the start.)
if (eval("/:EnsignPass") eq "1") {
    set("../:EnsignPass", "1");
} else {
    setProperty("MntForward", _x, "2000");
}
