#version 4
tellTarget ("/") {
    Startedloading = "1";
}
OnuKoroDoor = eval("/:OnuKoroDoor");
if (OnuKoroDoor eq "1") {
    setProperty("Fore-Ground/talktobutton", _x, "2000");
}
