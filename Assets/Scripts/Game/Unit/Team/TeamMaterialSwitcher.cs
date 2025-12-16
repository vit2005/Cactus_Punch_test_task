using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TeamMember))]
public class TeamMaterialSwitcher : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material teamAMaterial;
    [SerializeField] private Material teamBMaterial;

    private TeamMember _teamMember;

    private void Awake()
    {
        _teamMember = GetComponent<TeamMember>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        _teamMember.TeamChanged += OnTeamChanged;

        // важливо: застосувати матеріал одразу
        OnTeamChanged(_teamMember.Team);
    }

    private void OnDisable()
    {
        _teamMember.TeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(TeamSide team)
    {
        if (targetRenderer == null)
            return;

        targetRenderer.material = team switch
        {
            TeamSide.TeamA => teamAMaterial,
            TeamSide.TeamB => teamBMaterial,
            _ => targetRenderer.material
        };
    }
}
