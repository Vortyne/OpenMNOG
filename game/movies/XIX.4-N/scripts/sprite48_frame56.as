#version 4
setTarget("/");
$r128 = getProperty("", _target);
tellTarget ("/Holder/Middle-Ground/Door") {
    gotoAndPlay("Open");
}
tellTarget ($r128) {
    set("/:KoKoro", "2");
}
stop();
