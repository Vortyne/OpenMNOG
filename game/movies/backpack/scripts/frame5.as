#version 4
AllowChange = "0";
if (eval("cBar/:cBarOpen") eq "1") {
    tellTarget ("cBar") {
        gotoAndPlay("Close cBar");
    }
}
