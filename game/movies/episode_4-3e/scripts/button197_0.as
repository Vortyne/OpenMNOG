#version 4
TextCount = TextCount + "1";
if (TextCount < "6") {
    if (TextCount eq "2") {
        if (Language eq "eng") {
            Text = "That was an infected mask, set beside the Koli balls... But who left it there, and placed the balls beside it?";
        } else if (Language eq "deu") {
            Text = "Dies ist eine infizierte Maske, die direkt neben die Koli-B‰lle gelegt wurde... Aber wer hat sie hier gelassen und die B‰lle direkt daneben gelegt?";
        } else if (Language eq "fre") {
            Text = "That was an infected mask, set beside the Koli balls... But who left it there, and placed the balls beside it?";
        }
    } else if (TextCount eq "3") {
        if (Language eq "eng") {
            Text = "Though Po-Koro will be safe now, we have uncovered an even greater mystery. I fear the Makuta's influence may extend beyond the Rahi, now...";
        } else if (Language eq "deu") {
            Text = "Po-Koro ist nun sicher. Wir haben sogar ein noch grˆeres Geheimnis gel¸ftet. Ich f¸rchte, Makutas Einfluss wird jetzt ¸ber die Rahi hinausgehen...";
        } else if (Language eq "fre") {
            Text = "Though Po-Koro will be safe now, we have uncovered an even greater mystery. I fear the Makuta's influence may extend beyond the Rahi, now...";
        }
    } else if (TextCount eq "4") {
        if (Language eq "eng") {
            Text = "I will take this news back to Po-Koro, and help them carry the infected Koli balls into the sea. My Mask of Power will let me get to the village quickly enough, I think, to save even Huki.";
        } else if (Language eq "deu") {
            Text = "Ich werde die Nachricht nach Po-Koro bringen und ihnen helfen, die infizierten Koli-B‰lle ins Meer zu werfen. Ich sch‰tze, meine Maske der Macht wird mir helfen, schnell genug zum Dorf zu gelangen, um selbst Huki zu retten.";
        } else if (Language eq "fre") {
            Text = "I will take this news back to Po-Koro, and help them carry the infected Koli balls into the sea. My Mask of Power will let me get to the village quickly enough, I think, to save even Huki.";
        }
    } else if (TextCount eq "5") {
        if (Language eq "eng") {
            Text = "You are a cunning ally, friend, and brave. I hope that we shall meet again, someday.";
        } else if (Language eq "deu") {
            Text = "Du bist ein schlauer Verb¸ndeter, und ein mutiger Freund. Ich hoffe, wir treffen uns eines Tages wieder.";
        } else if (Language eq "fre") {
            Text = "You are a cunning ally, friend, and brave. I hope that we shall meet again, someday.";
        }
    }
    gotoAndPlay(623);
} else {
    gotoAndPlay(644);
}
