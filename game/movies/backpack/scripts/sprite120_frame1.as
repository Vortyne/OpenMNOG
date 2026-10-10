#version 4
Dragging = "-1";
DisplayText = "";
xOffset = "-385";
yOffset = "-50";
NumItems = "0";
Slot1 = "null";
Slot2 = "null";
Slot3 = "null";
Slot4 = "null";
Slot5 = "null";
Slot6 = "null";
Slot7 = "null";
Slot8 = "null";
Slot9 = "null";
Slot1_Description = "";
Slot2_Description = "";
Slot3_Description = "";
Slot4_Description = "";
Slot5_Description = "";
Slot6_Description = "";
Slot7_Description = "";
Slot8_Description = "";
Slot9_Description = "";
LightStone = eval("/:Item_LightStone");
GaKoro_Cog = eval("/:GaKoro_Cog");
GaKoro = eval("/:GaKoro");
PoKoro = eval("/:PoKoro");
KohliBall = eval("/:KohliBall");
QuarryKey = eval("/:QuarryKey");
Biodermis = eval("/:Biodermis");
LavaBoard = eval("/:LavaBoard");
LetterA = eval("/:LetterA");
LetterB = eval("/:LetterB");
Book = eval("/:Book");
Chisel = eval("/:Chisel");
Gnomon = eval("/:Gnomon");
pBook_goat = eval("/:pBook_goat");
Disks = eval("/:Disks");
Flute = eval("/:Flute");
FireStaff = eval("/:FireStaff");
Ensign = eval("/:Ensign");
if (Book eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "Book");
    set("Slot" add NumItems add "_NoDrag", "1");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if (Flute eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "Flute");
    set("Slot" add NumItems add "_NoDrag", "1");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if (pBook_goat eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "PlanBook");
    set("Slot" add NumItems add "_NoDrag", "1");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if (Gnomon eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "gnomon");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if ("0" < Biodermis) {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "BioDermis");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if (LightStone eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "lightstone");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if (LavaBoard eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "LavaBoard");
    set("Slot" add NumItems add "_Description", "TextNum");
}
if (FireStaff eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "FireStaff");
    set("Slot" add NumItems add "_Description", "HEAT STONE");
}
if (Ensign eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "Ensign");
    set("Slot" add NumItems add "_Description", "TA-KORO ENSIGN");
}
if ("0" < Disks) {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "Disks");
    set("Slot" add NumItems add "_Description", "DISKS");
}
if (LetterA eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "LetterA");
    set("Slot" add NumItems add "_Description", "LETTER TO THE ASTROLOGER");
}
if (LetterB eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "LetterB");
    set("Slot" add NumItems add "_Description", "LETTER FROM NUJU");
}
if (GaKoro_Cog eq "1" and GaKoro eq "0") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "GaKoro-Cog");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "MACHINE PART");
}
if (PoKoro eq "0" and KohliBall eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "KohliBall");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "INFECTED KOLI BALL");
}
if (PoKoro eq "0" and "0" < QuarryKey) {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "QuarryKey");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "???");
    tellTarget ("QuarryKey") {
        gotoAndStop(eval("../:QuarryKey"));
    }
}
if (PoKoro eq "1" and Chisel eq "1") {
    NumItems = NumItems + "1";
    set("Slot" add NumItems, "Chisel");
    set("Slot" add NumItems add "_NoDrag", "0");
    set("Slot" add NumItems add "_Description", "PO-KORO CHISEL");
}
loop = "0";
while (loop < NumItems) {
    loop = loop + "1";
    setProperty(eval("Slot" add loop), _x, "-306.1" + (loop - "1") * "73");
    setProperty(eval("Slot" add loop), _y, "-9.3");
}
if (Slot1 ne "null") {
    BackpackOpen = eval("../:BackpackOpen");
    AllowChange = eval("../:AllowChange");
    cbAllowChange = eval("../cBar/:AllowChange");
    if (AllowChange eq "1" and BackpackOpen eq "0" and cbAllowChange eq "1") {
        tellTarget ("../") {
            gotoAndPlay("Open Backpack");
        }
    } else if (AllowChange eq "1" and BackpackOpen eq "1" and cbAllowChange eq "1") {
        tellTarget ("../") {
            gotoAndPlay("Close Backpack");
        }
    }
}
