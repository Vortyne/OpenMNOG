#version 4
set("/:SlideX", "3");
set("/:SlideY", "1");
set("/:Shake", "0");
tellTarget ("Head/Target") {
    gotoAndStop(4);
}
set("/:ArmLSlideX", "-2");
set("/:ArmLSlideY", "-1");
set("/:ArmLShake", "0");
set("/:LBodySlideX", "-1");
set("/:LBodySlideY", "-0.5");
set("/:LBodyShake", "0");
