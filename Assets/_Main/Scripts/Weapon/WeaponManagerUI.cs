using TMPro;
using UnityEngine;

public class WeaponManagerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponNameTextMesh; // Silah adını göstermek için TextMeshProUGUI bileşeni

    private void Start()
    {
        WeaponManager.OnAnyWeaponChanged += WeaponManager_OnAnyWeaponChanged; // WeaponManager'dan herhangi bir silah değiştiğinde tetiklenecek eventi dinle
    }

    private void WeaponManager_OnAnyWeaponChanged(WeaponData weaponData)
    {
        ChangeWeaponName(weaponData.weaponName); // Silah adını değiştir
    }

    private void ChangeWeaponName(string weaponName)
    {
        weaponNameTextMesh.text = weaponName; // UI üzerinde silah adını gösteren metni güncelle
    }
}
