// ============================================
// Game Manager для керування правилами
// ============================================
using System.Collections.Generic;
using UnityEngine;

public class GameRulesManager : MonoBehaviour
{
    private GameRuleStrategy _currentRuleStrategy;
    private List<TeamMember> _allPlayers;

    public void InitializeGameRules(GameRuleStrategy currentRuleStrategy, List<TeamMember> allPlayers)
    {
        _currentRuleStrategy = currentRuleStrategy;
        _allPlayers = allPlayers;

        _currentRuleStrategy.Initialize(_allPlayers);

        // Підписуємось на події
        _currentRuleStrategy.OnPlayerDied += HandlePlayerDied;
        _currentRuleStrategy.OnVictoryConditionMet += HandleVictory;
    }

    private void HandlePlayerDied(TeamMember player)
    {
        Debug.Log($"GameManager: Гравець {player.name} загинув");
        // Тут можна додати UI повідомлення, звуки і т.д.
    }

    private void HandleVictory(VictoryResult result)
    {
        Debug.Log($"GameManager: Гра закінчена! Тип перемоги: {result.Type}");

        switch (result.Type)
        {
            case VictoryType.TeamVictory:
                Debug.Log($"Команда {result.WinningTeam} перемогла!");
                break;
            case VictoryType.PlayerVictory:
                Debug.Log($"Гравець {result.WinningPlayer.name} переміг!");
                break;
            case VictoryType.Draw:
                Debug.Log("Нічия!");
                break;
        }

        // Тут можна показати екран перемоги, зупинити гру і т.д.
        ShowVictoryScreen(result);
    }

    private void ShowVictoryScreen(VictoryResult result)
    {
        // Реалізація UI екрану перемоги
        Time.timeScale = 0; // Призупиняємо гру
    }

    private void OnDestroy()
    {
        if (_currentRuleStrategy != null)
        {
            _currentRuleStrategy.OnPlayerDied -= HandlePlayerDied;
            _currentRuleStrategy.OnVictoryConditionMet -= HandleVictory;
        }
    }
}