#version 4
if (eval("/:Quest") >= "6") {
    tellTarget ("Back-Ground/Gate") {
        gotoAndStop(2);
    }
    setProperty("Fore-Ground/Pokoran Statues", _x, "5000");
}
