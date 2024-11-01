﻿using Sokabon.CommandSystem;

namespace Sokabon.Trigger
{
    public class TriggerTargetLootGel : TriggerTarget
    {
        private Player _player;

        protected override void Awake()
        {
            base.Awake();
            _player = GetComponent<Player>();
        }

        protected override void OnSokabonTriggerEnter(Trigger trigger)
        {
            TriggerLootGel triggerLootGel = trigger as TriggerLootGel;
            if (triggerLootGel is null)
            {
                return;
            }

            _turnManager.ExecuteCommand(new PickUpGelLoot(_player, triggerLootGel));
        }

        protected override void OnSokabonTriggerExit(Trigger trigger)
        {
            // Do nothing
        }
    }
}