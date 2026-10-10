#version 4
setTarget("/");
$r128 = getProperty("", _target);
tellTarget ("../Door") {
    gotoAndPlay("Play");
}
tellTarget ($r128) {
}
