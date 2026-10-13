#version 4
Language = eval("/:Language");
if (Language eq "deu") {
    Text = "Chronist, wach auf! Bitte wach auf! Es ist schrecklich!";
} else if (Language eq "fre") {
    Text = "Chroniqueur, réveille-toi ! Réveille-toi, je t'en prie ! C'est terrible !";
} else {
    Text = "It's horrible!";
}
