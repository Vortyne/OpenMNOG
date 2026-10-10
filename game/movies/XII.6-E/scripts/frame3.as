#version 4
OnuKoroDoor = eval("/:OnuKoroDoor");
if (OnuKoroDoor eq "1") {
    setProperty("Fore-Ground/talktobutton", _x, "2000");
}
tellTarget ("/") {
    Movieisloaded = "1";
}
stop();
