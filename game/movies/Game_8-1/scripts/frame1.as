#version 4
Language = eval("/:Language");
TextToLoad = "Text_Game_8-1_" add Language add ".txt";
loadVariables(TextToLoad, "Indicator");
FightScreen = "1";
FightWins = "6";
FightVisions = "3";
HafuHealth = "250";
TaipuHealth = "400";
KopekeHealth = "300";
MakuHealth = "150";
KapuraHealth = "250";
TamaruHealth = "200";
FightScreen = eval("/:FightScreen");
FightWins = eval("/:FightWins");
FightVisions = eval("/:FightVisions");
tellTarget ("BG") {
    gotoAndStop(eval("../:FightScreen"));
}
NumTohunga = "6";
NumRahi = "0";
FramesPassed = "0";
tellTarget ("Tohunga1") {
    name = "tohunga1";
}
tellTarget ("Tohunga2") {
    name = "tohunga2";
}
tellTarget ("Tohunga3") {
    name = "tohunga3";
}
tellTarget ("Tohunga4") {
    name = "tohunga4";
}
tellTarget ("Tohunga5") {
    name = "tohunga5";
}
tellTarget ("Tohunga6") {
    name = "tohunga6";
}
if (FightVisions eq "0") {
    if (FightWins eq "0" and FightScreen eq "1") {
        NumRahi = "1";
        duplicateMovieClip("NuiRama", "Rahi1", "16395");
        setProperty("Rahi1", _x, "178.6");
        setProperty("Rahi1", _y, "186.4");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
    } else {
        NumChoices = "4";
        RandNum = random(NumChoices) + "1";
        if (RandNum eq "1") {
            NumRahi = "2";
            duplicateMovieClip("NuiRama", "Rahi1", "16395");
            setProperty("Rahi1", _x, "146.7");
            setProperty("Rahi1", _y, "112.5");
            tellTarget ("Rahi1") {
                name = "rahi1";
                Alive = "1";
            }
            duplicateMovieClip("NuiRama", "Rahi2", "16396");
            setProperty("Rahi2", _x, "146.7");
            setProperty("Rahi2", _y, "286.4");
            tellTarget ("Rahi2") {
                name = "rahi2";
                Alive = "1";
            }
        } else if (RandNum eq "2") {
            NumRahi = "2";
            duplicateMovieClip("NuiRama", "Rahi1", "16395");
            setProperty("Rahi1", _x, "146.7");
            setProperty("Rahi1", _y, "112.5");
            tellTarget ("Rahi1") {
                name = "rahi1";
                Alive = "1";
            }
            duplicateMovieClip("NuiJaga", "Rahi2", "16396");
            setProperty("Rahi2", _x, "228.7");
            setProperty("Rahi2", _y, "289.3");
            tellTarget ("Rahi2") {
                name = "rahi2";
                Alive = "1";
            }
        } else if (RandNum eq "3") {
            NumRahi = "3";
            duplicateMovieClip("NuiRama", "Rahi1", "16395");
            setProperty("Rahi1", _x, "107.8");
            setProperty("Rahi1", _y, "99.5");
            tellTarget ("Rahi1") {
                name = "rahi1";
                Alive = "1";
            }
            duplicateMovieClip("NuiJaga", "Rahi2", "16396");
            setProperty("Rahi2", _x, "228.7");
            setProperty("Rahi2", _y, "289.3");
            tellTarget ("Rahi2") {
                name = "rahi2";
                Alive = "1";
            }
            duplicateMovieClip("NuiRama", "Rahi3", "16397");
            setProperty("Rahi3", _x, "306.7");
            setProperty("Rahi3", _y, "99.5");
            tellTarget ("Rahi3") {
                name = "rahi3";
                Alive = "1";
            }
        } else if (RandNum eq "4") {
            NumRahi = "2";
            duplicateMovieClip("NuiJaga", "Rahi1", "16395");
            setProperty("Rahi1", _x, "278.6");
            setProperty("Rahi1", _y, "183.3");
            tellTarget ("Rahi1") {
                name = "rahi1";
                Alive = "1";
            }
            duplicateMovieClip("NuiJaga", "Rahi2", "16396");
            setProperty("Rahi2", _x, "215.7");
            setProperty("Rahi2", _y, "311.4");
            tellTarget ("Rahi2") {
                name = "rahi2";
                Alive = "1";
            }
        }
    }
} else if (FightVisions eq "1") {
    NumChoices = "4";
    RandNum = random(NumChoices) + "1";
    if (RandNum eq "1") {
        NumRahi = "2";
        duplicateMovieClip("NuiJaga", "Rahi1", "16395");
        setProperty("Rahi1", _x, "278.6");
        setProperty("Rahi1", _y, "183.3");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("NuiJaga", "Rahi2", "16396");
        setProperty("Rahi2", _x, "215.7");
        setProperty("Rahi2", _y, "311.4");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    } else if (RandNum eq "2") {
        NumRahi = "3";
        duplicateMovieClip("NuiRama", "Rahi1", "16395");
        setProperty("Rahi1", _x, "107.8");
        setProperty("Rahi1", _y, "99.5");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi2", "16396");
        setProperty("Rahi2", _x, "328.1");
        setProperty("Rahi2", _y, "99.5");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
        duplicateMovieClip("Tarakava", "Rahi3", "16397");
        setProperty("Rahi3", _x, "210.9");
        setProperty("Rahi3", _y, "281.4");
        tellTarget ("Rahi3") {
            name = "rahi3";
            Alive = "1";
        }
    } else if (RandNum eq "3") {
        NumRahi = "2";
        duplicateMovieClip("NuiJaga", "Rahi1", "16395");
        setProperty("Rahi1", _x, "192.7");
        setProperty("Rahi1", _y, "155.3");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("Tarakava", "Rahi2", "16396");
        setProperty("Rahi2", _x, "261.4");
        setProperty("Rahi2", _y, "285.1");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    } else if (RandNum eq "4") {
        NumRahi = "4";
        duplicateMovieClip("NuiRama", "Rahi1", "16395");
        setProperty("Rahi1", _x, "114.8");
        setProperty("Rahi1", _y, "105.6");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi2", "16396");
        setProperty("Rahi2", _x, "297.7");
        setProperty("Rahi2", _y, "131.6");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi3", "16397");
        setProperty("Rahi3", _x, "105.8");
        setProperty("Rahi3", _y, "260.5");
        tellTarget ("Rahi3") {
            name = "rahi3";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi4", "16398");
        setProperty("Rahi4", _x, "276.7");
        setProperty("Rahi4", _y, "309.5");
        tellTarget ("Rahi4") {
            name = "rahi4";
            Alive = "1";
        }
    }
} else if (FightVisions eq "2") {
    NumChoices = "4";
    RandNum = random(NumChoices) + "1";
    if (RandNum eq "1") {
        NumRahi = "3";
        duplicateMovieClip("NuiJaga", "Rahi1", "16395");
        setProperty("Rahi1", _x, "197.6");
        setProperty("Rahi1", _y, "288");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi2", "16396");
        setProperty("Rahi2", _x, "321.7");
        setProperty("Rahi2", _y, "278.5");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi3", "16397");
        setProperty("Rahi3", _x, "261.7");
        setProperty("Rahi3", _y, "130.3");
        tellTarget ("Rahi3") {
            name = "rahi3";
            Alive = "1";
        }
    } else if (RandNum eq "2") {
        NumRahi = "3";
        duplicateMovieClip("Tarakava", "Rahi1", "16395");
        setProperty("Rahi1", _x, "164.6");
        setProperty("Rahi1", _y, "135.3");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("NuiRama", "Rahi2", "16396");
        setProperty("Rahi2", _x, "333.7");
        setProperty("Rahi2", _y, "207.1");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
        duplicateMovieClip("NuiJaga", "Rahi3", "16397");
        setProperty("Rahi3", _x, "245.7");
        setProperty("Rahi3", _y, "298.4");
        tellTarget ("Rahi3") {
            name = "rahi3";
            Alive = "1";
        }
    } else if (RandNum eq "3") {
        NumRahi = "2";
        duplicateMovieClip("Tarakava", "Rahi1", "16395");
        setProperty("Rahi1", _x, "191.6");
        setProperty("Rahi1", _y, "133.3");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("Tarakava", "Rahi2", "16396");
        setProperty("Rahi2", _x, "248.6");
        setProperty("Rahi2", _y, "279.2");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    } else if (RandNum eq "4") {
        NumRahi = "1";
        duplicateMovieClip("Muaka", "Rahi1", "16395");
        setProperty("Rahi1", _x, "326.8");
        setProperty("Rahi1", _y, "181.6");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
    }
} else if (FightVisions eq "3") {
    NumChoices = "4";
    RandNum = random(NumChoices) + "1";
    if (RandNum eq "1") {
        NumRahi = "2";
        duplicateMovieClip("NuiRama", "Rahi1", "16395");
        setProperty("Rahi1", _x, "108.8");
        setProperty("Rahi1", _y, "84.2");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("Muaka", "Rahi2", "16396");
        setProperty("Rahi2", _x, "329.8");
        setProperty("Rahi2", _y, "228.6");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    } else if (RandNum eq "2") {
        NumRahi = "2";
        duplicateMovieClip("NuiJaga", "Rahi1", "16395");
        setProperty("Rahi1", _x, "218.7");
        setProperty("Rahi1", _y, "154.3");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("Muaka", "Rahi2", "16396");
        setProperty("Rahi2", _x, "357.8");
        setProperty("Rahi2", _y, "252.6");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    } else if (RandNum eq "3") {
        NumRahi = "2";
        duplicateMovieClip("Muaka", "Rahi1", "16395");
        setProperty("Rahi1", _x, "320.8");
        setProperty("Rahi1", _y, "127.6");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("Tarakava", "Rahi2", "16396");
        setProperty("Rahi2", _x, "221.6");
        setProperty("Rahi2", _y, "286.2");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    } else if (RandNum eq "4") {
        NumRahi = "2";
        duplicateMovieClip("Muaka", "Rahi1", "16395");
        setProperty("Rahi1", _x, "316.7");
        setProperty("Rahi1", _y, "91.2");
        tellTarget ("Rahi1") {
            name = "rahi1";
            Alive = "1";
        }
        duplicateMovieClip("Muaka", "Rahi2", "16396");
        setProperty("Rahi2", _x, "370.8");
        setProperty("Rahi2", _y, "255.6");
        tellTarget ("Rahi2") {
            name = "rahi2";
            Alive = "1";
        }
    }
}
QueuePick = "1";
AllowQueue = "1";
HitCalc = "0";
RunName = "";
ReturnName = "";
