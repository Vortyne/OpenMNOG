#version 4
Empty = "1";
if (eval("../Magazine1/:Empty") eq "1" and eval("../Magazine2/:Empty") eq "1") {
    tellTarget ("../Magazine1") {
        gotoAndPlay("Reload");
    }
    tellTarget ("../Magazine2") {
        gotoAndPlay("Reload");
    }
} else {
    stop();
}
