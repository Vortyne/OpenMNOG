#version 4
setTarget("/");
$r128 = getProperty("", _target);
tellTarget ("/Holder/Middle-Ground/Door") {
    gotoAndPlay("Play");
}
tellTarget ($r128) {
}
stop();
