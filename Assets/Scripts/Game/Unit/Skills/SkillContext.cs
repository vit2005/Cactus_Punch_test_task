using UnityEngine;

public struct SkillContext
{
    public GameObject Caster;
    public GameObject Target; // може бути null
    public Vector3 CasterPosition;
    public Vector3? TargetPosition; // nullable
}