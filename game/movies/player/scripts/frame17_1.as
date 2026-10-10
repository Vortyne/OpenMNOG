#version 5
#operators flash4
if (ServerBasedDate < "1000") {
    currenttime = getTimer();
    gotoAndPlay(12);
} else {
    TimeLoopCounter = "1";
    while (eval("Launch" add TimeLoopCounter) < ServerBasedDate) {
        TimeLoopCounter = TimeLoopCounter + "1";
    }
    TimeOfLaunch = eval("Launch" add TimeLoopCounter);
}
