#version 4
Dragging = "-1";
setProperty(Slot2, _x, "-233.1");
setProperty(Slot2, _y, "-9.3");
if (Slot2 eq "Flute") {
    tellTarget ("/") {
        loadMovie("blank.swf", "Music-holder");
        FluteOpen = "1";
    }
}
