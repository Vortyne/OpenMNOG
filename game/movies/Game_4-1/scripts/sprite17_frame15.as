#version 4
set("../:NumKnockedOver", eval("../:NumKnockedOver") + "1");
set("../:FallenCount", MyNum);
tellTarget ("../") {
    set("CollObjHeight" add FallenCount, "-100");
}
stop();
