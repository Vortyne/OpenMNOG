#version 4
if (eval("/:cKopeke") eq "0" and eval("/:cKapura") eq "1") {
    Section = eval("../TextSub/:Section");
    Segment = eval("../TextSub/:Segment");
    if (Section eq "2" and Segment eq "1") {
        tellTarget ("/") {
            cKopeke = "1";
            cBarNum = cBarNum + "1";
            set("Comrade_" add cBarNum, "comrade_kopeke.swf");
        }
        set("/Backpack/cBar/:cLoadNum", eval("/:cBarNum"));
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
}
