#version 4
cBarOpen = eval("../:cBarOpen");
AllowChange = eval("../:AllowChange");
bpAllowChange = eval("../../:AllowChange");
if (AllowChange eq "1" and bpAllowChange eq "1") {
    if (cBarOpen eq "1") {
        tellTarget ("../") {
            gotoAndPlay("Close cBar");
        }
    } else {
        tellTarget ("../") {
            gotoAndPlay("Open cBar");
        }
    }
}
