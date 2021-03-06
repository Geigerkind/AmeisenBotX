﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Profiles;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest
{
    public class QuestEngine
    {
        public QuestEngine(WowInterface wowInterface, AmeisenBotConfig config, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            Config = config;
            StateMachine = stateMachine;

            CompletedQuests = new List<int>();
            QueryCompletedQuestsEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public List<int> CompletedQuests { get; private set; }

        public IQuestProfile Profile { get; set; }

        public bool UpdatedCompletedQuests { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        private AmeisenBotStateMachine StateMachine { get; }

        private WowInterface WowInterface { get; }
        
        private DateTime LastAbandonQuestTime { get; set; } = DateTime.Now;

        public void Execute()
        {
            if (Profile == null)
            {
                return;
            }

            if (!UpdatedCompletedQuests)
            {
                if (WowInterface.EventHookManager.EventDictionary.All(e => e.Key != "QUEST_QUERY_COMPLETE"))
                {
                    WowInterface.EventHookManager.Subscribe("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);
                }

                if (QueryCompletedQuestsEvent.Run())
                {
                    WowInterface.HookManager.LuaQueryQuestsCompleted();
                }

                return;
            }

            if (Profile.Quests.Count > 0)
            {
                // do i need to recover my hp
                if (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                    && WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 60.0).Any())
                {
                    // wait or eat something
                    if (WowInterface.CharacterManager.HasItemTypeInBag<WowFood>() || WowInterface.CharacterManager.HasItemTypeInBag<WowRefreshment>())
                    {
                        StateMachine.SetState(BotState.Eating);
                        return;
                    }
                }

                IEnumerable<IBotQuest> selectedQuests = Profile.Quests.Peek().Where(e => !e.Returned && !CompletedQuests.Contains(e.Id));
                
                // Drop all quest that are not selected
                if (WowInterface.ObjectManager.Player.QuestlogEntries.Count() == 25 && DateTime.Now.Subtract(LastAbandonQuestTime).TotalSeconds > 30)
                {
                    WowInterface.HookManager.LuaAbandonQuestsNotIn(selectedQuests.Select(q => q.Name));
                    LastAbandonQuestTime = DateTime.Now;
                }

                if (selectedQuests.Any())
                {
                    IBotQuest notAcceptedQuest = selectedQuests.FirstOrDefault(e => !e.Accepted);

                    // make sure we got all quests
                    if (notAcceptedQuest != null)
                    {
                        if (!notAcceptedQuest.Accepted)
                        {
                            notAcceptedQuest.AcceptQuest();
                            return;
                        }
                    }
                    else
                    {
                        // do the quests if not all of them are finished
                        if (selectedQuests.Any(e => !e.Finished))
                        {
                            IBotQuest activeQuest = selectedQuests.FirstOrDefault(e => !e.Finished);
                            activeQuest?.Execute();
                        }
                        else
                        {
                            // make sure we return all quests
                            IBotQuest notReturnedQuest = selectedQuests.FirstOrDefault(e => !e.Returned);

                            if (notReturnedQuest != null)
                            {
                                if (notReturnedQuest.CompleteQuest())
                                {
                                    CompletedQuests.Add(notReturnedQuest.Id);
                                }

                                return;
                            }
                        }
                    }
                }
                else
                {
                    CompletedQuests.AddRange(Profile.Quests.Dequeue().Select(e => e.Id));
                    return;
                }
            }

            // filter duplicates
            CompletedQuests = CompletedQuests.Distinct().ToList();
        }

        public void Start()
        {
        }

        private void OnGetQuestsCompleted(long timestamp, List<string> args)
        {
            WowInterface.QuestEngine.CompletedQuests.Clear();
            WowInterface.QuestEngine.CompletedQuests.AddRange(WowInterface.HookManager.LuaGetCompletedQuests());

            WowInterface.QuestEngine.UpdatedCompletedQuests = true;
        }
    }
}