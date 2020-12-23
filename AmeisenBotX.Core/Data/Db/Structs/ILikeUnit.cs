﻿using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;

namespace AmeisenBotX.Core.Data.Db.Structs
{
    interface ILikeUnit
    {
        public bool LikesHorde { get; set; }

        public bool LikesAlliance { get; set; }

        public bool LikesUnit(WowUnit wowUnit)
        {
            return (LikesAlliance && (wowUnit.Race == WowRace.Human || wowUnit.Race == WowRace.Gnome ||
                                      wowUnit.Race == WowRace.Draenei || wowUnit.Race == WowRace.Dwarf ||
                                      wowUnit.Race == WowRace.Nightelf)) ||
                   (LikesHorde && (wowUnit.Race == WowRace.Orc || wowUnit.Race == WowRace.Troll ||
                                   wowUnit.Race == WowRace.Bloodelf || wowUnit.Race == WowRace.Undead ||
                                   wowUnit.Race == WowRace.Tauren));
        }
    }
}
