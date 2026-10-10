#version 4
MiniSubHit = eval("/:MiniSubHit");
if (MiniSubHit eq "1") {
    tellTarget ("/") {
        SailTo = "Po-Wahi";
    }
} else if (MiniSubHit eq "3") {
    tellTarget ("/") {
        SailTo = "Ta-Wahi";
    }
}
