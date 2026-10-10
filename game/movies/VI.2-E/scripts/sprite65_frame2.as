#version 4
count = "1";
xAddedPos = "0";
yAddedPos = "0";
xAddedVel = "0";
yAddedVel = "0";
while (NumFish >= count) {
    xAddtoPos = getProperty("Fish" add count, _x);
    yAddtoPos = getProperty("Fish" add count, _y);
    xAddedPos = xAddedPos + xAddtoPos;
    yAddedPos = yAddedPos + yAddtoPos;
    tellTarget ("Fish" add count) {
        set("../:xAddtoVel", xVelocity);
        set("../:yAddtoVel", yVelocity);
    }
    xAddedVel = xAddedVel + xAddtoVel;
    yAddedVel = yAddedVel + yAddtoVel;
    count = count + "1";
}
xAvgPos = xAddedPos / NumFish;
yAvgPos = yAddedPos / NumFish;
xAvgVel = xAddedVel / NumFish;
yAvgVel = yAddedVel / NumFish;
