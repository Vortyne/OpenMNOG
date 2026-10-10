#version 4
tellTarget ("../") {
    TurnClick = "0";
    AllowClick = "0";
    WalkClick = "0";
    gotoAndPlay("WalkLoop");
}
tellTarget ("../WalkingGuard") {
    gotoAndPlay(ReturnTo);
}
stop();
