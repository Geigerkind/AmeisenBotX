﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using System;
using System.Diagnostics;

namespace AmeisenBotX.Core.StateMachine.Routines
{
    public static class SpeakToMerchantRoutine
    {
        public static bool Run(WowInterface wowInterface, WowUnit selectedUnit)
        {
            Console.WriteLine("IS CALLED!");
            if (wowInterface == null || selectedUnit == null)
            {
                return false;
            }

            if (wowInterface.ObjectManager.TargetGuid != selectedUnit.Guid)
            {
                wowInterface.HookManager.WowTargetGuid(selectedUnit.Guid);
                return false;
            }

            if (wowInterface.HookManager.WowIsClickToMoveActive())
            {
                wowInterface.MovementEngine.StopMovement();
            }

            if (!BotMath.IsFacing(wowInterface.ObjectManager.Player.Position, wowInterface.ObjectManager.Player.Rotation, selectedUnit.Position))
            {
                wowInterface.HookManager.WowFacePosition(wowInterface.ObjectManager.Player, selectedUnit.Position);
            }

            if (!wowInterface.HookManager.LuaUiIsVisible("GossipFrame", "MerchantFrame"))
            {
                wowInterface.HookManager.WowUnitRightClick(selectedUnit);
                return false;
            }

            if (selectedUnit.IsGossip)
            {
                if (wowInterface.HookManager.LuaUiIsVisible("GossipFrame"))
                {
                    string[] gossipTypes = wowInterface.HookManager.LuaGetGossipTypes();

                    for (int i = 0; i < gossipTypes.Length; ++i)
                    {
                        if (gossipTypes[i].Equals("vendor", StringComparison.OrdinalIgnoreCase)
                            || gossipTypes[i].Equals("repair", StringComparison.OrdinalIgnoreCase))
                        {
                            wowInterface.HookManager.LuaSelectGossipOption(i + 1);
                        }
                    }
                }

                if (!wowInterface.HookManager.LuaUiIsVisible("MerchantFrame"))
                {
                    return false;
                }
            }

            return true;
        }
    }
}