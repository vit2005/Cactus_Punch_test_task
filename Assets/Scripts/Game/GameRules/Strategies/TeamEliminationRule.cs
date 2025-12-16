// ============================================
// СТРАТЕГІЯ 1: Team Elimination (класична)
// Після смерті гравець виходить з гри, перемагає команда, що залишилась
// ============================================
using System.Linq;
using UnityEngine;

public class TeamEliminationRule : GameRuleStrategy
{
    [SerializeField] private bool enableSpectatorMode = true;

    protected override void ProcessPlayerDeath(GameObject source, TeamMember player)
    {
        Debug.Log($"[TeamElimination] Гравець {player.name} з команди {player.Team} загинув");

        // Деактивуємо гравця або переводимо в режим спостерігача
        if (enableSpectatorMode)
        {
            SetPlayerAsSpectator(player);
        }
        else
        {
            player.gameObject.SetActive(false);
        }
    }

    protected override void CheckVictoryCondition()
    {
        // Групуємо живих гравців по командах
        var alivePlayersByTeam = allPlayers
            .Where(p => p.gameObject.activeSelf && p.GetComponent<IHealthProvider>()?.IsAlive == true)
            .GroupBy(p => p.Team)
            .ToList();

        // Якщо залишилась одна команда - вона перемогла
        if (alivePlayersByTeam.Count == 1)
        {
            var winningTeam = alivePlayersByTeam[0].Key;
            var winners = alivePlayersByTeam[0].ToList();

            Debug.Log($"[TeamElimination] Команда {winningTeam} перемогла!");

            var result = new VictoryResult(VictoryType.TeamVictory)
            {
                WinningTeam = winningTeam,
                Winners = winners
            };

            AnnounceVictory(result);
        }
        // Якщо не залишилось жодної команди - нічия
        else if (alivePlayersByTeam.Count == 0)
        {
            Debug.Log("[TeamElimination] Нічия - всі загинули!");
            AnnounceVictory(new VictoryResult(VictoryType.Draw));
        }
    }

    private void SetPlayerAsSpectator(TeamMember player)
    {
        // Вимикаємо можливість стріляти та рухатись
        var shooter = player.GetComponent<SkillExecutor>(); // Ваш компонент стрільби
        var movement = player.GetComponent<MovementController>(); // Ваш компонент руху

        if (shooter) shooter.enabled = false;
        if (movement) movement.enabled = false;

        // Можна додати камеру для спостереження
        Debug.Log($"[TeamElimination] {player.name} тепер спостерігач");
    }
}