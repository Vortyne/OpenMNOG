#version 4
if ("0" < eval("/:Launch9")) {
} else {
    startDrag("MouseDrag", "1");
}
NumHoles = "18";
count = "0";
LevelCount = "0";
yPlacement = "300";
xPlacement1 = "106";
xPlacement2 = "188";
xPlacement3 = "268";
xPlacement4 = "501";
xPlacement5 = "585";
xPlacement6 = "666";
isHolding = "1";
placeCount = "1";
while (count < NumHoles) {
    count = count + "1";
    xPlacement = eval("xPlacement" add placeCount);
    duplicateMovieClip("HoleBase", "Hole" add count, "16384" + (count + "10"));
    setProperty("Hole" add count, _y, yPlacement);
    setProperty("Hole" add count, _x, xPlacement);
    tellTarget ("Hole" add count) {
        xOffset = eval("../:xPlacement");
        yOffset = eval("../:yPlacement");
        Holding = eval("../:isHolding");
        LevelNum = eval("../:LevelCount");
        ItemNum = eval("../:count") - "1" - int((eval("../:count") - "1") / "6") * "6" + "1";
    }
    if (count - int(count / "6") * "6" eq "0") {
        yPlacement = yPlacement - "70";
        xPlacement = "70";
        LevelCount = LevelCount + "1";
        isHolding = "0";
    }
    placeCount = placeCount + "1";
    if ("6" < placeCount) {
        placeCount = "1";
    }
}
NumLevel0 = "6";
count = "0";
yPlacement = "300";
xPlacement = "70";
placeCount = "1";
while (count < NumLevel0) {
    count = count + "1";
    xPlacement = eval("xPlacement" add placeCount);
    duplicateMovieClip("Level0Base", "Level0-" add count, "16384" + (count + "30"));
    setProperty("Level0-" add count, _y, yPlacement);
    setProperty("Level0-" add count, _x, xPlacement);
    tellTarget ("Level0-" add count add "/Symbol") {
        gotoAndPlay(eval("../../:count"));
    }
    placeCount = placeCount + "1";
    if ("6" < placeCount) {
        placeCount = "1";
    }
}
NumLevel1 = "6";
count = "0";
levelcount = "1";
yPlacement = "350";
xPlacement = "400";
while (count < NumLevel1) {
    count = count + "1";
    duplicateMovieClip("Level1Base", "Level1-" add count, "16384" + (levelcount + "70"));
    duplicateMovieClip("Level1Base", "Placed1-" add count, "16384" + (levelcount + "50"));
    setProperty("Placed1-" add count, _x, "2000");
    levelcount = levelcount + "1";
    xPosition = xPlacement + random("600") - "300";
    setProperty("Level1-" add count, _x, xPosition);
    yPosition = yPlacement;
    setProperty("Level1-" add count, _y, yPosition);
    tellTarget ("Level1-" add count) {
        xPosition = eval("../:xPosition");
        yPosition = eval("../:yPosition");
        Name = "Level1-" add eval("../:count");
        LevelNum = "1";
        Gravity = "0";
        ItemNum = eval("../:count");
    }
    tellTarget ("Level1-" add count add "/Symbol") {
        gotoAndPlay(eval("../../:count"));
    }
    tellTarget ("Placed1-" add count) {
        xPosition = eval("../:xPosition");
        yPosition = eval("../:yPosition");
        Name = "Level1-" add eval("../:count");
        LevelNum = "1";
        Gravity = "0";
        ItemNum = eval("../:count");
    }
    tellTarget ("Placed1-" add count add "/Symbol") {
        gotoAndPlay(eval("../../:count"));
    }
    yPlacement = yPlacement + "5";
    duplicateMovieClip("Level2Base", "Level2-" add count, "16384" + (levelcount + "70"));
    duplicateMovieClip("Level2Base", "Placed2-" add count, "16384" + (levelcount + "50"));
    setProperty("Placed2-" add count, _x, "2000");
    levelcount = levelcount + "1";
    xPosition = xPlacement + random("600") - "300";
    setProperty("Level2-" add count, _x, xPosition);
    yPosition = yPlacement;
    setProperty("Level2-" add count, _y, yPosition);
    tellTarget ("Level2-" add count) {
        xPosition = eval("../:xPosition");
        yPosition = eval("../:yPosition");
        Name = "Level2-" add eval("../:count");
        LevelNum = "2";
        Gravity = "0";
        ItemNum = eval("../:count");
    }
    tellTarget ("Level2-" add count add "/Symbol") {
        gotoAndPlay(eval("../../:count"));
    }
    tellTarget ("Placed2-" add count) {
        xPosition = eval("../:xPosition");
        yPosition = eval("../:yPosition");
        Name = "Level2-" add eval("../:count");
        LevelNum = "2";
        Gravity = "0";
        ItemNum = eval("../:count");
    }
    tellTarget ("Placed2-" add count add "/Symbol") {
        gotoAndPlay(eval("../../:count"));
    }
    yPlacement = yPlacement + "5";
}
count1 = "0";
while (count1 < "6") {
    count1 = count1 + "1";
    count2 = "0";
    while (count2 < "2") {
        count2 = count2 + "1";
        set("Placed-" add count2 add "-" add count1, "0");
    }
}
DragOn = "0";
Releasing = "0";
Pickup = "0";
