using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;       // Mermi prefabı
    [SerializeField] private WeaponData[] weaponDataArray;  // Silah verilerinin dizisi
    [SerializeField] private WeaponManager.Weapon[] weaponArray; // Silahların dizisi

    private int currentWeaponDataIndex;                     // Mevcut silah verisi dizin indeksi
    private Transform bulletSpawnPoint;                     // Mermi spawn noktası transform'u

    private void Start()
    {
        if (weaponDataArray.Length <= 0)
        {
            Debug.LogError("Kullanılacak silah bulunamadı!");  // Eğer silah belirtilmemişse hata ver
            return;
        }

        GetRandomWeaponData();  // Rastgele bir silahla başla
    }

    private void GetRandomWeaponData()
    {
        currentWeaponDataIndex = Random.Range(0, weaponDataArray.Length); // Rastgele bir silah seç

        ChangeWeaponMesh();  // Silahın görselini değiştir
    }

    public void SpawnBullet()
    {
        // Mermi oluştur ve yönünü ve hasarını ayarla
        Bullet bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.transform.forward = bulletSpawnPoint.forward;
        bullet.SetBulletDamage(GetCurrentWeaponData().damage);
    }

    public WeaponData GetCurrentWeaponData()
    {
        return weaponDataArray[currentWeaponDataIndex];  // Mevcut silah verisini döndür
    }

    public WeaponManager.Weapon GetCurrentWeapon()
    {
        WeaponManager.Weapon currentWeapon = weaponArray[currentWeaponDataIndex]; // Mevcut silah nesnesini al

        // Silah nesnesini silah dizisinde ara
        foreach (WeaponManager.Weapon weapon in weaponArray)
        {
            if (weapon.weaponName == GetCurrentWeaponData().weaponName)
            {
                currentWeapon = weapon;
            }
        }

        return currentWeapon;  // Mevcut silah nesnesini döndür
    }

    public void ChangeWeaponMesh()
    {
        if (weaponArray.Length <= 0)
        {
            Debug.LogError("Silah Dizisinde Silah Bulunamadı! " + transform);  // Eğer silah dizisi boşsa hata ver
            return;
        }

        // Silah nesnelerinin hepsini devre dışı bırak
        for (int i = 0; i < weaponArray.Length; i++)
        {
            weaponArray[i].weaponObject.SetActive(false);
        }

        bool weaponFound = false;

        // Silah verisine göre doğru silah nesnesini aktif hale getir
        foreach (WeaponManager.Weapon weapon in weaponArray)
        {
            if (weapon.weaponName == GetCurrentWeaponData().weaponName)
            {
                weapon.weaponObject.SetActive(true);  // Silah nesnesini aktif hale getir
                bulletSpawnPoint = weapon.bulletSpawnPoint;  // Mermi spawn noktasını ayarla
                weaponFound = true;
            }
        }

        if (!weaponFound)
        {
            Debug.LogError("Silah Dizisinde " + GetCurrentWeaponData().weaponName + " İsimli Bir Silah Bulunamadı! " + transform);
            return;
        }
    }
}
