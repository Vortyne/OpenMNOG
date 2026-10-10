#version 4
set("/:Blind", eval("/:Blind") + "20");
if (eval("/:Blind") >= "100") {
    setTarget("/");
    $r128 = getProperty("", _target);
    tellTarget ("/Holder") {
        gotoAndPlay("PassOut");
    }
    tellTarget ($r128) {
    }
}
setProperty("Art", _alpha, eval("/:Blind"));
