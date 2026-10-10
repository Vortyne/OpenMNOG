#version 4
tellTarget ("/Holder/ForwardTunnel") {
    gotoAndStop(eval("/:TunnelStep") + "1");
}
