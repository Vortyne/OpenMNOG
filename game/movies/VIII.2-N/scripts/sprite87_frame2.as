#version 4
if ("0" < eval("/:QuarryKey") or "0" < eval("/:QuarryKeyUsed")) {
    setProperty("QuarryKey", _y, "-2000");
}
