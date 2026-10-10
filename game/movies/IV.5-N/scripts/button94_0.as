#version 4
if (Stopped eq "0") {
    StopNow = "1";
    Stopped = "1";
} else {
    tellTarget ("../") {
        Walk = "1";
    }
    tellTarget ("Head") {
        gotoAndPlay("TurnBack");
    }
    tellTarget ("../Surfer") {
        Stopped = "0";
        StopNow = "0";
        nextFrame();
        play();
    }
}
