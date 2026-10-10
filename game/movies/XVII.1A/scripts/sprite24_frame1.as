#version 4
if (eval("/holder/textSub/:section") eq "1") {
    set("/:Taipu", "XVII.1-S");
} else if (eval("/holder/textSub/:section") eq "2") {
    set("/:Taipu", "XVII.1-N");
}
