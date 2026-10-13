#version 4
Language = eval("/:Language");
if (Language eq "deu") {
    Text = "Du kannst passieren.";
} else if (Language eq "fre") {
    Text = "Vous pouvez passer.";
} else {
    Text = "You may pass";
}
