#version 5
if (notloggedin eq "0" and nostate eq "0" and servererror eq "0") {
    if (StateLoaded eq "1") {
        if (AbrevState eq "1") {
            MovieName = mvn;
            VerticalShift = vsh;
            HorizontalShift = hsh;
            ShiftOff = sho;
            Sound_BackGround = s_bg;
            Sound_BG_Volume = s_bg_v;
            Sound_SFXMovie = s_fx;
            Music_Movie = m_mv;
            if (Sound_BackGround eq "" or Sound_SFXMovie eq "" or Music_Movie eq "") {
                getURL("FSCommand:scenesounds", "");
            }
            LetterA = ltA;
            LetterB = ltB;
            Gnomon = Gno;
            if (ltS eq "1") {
                Item_LightStone = ltS;
            } else {
                Item_LightStone = IltS;
            }
            TaBridge = TaB;
            TaVisit = TaV;
            GaKoro = Gak;
            LeKoro = Lek;
            GaKoro_Cog = Ga_c;
            GaKoroVisited = Ga_v;
            GaKoro_GateOpen = Ga_g;
            EnsignPass = Ens_p;
            Book = Bk;
            PoKoro = Pok;
            QuarryKeyUsed = q_k_u;
            QuarryKey = q_k;
            KohliBall = k_b;
            KohliBallUsed = k_b_u;
            Chisel = Chi;
            OnukoroLava = Onu_L;
            OnukoroDoor = Onu_D;
            OnukoroRats = Onu_R;
            LavaBoard = Lv_b;
            Flute = Fl;
            KahuRide = kr;
            Taipu = eval("/:Taipu");
            FireStaff = F_s;
            Ensign = Ens;
            KoKoro = Kok;
            fLeKoro = f_L;
            fPoKoro = f_P;
            fOnuKoro = f_O;
            fTaKoro = f_T;
            fGaKoro = f_G;
            fKoKoro = f_K;
            if (p8 eq "1") {
                cMaku = cMa;
                cKapura = cKa;
                cTamaru = cTa;
                cKopeke = cKo;
                cHafu = cHa;
                cTaipu = cTu;
                Comrade_1 = C1;
                Comrade_2 = C2;
                Comrade_3 = C3;
                Comrade_4 = C4;
                Comrade_5 = C5;
                Comrade_6 = C6;
                FightWins = fgW;
                FightVisions = fgV;
                FightScreen = fgS;
                MakuHealth = MaHlth;
                KapuraHealth = KaHlth;
                TamaruHealth = TuHlth;
                KopekeHealth = KoHlth;
                HafuHealth = HaHlth;
                TaipuHealth = TaHlth;
            }
        }
    } else {
        gotoAndPlay(6);
    }
}
