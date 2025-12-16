using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameRuleStrategy
{
    public event Action<TeamMember> OnPlayerDied;
    public event Action<VictoryResult> OnVictoryConditionMet;

    protected List<TeamMember> allPlayers = new List<TeamMember>();
    protected bool gameEnded = false;

    public virtual void Initialize(List<TeamMember> players)
    {
        allPlayers = players;
        gameEnded = false;

        // Підписуємось на смерть кожного гравця
        foreach (var player in allPlayers)
        {
            var healthProvider = player.GetComponent<IHealthProvider>();
            if (healthProvider != null)
            {
                healthProvider.OnDeath += (source) => HandlePlayerDeath(source, player);
            }
        }
    }

    protected virtual void HandlePlayerDeath(GameObject source, TeamMember player)
    {
        if (gameEnded) return;

        OnPlayerDied?.Invoke(player);
        ProcessPlayerDeath(source, player);
        CheckVictoryCondition();
    }

    // Метод для обробки смерті гравця (кожна стратегія реалізує по-своєму)
    protected abstract void ProcessPlayerDeath(GameObject source, TeamMember player);

    // Метод для перевірки умов перемоги
    protected abstract void CheckVictoryCondition();

    protected void AnnounceVictory(VictoryResult result)
    {
        if (gameEnded) return;

        gameEnded = true;
        OnVictoryConditionMet?.Invoke(result);
    }

    protected virtual void OnDestroy()
    {
        // Відписуємось від подій
        foreach (var player in allPlayers)
        {
            var healthProvider = player.GetComponent<IHealthProvider>();
            if (healthProvider != null)
            {
                healthProvider.OnDeath -= (source) => HandlePlayerDeath(source, player);
            }
        }
    }
}