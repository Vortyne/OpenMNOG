#version 4
currenttime = eval("/:currenttime");
if (currenttime - Time_Start < Waitlength) {
    gotoAndPlay(2);
}
