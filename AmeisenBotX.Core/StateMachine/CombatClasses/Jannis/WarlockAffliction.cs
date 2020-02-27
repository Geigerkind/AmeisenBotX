﻿using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class WarlockAffliction : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string corruptionSpell = "Corruption";
        private readonly string curseOfAgonySpell = "Curse of Agony";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string demonArmorSpell = "Demon Armor";
        private readonly string demonSkinSpell = "Demon Skin";
        private readonly string drainSoulSpell = "Drain Soul";
        private readonly int fearAttemptDelay = 5;
        private readonly string fearSpell = "Fear";
        private readonly string felArmorSpell = "Fel Armor";
        private readonly string hauntSpell = "Haunt";
        private readonly string howlOfTerrorSpell = "Howl of Terror";
        private readonly string lifeTapSpell = "Life Tap";
        private readonly string shadowBoltSpell = "Shadow Bolt";
        private readonly string summonFelhunterSpell = "Summon Felhunter";
        private readonly string summonImpSpell = "Summon Imp";
        private readonly string unstableAfflictionSpell = "Unstable Affliction";

        public WarlockAffliction(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            PetManager = new PetManager(
                ObjectManager.Pet,
                TimeSpan.FromSeconds(1),
                null,
                () => (CharacterManager.SpellBook.IsSpellKnown(summonFelhunterSpell) && CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) && CastSpellIfPossible(summonFelhunterSpell))
                   || (CharacterManager.SpellBook.IsSpellKnown(summonImpSpell) && CastSpellIfPossible(summonImpSpell)),
                null);

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { felArmorSpell, () => CharacterManager.SpellBook.IsSpellKnown(felArmorSpell) && CastSpellIfPossible(felArmorSpell, true) },
                { demonArmorSpell, () => CharacterManager.SpellBook.IsSpellKnown(demonArmorSpell) && CastSpellIfPossible(demonArmorSpell, true) },
                { demonSkinSpell, () => CharacterManager.SpellBook.IsSpellKnown(demonSkinSpell) && CastSpellIfPossible(demonSkinSpell, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { corruptionSpell, () => CastSpellIfPossible(corruptionSpell, true) },
                { curseOfAgonySpell, () => CastSpellIfPossible(curseOfAgonySpell, true) },
                { unstableAfflictionSpell, () => CastSpellIfPossible(unstableAfflictionSpell, true) },
                { hauntSpell, () => CastSpellIfPossible(hauntSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warlock;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Affliction Warlock spec.";

        public override string Displayname => "Warlock Affliction";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public PetManager PetManager { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastFearAttempt { get; set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || PetManager.Tick()
                || ObjectManager.Player.ManaPercentage < 90
                    && ObjectManager.Player.HealthPercentage > 60
                    && CastSpellIfPossible(lifeTapSpell)
                || (ObjectManager.Player.HealthPercentage < 80
                    && CastSpellIfPossible(deathCoilSpell, true)))
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if (ObjectManager.Target.GetType() == typeof(WowPlayer))
                {
                    if (DateTime.Now - LastFearAttempt > TimeSpan.FromSeconds(fearAttemptDelay)
                        && ((ObjectManager.Player.Position.GetDistance(ObjectManager.Target.Position) < 6
                            && CastSpellIfPossible(howlOfTerrorSpell, true))
                        || (ObjectManager.Player.Position.GetDistance(ObjectManager.Target.Position) < 12
                            && CastSpellIfPossible(fearSpell, true))))
                    {
                        LastFearAttempt = DateTime.Now;
                        return;
                    }
                }

                if (!ObjectManager.Player.IsCasting
                    && CharacterManager.Inventory.Items.Count(e => e.Name.Equals("Soul Shard", StringComparison.OrdinalIgnoreCase)) < 5
                    && ObjectManager.Target.HealthPercentage < 8
                    && CastSpellIfPossible(drainSoulSpell, true))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(shadowBoltSpell, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || PetManager.Tick())
            {
                return;
            }
        }
    }
}