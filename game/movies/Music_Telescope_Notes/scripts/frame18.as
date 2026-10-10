#version 4
currenttime = eval("/:currenttime");
if (SoundLength < currenttime - Time_Start) {
    Time_Start = eval("/:currenttime");
    gotoAndPlay(2);
}
