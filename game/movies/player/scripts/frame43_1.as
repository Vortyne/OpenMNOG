#version 5
framecount = "0";
framerate = "0";
lasttime = getTimer();
if (Backpack eq "1" and (substring(Moviename, "0", "7") eq "Episode" or substring(Moviename, "0", "4") eq "Game" or BookOpen eq "1" or ShowBackPack eq "0")) {
    setProperty("Backpack", _visible, "0");
} else {
    setProperty("Backpack", _visible, "1");
}
gotoAndPlay(19);
