#version 5
AbrevState = "1";
mvn = eval("/:MovieName");
vsh = eval("/:VerticalShift");
hsh = eval("/:HorizontalShift");
sho = eval("/:ShiftOff");
s_bg = eval("/:Sound_BackGround");
s_bg_v = eval("/:Sound_BG_Volume");
s_fx = eval("/:Sound_SFXMovie");
m_mv = eval("/:Music_Movie");
if ("0" < eval("/:LetterA")) {
    ltA = eval("/:LetterA");
}
if ("0" < eval("/:LetterB")) {
    ltB = eval("/:LetterB");
}
if ("0" < eval("/:Gnomon")) {
    Gno = eval("/:Gnomon");
}
if ("0" < eval("/:Item_LightStone")) {
    IltS = eval("/:Item_LightStone");
}
TaB = eval("/:TaBridge");
// (Port: the Ga-Koro gate, once opened with the puzzle, stays open across saves.)
Ga_g = eval("/:GaKoro_GateOpen");
Ens_p = eval("/:EnsignPass");
TaV = eval("/:TaVisit");
Gak = eval("/:GaKoro");
Lek = eval("/:LeKoro");
if (Gak eq "0") {
    Ga_c = eval("/:GaKoro_Cog");
    Ga_v = eval("/:GaKoroVisited");
} else {
    Bk = eval("/:Book");
}
Pok = eval("../:PoKoro");
q_k_u = eval("../:QuarryKeyUsed");
if (Pok eq "0") {
    q_k = eval("../:QuarryKey");
    k_b = eval("../:KohliBall");
    k_b_u = eval("../:KohliBallUsed");
} else {
    Chi = eval("../:Chisel");
}
if (eval("/:OnuKoroLava") eq "1") {
    Onu_L = eval("/:OnukoroLava");
}
if (eval("/:OnuKoroDoor") eq "1") {
    Onu_D = eval("/:OnukoroDoor");
}
if (eval("/:OnuKoroRats") eq "1") {
    Onu_R = eval("/:OnukoroRats");
}
if (eval("/:LavaBoard") eq "1") {
    Lv_b = eval("/:LavaBoard");
}
if ("0" < eval("/:LeKoro")) {
    Taipu = eval("/:Taipu");
}
if (eval("/:Flute") eq "1") {
    Fl = eval("/:Flute");
    Kr = eval("/:KahuRide");
}
if (eval("/:FireStaff") eq "1") {
    F_s = eval("/:FireStaff");
}
if (eval("/:Ensign") eq "1") {
    Ens = eval("/:Ensign");
}
if ("0" < eval("/:KoKoro")) {
    Kok = eval("/:KoKoro");
}
f_K = eval("/:fKoKoro");
f_L = eval("/:fLeKoro");
f_P = eval("/:fPoKoro");
f_O = eval("/:fOnuKoro");
f_T = eval("/:fTaKoro");
f_G = eval("/:fGaKoro");
cMa = eval("/:cMaku");
cKa = eval("/:cKapura");
cTa = eval("/:cTamaru");
cKo = eval("/:cKopeke");
cHa = eval("/:cHafu");
cTu = eval("/:cTaipu");
Quest = eval("/:Quest");
cBarNum = eval("/:cBarNum");
C1 = eval("/:Comrade_1");
C2 = eval("/:Comrade_2");
C3 = eval("/:Comrade_3");
C4 = eval("/:Comrade_4");
C5 = eval("/:Comrade_5");
C6 = eval("/:Comrade_6");
fgW = eval("/:FightWins");
fgV = eval("/:FightVisions");
fgS = eval("/:FightScreen");
MaHlth = eval("/:MakuHealth");
KaHlth = eval("/:KapuraHealth");
TuHlth = eval("/:TamaruHealth");
KoHlth = eval("/:KopekeHealth");
HaHlth = eval("/:HafuHealth");
TaHlth = eval("/:TaipuHealth");
p8 = "1";
StateLoaded = "1";
vars = "1";
Backpack = eval("/:Backpack");
ShowBackPack = eval("/:ShowBackPack");
I4WVisited = eval("/:I4WVisited");
// (OpenMNOG: saved locally; nothing is sent to the original website's setstate.asp.)
_level0.__matanui.setstate(this);
