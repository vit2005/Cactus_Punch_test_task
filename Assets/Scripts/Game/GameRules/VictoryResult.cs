using System.Collections.Generic;

// Результат перемоги
public class VictoryResult
{
    public VictoryType Type { get; set; }
    public TeamSide WinningTeam { get; set; }
    public TeamMember WinningPlayer { get; set; }
    public List<TeamMember> Winners { get; set; }

    public VictoryResult(VictoryType type)
    {
        Type = type;
        Winners = new List<TeamMember>();
    }
}

public enum VictoryType
{
    TeamVictory,      // Перемогла команда
    PlayerVictory,    // Переміг конкретний гравець
    Draw              // Нічия
}