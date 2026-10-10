#version 4
if (eval("/:Pokoro") eq "1") {
    gotoAndPlay(4);
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
VIII4NVisited = eval("/:VIII4NVisited");
if ("0" < VIII4NVisited) {
    tellTarget ("/") {
        VIII4NVisited = "2";
        Startedloading = "1";
    }
}
