#version 4
if (eval("/:Speed") eq "0") {
    tellTarget ("ForwardTunnel/Tunnel") {
        stop();
    }
} else if (eval("/:Speed") eq "1") {
    tellTarget ("ForwardTunnel/Tunnel") {
        play();
    }
}
if (eval("/:Exit") eq "1") {
    gotoAndPlay(19);
}
