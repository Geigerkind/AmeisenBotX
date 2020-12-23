﻿using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Data.Db.Structs
{
    public struct StaticMailbox : ILikeUnit
    {

        public StaticMailbox(int entry, int mapId, float posX, float posY, float posZ, bool likesHorde, bool likesAlliance)
        {
            Entry = entry;
            MapId = mapId;
            Position = new Vector3(posX, posY, posZ);
            LikesHorde = likesHorde;
            LikesAlliance = likesAlliance;
        }
        
        public int Entry { get; set; }
        
        public int MapId { get; set; }
        
        public Vector3 Position { get; set; }
        
        public bool LikesHorde { get; set; }
        
        public bool LikesAlliance { get; set; }
    }
}
