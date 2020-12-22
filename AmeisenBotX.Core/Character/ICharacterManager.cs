﻿using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Enums;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Core.Character.Talents;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character
{
    public interface ICharacterManager
    {
        CharacterEquipment Equipment { get; }

        CharacterInventory Inventory { get; }

        IWowItemComparator ItemComparator { get; set; }

        List<EquipmentSlot> ItemSlotsToSkip { get; set; }

        int Money { get; }

        List<WowMount> Mounts { get; }

        Dictionary<string, (int, int)> Skills { get; }

        SpellBook SpellBook { get; }

        TalentManager TalentManager { get; }

        void AntiAfk();

        void ClickToMove(Vector3 pos, ulong guid, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float turnSpeed = 20.9f, float distance = 0.5f);

        bool HasItemTypeInBag<T>(bool needsToBeUseable = false);

        bool IsAbleToUseArmor(WowArmor item);

        bool IsAbleToUseWeapon(WowWeapon item);

        bool IsItemAnImprovement(IWowItem item, out IWowItem itemToReplace);

        void Jump();

        void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.5f);

        void UpdateAll();

        void UpdateCharacterGear();

        void UpdateCharacterBags();
    }
}