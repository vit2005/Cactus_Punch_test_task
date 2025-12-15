using UnityEngine;

[DisallowMultipleComponent]
public class TeamMember : MonoBehaviour
{
    [field: SerializeField]
    public TeamSide Team { get; private set; }

    /// <summary>
    /// Викликається при спавні або ініціалізації персонажа
    /// </summary>
    public void SetTeam(TeamSide team)
    {
        Team = team;
    }

    /// <summary>
    /// Корисно для швидких перевірок
    /// </summary>
    public bool IsEnemy(TeamMember other)
    {
        if (other == null)
            return false;

        return Team != other.Team;
    }

    /// <summary>
    /// Якщо не хочеш тягнути сам компонент
    /// </summary>
    public bool IsEnemy(GameObject other)
    {
        if (!other.TryGetComponent(out TeamMember member))
            return false;

        return Team != member.Team;
    }
}
