#version 4
if (eval("/:Taipu") eq "Caught") {
    setProperty("Taipu", _visible, "1");
} else {
    setProperty("Taipu", _visible, "0");
}
