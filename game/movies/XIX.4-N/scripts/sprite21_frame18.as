#version 4
setTarget("/");
$r128 = getProperty("", _target);
tellTarget ("/Holder/Middle-Ground/Kopeke/Kopeke") {
    gotoAndPlay("Play2");
}
tellTarget ($r128) {
}
stop();
