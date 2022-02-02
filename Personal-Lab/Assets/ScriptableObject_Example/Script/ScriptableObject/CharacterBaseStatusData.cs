using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBaseStatsData", menuName = "ScriptableObjects/CharacterBaseStatsData", order = 1)]
public class CharacterBaseStatusData : ScriptableObject
{
    public int Hp;
    public int Defence;
    public int Damage;
    public int AttackSpeed;
    public int MoveSpeed;
}