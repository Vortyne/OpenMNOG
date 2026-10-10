#version 4
if (StopNow eq "1") {
    tellTarget ("Head") {
        gotoAndPlay("TurnTowards");
    }
    tellTarget ("../") {
        Walk = "0";
    }
    StopNow = "0";
    stop();
}
