#version 4
NumFish = "3";
count = "2";
tellTarget ("Fish1") {
    WhichFish = "1";
    xVelocity = random("15") - "7";
    yVelocity = random("15") - "7";
}
while (NumFish >= count) {
    duplicateMovieClip("Fish1", "Fish" add count, "16384" + (count + "1"));
    tellTarget ("Fish" add count) {
        WhichFish = eval("../:count");
        xVelocity = random("31");
        yVelocity = random("31");
    }
    setProperty("Fish" add count, _x, getProperty("Fish" add count, _x) + random("200") - "100");
    setProperty("Fish" add count, _y, getProperty("Fish" add count, _y) + random("200") - "100");
    count = count + "1";
}
startDrag("MouseDrag", "1");
