using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static event Action<WeaponData> OnAnyWeaponChanged; // Herhangi bir silahın değiştirildiğini duyuran event

    [SerializeField] private GameObject bulletPrefab; // Kurşun prefabı
    [SerializeField] private WeaponData[] weaponDataArray; // Silah verilerinin dizisi
    [SerializeField] private Weapon[] weaponArray; // Silahların dizisi

    private Transform bulletSpawnPoint; // Kurşunun başlangıç noktası
    private int currentWeaponDataIndex; // Mevcut silah verisi dizin indeksi
    private PlayerControls controls; // Oyuncu kontrolleri
    private bool inCooldown; // Soğuma sürecinde mi?
    private bool canShoot = true; // Ateş edebilir mi?
    private float cooldownTimer; // Soğuma süresi sayaçı

    private void Awake()
    {
        controls = new PlayerControls(); // Oyuncu kontrollerini başlat

        // Ateş etme ve silah değiştirme kontrolleri ayarla
        controls.Gameplay.Shoot.performed += context => Shoot();
        controls.Gameplay.ChangeWeapon.performed += context => ChangeWeapon();
    }

    private void Start()
    {
        if (weaponDataArray.Length <= 0)
        {
            Debug.LogError("Kullanılacak silah yok!"); // Hata: Kullanılacak silah yok
            return;
        }

        ChangeWeapon(0); // Başlangıçta ilk silahı seç
    }

    private void Update()
    {
        if (inCooldown)
        {
            cooldownTimer -= Time.deltaTime; // Soğuma süresini azalt

            if (cooldownTimer <= 0)
            {
                inCooldown = false; // Soğuma süresi tamamlandı
            }
        }
    }

    private void Shoot()
    {
        if (inCooldown || !canShoot) return; // Eğer soğuma sürecindeyse veya ateş edemiyorsa işlemi durdur

        cooldownTimer = GetCurrentWeaponData().shootRate; // Soğuma süresini güncelle
        GetCurrentWeapon().weaponSound.Play(); // Silah sesini çal
        inCooldown = true; // Soğuma sürecinde olduğunu işaretle

        SpawnBullet(); // Kurşun oluştur
    }

    private void SpawnBullet()
    {
        // Kurşun prefabından bir kurşun oluştur ve başlangıç noktasına yerleştir
        Bullet bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.transform.forward = bulletSpawnPoint.forward; // Kurşunun yönünü ayarla
        bullet.SetBulletDamage(GetCurrentWeaponData().damage); // Kurşunun hasarını ayarla
    }

    private void ChangeWeapon()
    {
        // Mevcut silah verisi dizin indeksi son silahın dizinin sonunda ise ilk silaha geç
        if (currentWeaponDataIndex >= weaponDataArray.Length - 1)
        {
            currentWeaponDataIndex = 0;
        }
        else
        {
            currentWeaponDataIndex++;
        }

        OnAnyWeaponChanged?.Invoke(GetCurrentWeaponData()); // Silah değiştiğinde eventi tetikle
        ChangeWeaponMesh(); // Silahın modelini değiştir
    }

    private void ChangeWeapon(int index)
    {
        currentWeaponDataIndex = index; // Belirtilen indeksteki silaha geç

        OnAnyWeaponChanged?.Invoke(GetCurrentWeaponData()); // Silah değiştiğinde eventi tetikle
        ChangeWeaponMesh(); // Silahın modelini değiştir
    }

    private void ChangeWeaponMesh()
    {
        if (weaponArray.Length <= 0)
        {
            Debug.LogError("Silah Dizisinde Silah Bulunamadı! " + transform); // Hata: Silah dizisinde silah bulunamadı
            return;
        }

        // Silah dizisindeki tüm silahları devre dışı bırak
        for (int i = 0; i < weaponArray.Length; i++)
        {
            weaponArray[i].weaponObject.SetActive(false);
        }

        bool weaponFound = false;

        // Silah dizisindeki her silah için
        foreach (Weapon weapon in weaponArray)
        {
            // Mevcut silah verisi dizinindeki silah ismiyle eşleşen silahı aktif hale getir
            if (weapon.weaponName == GetCurrentWeaponData().weaponName)
            {
                weapon.weaponObject.SetActive(true);
                bulletSpawnPoint = weapon.bulletSpawnPoint; // Kurşun başlangıç noktasını güncelle
                weaponFound = true;
            }
        }

        if (!weaponFound)
        {
            Debug.LogError("Belirtilen isimde silah bulunamadı " + GetCurrentWeaponData().weaponName + " Silah Dizisinde! " + transform); // Hata: Belirtilen isimde silah bulunamadı
            return;
        }
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable(); // Oyuncu kontrollerini etkinleştir

        PlayerSpawner.OnAnySpawnStarted += PlayerSpawner_OnAnySpawnStarted; // Herhangi bir spawn işlemi başladığında tetiklenecek eventi dinle
        PlayerSpawner.OnAnySpawnCompleted += PlayerSpawner_OnAnySpawnCompleted; // Herhangi bir spawn işlemi tamamlandığında tetiklenecek eventi dinle
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable(); // Oyuncu kontrollerini devre dışı bırak

        PlayerSpawner.OnAnySpawnStarted -= PlayerSpawner_OnAnySpawnStarted; // Spawn işlemi başladığında tetiklenen eventi dinlemeyi durdur
        PlayerSpawner.OnAnySpawnCompleted -= PlayerSpawner_OnAnySpawnCompleted; // Spawn işlemi tamamlandığında tetiklenen eventi dinlemeyi durdur
    }

    public WeaponData GetCurrentWeaponData()
    {
        return weaponDataArray[currentWeaponDataIndex]; // Mevcut silah verisini döndür
    }

    public Weapon GetCurrentWeapon()
    {
        Weapon currentWeapon = weaponArray[currentWeaponDataIndex]; // Mevcut silahı al

        // Silah dizisindeki her silah için
        foreach (Weapon weapon in weaponArray)
        {
            // Mevcut silah verisi dizinindeki silah ismiyle eşleşen silahı döndür
            if (weapon.weaponName == GetCurrentWeaponData().weaponName)
            {
                currentWeapon = weapon;
            }
        }

        return currentWeapon; // Mevcut silahı döndür
    }

    private void DisablePlayerShoot()
    {
        canShoot = false; // Oyuncunun ateş etmesini engelle
    }

    private void EnablePlayerShoot()
    {
        canShoot = true; // Oyuncunun ateş etmesine izin ver
    }

    private void PlayerSpawner_OnAnySpawnStarted()
    {
        DisablePlayerShoot(); // Oyuncu spawn işlemi başladığında ateş etmeyi engelle
    }

    private void PlayerSpawner_OnAnySpawnCompleted()
    {
        EnablePlayerShoot(); // Oyuncu spawn işlemi tamamlandığında ateş etmeye izin ver
    }

    [System.Serializable]
    public struct Weapon
    {
        public string weaponName; // Silahın adı
        public GameObject weaponObject; // Silahın oyun nesnesi
        public Transform bulletSpawnPoint; // Kurşun başlangıç noktası
        public AudioSource weaponSound; // Silahın ses kaynağı
    }
}
