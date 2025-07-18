using UnityEngine;

[CreateAssetMenu(fileName = "New_Weapon_Data", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float shootRate;
    //public float reloadTime;
}
