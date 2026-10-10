#version 4
if (eval("../../:Gate") eq "0") {
    tellTarget ("../Gate") {
        gotoAndPlay("Rise");
    }
}
