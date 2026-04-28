using UnityEngine;
using BillGameCore;

namespace Mythfall.Events
{
    /// <summary>Fired when the player takes damage. Subscribed by HUD HP bar, audio, vfx.</summary>
    public struct PlayerDamagedEvent : IEvent
    {
        public float damage;
        public float currentHP;
        public float maxHP;
    }

    /// <summary>Fired when player HP reaches 0. State machine routes to DefeatState.</summary>
    public struct PlayerDiedEvent : IEvent
    {
        public Mythfall.Player.PlayerBase player;
        public Vector3 position;
    }

    /// <summary>Fired every time a player attack lands on an enemy. Subscribed by VFX, audio, hitstop, damage numbers.</summary>
    public struct EnemyHitEvent : IEvent
    {
        public Mythfall.Player.PlayerBase attacker;
        public Mythfall.Enemy.EnemyBase victim;
        public float damage;
        public bool isCrit;
        public Vector3 hitPoint;
    }

    /// <summary>Fired when an enemy dies. Subscribed by XP gem spawner (Sprint 3), kill counter, audio, VFX.</summary>
    public struct EnemyKilledEvent : IEvent
    {
        public Mythfall.Enemy.EnemyBase enemy;
        public Mythfall.Player.PlayerBase killer;
        public Vector3 position;
    }

    /// <summary>Fired from CharacterSelectPanel when user picks a hero. InventoryService persists the choice.</summary>
    public struct CharacterSelectedEvent : IEvent
    {
        public string characterId;
    }
}
