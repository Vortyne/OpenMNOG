#version 4
cBarOpen = eval("/Backpack/cBar/:cBarOpen");
AllowChange = eval("/Backpack/cBar/:AllowChange");
if (cBarOpen eq "0" and AllowChange eq "1") {
    tellTarget ("/Backpack/cBar") {
        gotoAndPlay("Open cBar");
    }
}
