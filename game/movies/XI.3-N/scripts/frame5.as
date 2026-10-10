#version 4
if (eval("/:Quest") >= "6") {
    tellTarget ("Back-Ground/Gate") {
        gotoAndStop(2);
    }
    setProperty("Middle-Ground/Tak_Merch", _x, "2000");
    setProperty("Middle-Ground/TaKoranBubble", _x, "2000");
    setProperty("Middle-Ground/Tak Stuff 1", _x, "2000");
    setProperty("Middle-Ground/Tak Stuff 2", _x, "2000");
    setProperty("Middle-Ground/Gak Merch", _x, "2000");
    setProperty("Middle-Ground/GaKoranBubble", _x, "2000");
    setProperty("Middle-Ground/Gak Stuff", _x, "2000");
    setProperty("Fore-Ground/Pok Merch", _x, "4000");
    setProperty("Fore-Ground/Pok Stuff", _x, "4000");
    setProperty("Fore-Ground/Pok Stuff 2", _x, "4000");
    setProperty("Fore-Ground/PoKoranBubble", _x, "4000");
}
