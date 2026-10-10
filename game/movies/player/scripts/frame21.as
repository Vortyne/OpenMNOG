#version 5
if (notloggedin eq "0" and servererror eq "0") {
    tellTarget ("State-saving") {
        gotoAndPlay("SetState");
    }
}
