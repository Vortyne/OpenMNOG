#version 4
set("/:CloudRotate", eval("/:CloudRotate") + "5");
if (eval("/:CloudRotate") >= "360") {
    set("/:CloudRotate", "0");
}
setProperty("BG", _rotation, eval("/:CloudRotate") * "-1");
