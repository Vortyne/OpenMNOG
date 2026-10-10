#version 4
AllowChange = "0";
if (eval("../:BackpackOpen") eq "1") {
    tellTarget ("../") {
        gotoAndPlay("Close Backpack");
    }
}
