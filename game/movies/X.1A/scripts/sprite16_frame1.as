#version 4
if (eval("/:cHafu") eq "0") {
    Section = eval("../TextSub/:Section");
    Segment = eval("../TextSub/:Segment");
    if (Section eq "1" and Segment eq "2") {
        tellTarget ("/") {
            cHafu = "1";
            cBarNum = cBarNum + "1";
            set("Comrade_" add cBarNum, "comrade_hafu.swf");
        }
        set("/Backpack/cBar/:cLoadNum", eval("/:cBarNum"));
        setProperty("../Hafu", _x, "2000");
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
