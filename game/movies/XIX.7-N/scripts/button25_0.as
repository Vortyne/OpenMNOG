#version 4
if (eval("../:DragOn") eq "0") {
    set("../:DragOn", "1");
    set("../:Dragging", Name);
    set("../:DragLevel", LevelNum);
    set("../:DragItem", ItemNum);
    set("../:Pickup", "1");
    Gravity = "0";
}
