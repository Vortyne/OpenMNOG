#version 4
if (eval("../:Dragging") eq Name) {
    set("../:DragOn", "0");
    set("../:Releasing", "1");
    wasDragging = eval("../:Dragging");
    tellTarget ("../" add wasDragging) {
        Gravity = "1";
    }
}
