#version 4
Language = eval("/:Language");
if (Language eq "eng") {
    Text = "Let's get out of here!";
} else if (Language eq "deu") {
    Text = "Beeil Dich! Hauen wir hier ab!";
} else if (Language eq "fre") {
    Text = "Sortons d'ici!";
} else {
    Language = "eng";
    Text = "Let's get out of here!";
}
