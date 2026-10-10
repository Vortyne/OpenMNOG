#version 4
set("/:Count", eval("/:Count") + "4");
setProperty("Wall", _x, eval("/:Count"));
