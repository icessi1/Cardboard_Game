using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable, ITeamManager
{
    // Herhangi bir oyuncu öldüğünde tetiklenecek olay
    public static event Action OnAnyPlayerDied;
    // Bu oyuncu öldüğünde tetiklenecek olay
    public event Action OnDied;

    [SerializeField] private int maxHealth = 100; // Maksimum sağlık değeri
    [SerializeField] private Team team; // Oyuncunun takımı
    [SerializeField] private Image damageImage; // Hasar aldığında görüntülenecek resim
    [SerializeField] private AudioSource damageSound; // Hasar aldığında çalacak ses

    private int currentHealth; // Oyuncunun mevcut sağlığı

    private void Start()
    {
        // Oyuncu spawn tamamlandığında tetiklenecek olaya abone ol
        PlayerSpawner.OnAnySpawnCompleted += PlayerSpawner_OnAnySpawnCompleted;

        // Sağlığı başlat
        Initialize();
    }

    private void OnDestroy()
    {
        // Olay aboneliğini iptal et
        PlayerSpawner.OnAnySpawnCompleted -= PlayerSpawner_OnAnySpawnCompleted;
    }

    // Sağlığı başlatır
    private void Initialize()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Eğer damageImage'in alfa değeri 0'dan büyükse, renk değiştir
        if (damageImage.color.a > 0)
        {
            ColorChangeGradually(-2);
        }
    }

    // IDamageable arayüzünden gelen hasar alma metodu
    public void Damage(int damage)
    {
        TakeDamage(damage);
    }

    // Hasar alma işlemini gerçekleştirir
    private void TakeDamage(int damage)
    {
        currentHealth -= damage; // Sağlığı azalt

        damageSound.Play(); // Hasar sesi çal

        if (currentHealth <= 0)
        {
            // Oyuncu öldü
            currentHealth = 0;

            OnAnyPlayerDied?.Invoke(); // Herhangi bir oyuncu öldü olayını tetikle
            OnDied?.Invoke(); // Bu oyuncu öldü olayını tetikle
            return;
        }

        // Anında renk değiştir
        ColorChangeInstantly(0.75f);
    }

    // Rengi anında değiştirir
    private void ColorChangeInstantly(float value)
    {
        Color fadeImageColor = damageImage.color;
        fadeImageColor.a = value; // Alfa değerini ayarla
        damageImage.color = fadeImageColor;
    }

    // Rengi kademeli olarak değiştirir
    private void ColorChangeGradually(float value)
    {
        Color fadeImageColor = damageImage.color;
        fadeImageColor.a += value * Time.deltaTime; // Alfa değerini kademeli olarak değiştir
        damageImage.color = fadeImageColor;
    }

    // Oyuncu spawn tamamlandığında sağlığı maksimuma ayarlar
    private void PlayerSpawner_OnAnySpawnCompleted()
    {
        currentHealth = maxHealth;
    }

    // Oyuncunun takımını döner
    public Team GetTeam()
    {
        return team;
    }
}
