#version 4
Startpump2 = eval("../:Startpump2");
if (Startpump2 eq "1") {
    tellTarget ("../Pump2") {
        gotoAndPlay("Play");
    }
    tellTarget ("../") {
        Startpump2 = "0";
    }
}
