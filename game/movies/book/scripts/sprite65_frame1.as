#version 4
MaxSlots = eval("../:MaxSlots");
count = "0";
yPos = "-64.1";
xPos = "33.7";
while (count < MaxSlots) {
    count = count + "1";
    duplicateMovieClip("TextBase", "TextBox" add count, "16384" + (count + "10"));
    setProperty("TextBox" add count, _x, xPos);
    setProperty("TextBox" add count, _y, yPos);
    tellTarget ("TextBox" add count) {
        TextNum = eval("../:count");
    }
    yPos = yPos + "45";
}
