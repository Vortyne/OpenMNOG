#version 4
Section = eval("../TextSub/:Section");
Segment = eval("../TextSub/:Segment");
if (Section eq "4" and Segment eq "2") {
    tellTarget ("/") {
        cKapura = "1";
        cBarNum = "1";
        Comrade_1 = "comrade_kapura.swf";
    }
    setProperty("../Tohunga", _x, "2000");
    setProperty("/Backpack/cBar/cBar", _visible, "1");
    cBarOpen = eval("/Backpack/cBar/:cBarOpen");
    AllowChange = eval("/Backpack/cBar/:AllowChange");
    if (cBarOpen eq "0" and AllowChange eq "1") {
        tellTarget ("/Backpack/cBar") {
            gotoAndPlay("Open cBar");
        }
    }
}
