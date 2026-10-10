#version 4
tellTarget ("Lewa") {
    gotoAndPlay("Talk1");
}
Text = eval("Text" add textcount);
textcount = textcount + "1";
