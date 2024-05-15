﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgesideVillage
{
    internal static class RSVConstants
    {
        // Events
        public const string E_LENNYGH = "75160089";
        public const string E_NINJAQUESTS = "75160187";
        public const string E_LENNYCARTS = "75160190";
        public const string E_TOWNBOARD = "75160207";
        public const string E_WEDDINGRECEPTION = "75160245";
        public const string E_BIRTHDAY = "75160247";
        public const string E_ANNIVERSARY = "75160248";
        public const string E_MEETDAIA = "75160254";
        public const string E_MEETBELINDA = "75160255";
        public const string E_PREUNSEAL = "75160257";

        public const string E_BLISSVISIT = "75160258";
        public const string E_RAEUNSEAL = "75160259";
        public const string E_OPENPORTAL = "75160256";
        public const string E_CLEANSED = "75160263";
        public const string E_NINJABOARD = "75160264";
        public const string E_BLISSGH1 = "75160266";
        public const string E_SPIRITGH1 = "75160267";
        public const string E_BLISSGH2 = "75160268";
        public const string E_SPIRITGH2 = "75160269";
        public const string E_LORENZO1H = "75160333";
        public const string E_GRANDMA = "75160383";
        //outdated, its the letter OddJobsNotice
        //public const string E_SUMMITUNLOCK = "75160387";
        public const string E_BUSSTOP_INTRO = "75160186";
        public const string E_ZAYNE_INTRO = "75160338";
        public const string E_ZAYNE_2H = "75160436";
        public const string E_ZAYNE_6H = "75160438";
        public const string E_BRYLE_INTRO = "75160375";
        public const string E_BRYLE_8H = "75160453";
        public const string E_AGUAR_8H = "75160064";

        // Response IDs
        public const string R_AGUAR_FLOWERS = "751600641";
        public const string R_AGUAR_FRUIT = "751600642";

        // Quests
        public const string Q_CURSEDGH1 = "72860001";
        public const string Q_NINJANOTE = "72860002";
        public const string Q_OPENPORTAL = "72860003";
        public const string Q_CURSEDGH2 = "72860004";
        public const string Q_PREPCOMPLETE = "72860005";
        public const string Q_PREUNSEAL = "72860006";
        public const string Q_RAEUNSEAL = "72860007";

        // Special Orders
        public const string SO_FIXMINECART = "RSV.UntimedSpecialOrder.FixMinecart";
        public const string SO_FIXGREENHOUSE = "RSV.UntimedSpecialOrder.FixGreenhouse";
        public const string SO_DAIAQUEST = "RSV.UntimedSpecialOrder.DaiaQuest";
        public const string SO_RedCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalRed";
        public const string SO_OrangeCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalOrange";
        public const string SO_YellowCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalYellow";
        public const string SO_GreenCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalGreen";
        public const string SO_BlueCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalBlue";
        public const string SO_PurpleCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalPurple";
        public const string SO_GrayCRYSTAL = "RSV.UntimedSpecialOrder.ColorCrystalGray";
        public const string SO_CLEANSING = "RSV.UntimedSpecialOrder.SpiritRealmFlames";
        public const string SO_HAUNTEDGH = "RSV.UntimedSpecialOrder.HauntedGH";
        public const string SO_LINKEDFISH = "RSV.UntimedSpecialOrder.LinkedFishes";
        public const string SO_PIKAQUEST = "RSV.SpecialOrder.PikaDeliver";

        // Mail flags
        public const string M_ROOMBOOKEDFLAG = "RSV.HotelRoomBooked";
        public const string M_RECEPTIONBOOKEDFLAG = "RSV.ReservedReception";
        public const string M_RECEIVEDMAILWR = "WedReceptionMail";
        public const string M_BIRTHDAYBOOKEDFLAG = "RSV.BirthdayBooked";
        public const string M_ENGAGEDFLAG = "RSV.IsEngagedFlag";
        public const string M_BIRTHDAYBOOKED = "RSV.BirthdayBooked.";
        public const string M_ANNIVERSARYBOOKEDFLAG = "RSV.ReservedAnv";
        public const string M_ANNIVERSARYTODAY = "RSV.AnvToday";
        public const string M_CRYSTALS = "CrystalsFlagOngoing";
        public const string M_ODDJOBNOTICE = "OddJobsNotice";
            //relic items
        public const string M_MOOSE = "RSV.MooseStatue";
        public const string M_FOXMASK = "RSV.FoxMask";
        public const string M_SAPPHIRE = "RSV.Sapphire";
        public const string M_MUSICBOX = "RSV.MusicBox";
        public const string M_EVERFROST = "RSV.EverFrost";
        public const string M_HEROSTATUE = "RSV.HeroStatue";
        public const string M_CANDELABRUM = "RSV.Candelabrum";
        public const string M_ELVENCOMB = "RSV.ElvenComb";
        public const string M_OPALHALO = "RSV.OpalHalo";

        public const string M_OPENEDPORTAL = "RSV.Opened_Portal";
        public const string M_UNDREYAHOME = "RSV.UndreyaStayHome";
        public const string M_TORTSLOVE = "RSV.LoverMailFlag";
        public const string M_TORTSFAIRY = "RSV.FairyMailFlag";
        public const string M_TORTSMETEOR = "RSV.MeteorMailFlag";
        public const string M_LOANMAIL = "RSV.TakenLoan";
        public const string M_MINECARTSFIXED = "RSV.FixedMinecart";
        public const string M_HOUSEUPGRADED = "RSV.SummitHouseRedone";
        public const string M_CLIMATECONTROLLED = "RSV.ClimateControlled";
        public const string M_GOTSPRINKLERS = "RSV.SummitSprinklers";
        public const string M_OREAREAOPENED = "RSV.SummitOreArea";
        public const string M_SHEDADDED = "RSV.ShedAdded";
        public const string M_GOLDCLOCK = "RSV.HasGoldClock";

        // Location names
        public const string L_VILLAGE = "Custom_Ridgeside_RidgesideVillage";
        public const string L_RIDGE = "Custom_Ridgeside_Ridge";
        public const string L_FOREST = "Custom_Ridgeside_RidgeForest";
        public const string L_FALLS = "Custom_Ridgeside_RidgeFalls";
        public const string L_POND = "Custom_Ridgeside_RidgePond";
        public const string L_CABLECAR = "Custom_Ridgeside_RSVCableCar";
        public const string L_CLIFF = "Custom_Ridgeside_RSVCliff";
        public const string L_CLINIC = "Custom_Ridgeside_PaulaClinic";
        public const string L_HOTEL = "Custom_Ridgeside_LogCabinHotelLobby";
        public const string L_HOTEL2 = "Custom_Ridgeside_LogCabinHotel2ndFloor";
        public const string L_HOTEL3 = "Custom_Ridgeside_LogCabinHotel3rdFloor";
        public const string L_ALISSASHED = "Custom_Ridgeside_AlissaShed";
        public const string L_HIKE = "Custom_Ridgeside_RSVTheHike";
        public const string L_TOWNGH = "Custom_Ridgeside_RSVGreenhouse1";
        public const string L_HAUNTEDGH = "Custom_Ridgeside_RSVGreenhouse2";
        public const string L_NINJAS = "Custom_Ridgeside_RSVNinjaHouse";
        public const string L_SPIRITREALM = "Custom_Ridgeside_RSVSpiritRealm";
        public const string L_TORTSREALM = "Custom_Ridgeside_TortsRealm";
        public const string L_SUMMITFARM = "Custom_Ridgeside_SummitFarm";
        public const string L_SUMMITHOUSE = "Custom_Ridgeside_RSVSummitHouseNew";
        public const string L_SUMMITSHED = "Custom_Ridgeside_RSVSummitShed";
        public const string L_HIDDENWARP = "Custom_Ridgeside_RSVHiddenWarp2";
        public const string L_CABLECARBG = "Custom_Ridgeside_RSVTheRide_static";

        // Conversation topics
        public const string CT_HOUSEUPGRADE = "RSV.HouseCT";
        public const string CT_CLIMATE = "RSV.ClimateCT";
        public const string CT_SPRINKLER = "RSV.SprinklerCT";
        public const string CT_OREAREA = "RSV.OreCT";
        public const string CT_SHED = "RSV.ShedCT";
        public const string CT_ACTIVECONSTRUCTION = "RSV.ActiveConstruction";
        public const string CT_PIKATOPIC = "pika_pickup";
        public const string CT_KEAHITOPIC = "keahi_stink";

        // Miscellaneous
        
            //QuestBoards
        public const string Z_VILLAGEBOARD = "VillageQuestBoard";
        public const string Z_NINJAQUESTBOARD = "RSVNinjaBoard";

            //SpecialOrder Types
        public const string Z_VILLAGESPECIALORDER = "RSVTownSO";
        public const string Z_NINJASPECIALORDER = "RSVNinjaSO";

        //Powers
        public const string P_RIVERASECRET = "Rafseazz.RSVCP_RiveraSecret";

            //Items
        public const string ITOTEM = "Rafseazz.RSVCP_Warp_Totem__Ridgeside";
        public const string IFOXBLOOM = "Rafseazz.RSVCP_Foxbloom";
        public const string IMUSICBOX = "Rafseazz.RSVCP_Ancient_Music_Box";
        public const string IELVENCOMB = "Rafseazz.RSVCP_Elven_Comb";
        public const string IEVERFROSTSTONE = "Rafseazz.RSVCP_Everfrost_Stone";
        public const string IMOOSESTATUE = "Rafseazz.RSVCP_Moose_Statue";
        public const string IOPALHALO = "Rafseazz.RSVCP_Opal_Halo";
        public const string ICANDELABRUM = "Rafseazz.RSVCP_Pale_Candelabrum";
        public const string IRELICFOXMASK = "Rafseazz.RSVCP_Relic_Fox_Mask";
        public const string ISAPPHIREPEARL = "Rafseazz.RSVCP_Sapphire_Pearl";
        public const string IFOXTAILCHARM = "Rafseazz.RSVCP_Old_Lucky_Foxtail_Charm";
        public const string IHEROSTATUE = "Rafseazz.RSVCP_Village_Hero_Sculpture";

    }
}
