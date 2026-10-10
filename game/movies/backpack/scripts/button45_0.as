#version 4
BackpackOpen = eval("../../:BackpackOpen");
AllowChange = eval("../../:AllowChange");
cbAllowChange = eval("../../cBar/:AllowChange");
if (AllowChange eq "1" and cbAllowChange eq "1") {
    if (BackpackOpen eq "1") {
        tellTarget ("../../") {
            gotoAndPlay("Close Backpack");
        }
    } else {
        tellTarget ("../../") {
            gotoAndPlay("Open Backpack");
        }
    }
}
