using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    // Herhangi bir spawn işlemi başladığında tetiklenecek olay
    public static event Action OnAnySpawnStarted;
    // Herhangi bir spawn işlemi tamamlandığında tetiklenecek olay
    public static event Action OnAnySpawnCompleted;

    // Spawn işleminin durumunu belirten enum
    public enum State
    {
        Fadeout = 0, // Kararma
        Spawn = 1,   // Oyuncu spawn
        Fadein = 2   // Solma
    }

    [SerializeField] private Image fadeImage; // Kararma efekti için resim
    [SerializeField] private Transform spawnPointTransform; // Oyuncunun spawn olacağı nokta

    private State state; // Geçerli spawn durumu
    private float stateTimer; // Durum geçiş zamanlayıcısı
    private bool isSpawning; // Spawn işlemi devam ediyor mu?

    private void Start()
    {
        // Herhangi bir oyuncu öldüğünde tetiklenecek olaya abone ol
        PlayerHealth.OnAnyPlayerDied += PlayerHealth_OnAnyPlayerDied;
    }

    private void OnDestroy()
    {
        // Oyuncu ölüm olayı aboneliğini iptal et
        PlayerHealth.OnAnyPlayerDied -= PlayerHealth_OnAnyPlayerDied;
    }

    private void Update()
    {
        // Eğer spawn işlemi devam etmiyorsa, geri dön
        if (!isSpawning) return;

        // Duruma göre işlem yap
        switch (state)
        {
            case State.Fadeout:
                // Kararma tamamlanana kadar bekle
                if (fadeImage.color.a >= 1) break;

                // Kararma efektini uygula
                ColorFade(2);
                break;

            case State.Spawn:
                // Spawn durumunda ek bir işlem yok, sadece geçiş yapılacak
                break;

            case State.Fadein:
                // Solma tamamlanana kadar bekle
                if (fadeImage.color.a <= 0) break;

                // Solma efektini uygula
                ColorFade(-2);
                break;
        }

        // Durum zamanlayıcısını azalt
        stateTimer -= Time.deltaTime;

        // Bir sonraki duruma geç
        NextState();
    }

    // Bir sonraki duruma geçiş işlemi
    private void NextState()
    {
        // Zamanlayıcı henüz sıfırlanmadıysa işlem yapma
        if (stateTimer > 0) return;

        // Duruma göre işlem yap
        switch (state)
        {
            case State.Fadeout:
                // Kararma durumu tamamlandı, spawn durumuna geç
                state = State.Spawn;
                stateTimer = 0.5f; // Spawn işlemi için geçiş süresi
                Spawn(); // Oyuncuyu spawn et
                Debug.Log("Kararma Tamamlandı");
                break;

            case State.Spawn:
                // Spawn durumu tamamlandı, solma durumuna geç
                state = State.Fadein;
                stateTimer = 1.0f; // Solma işlemi için geçiş süresi
                Debug.Log("Spawn Tamamlandı");
                break;

            case State.Fadein:
                // Solma durumu tamamlandı, spawn işlemi tamamlandı olarak işaretle
                isSpawning = false;
                state = State.Fadeout;
                stateTimer = 1.0f; // Kararma işlemi için geçiş süresi

                // Herhangi bir spawn işlemi tamamlandı olayını tetikle
                OnAnySpawnCompleted?.Invoke();
                Debug.Log("Solma Tamamlandı");
                break;
        }
    }

    // Kararma efektini uygular
    private void ColorFade(float value)
    {
        Color fadeImageColor = fadeImage.color;
        fadeImageColor.a += value * Time.deltaTime; // Alfa değerini zamanla değiştir
        fadeImage.color = fadeImageColor;
    }

    // Oyuncuyu spawn eder
    private void Spawn()
    {
        if (spawnPointTransform != null)
        {
            // Spawn noktası bulunduğunda konumu ayarla
            Debug.Log("Spawn Noktası Bulundu!");
            transform.position = new Vector3(spawnPointTransform.position.x, transform.position.y, spawnPointTransform.position.z);
        }
        else
        {
            // Spawn noktası bulunamadığında varsayılan konuma ayarla
            Debug.Log("Spawn Noktası Bulunamadı!");
            transform.position = new Vector3(0, transform.position.y, 0);
        }
    }

    // Yeni bir spawn işlemi başlatır
    public void StartRespawn()
    {
        stateTimer = 1;
        state = State.Fadeout; // Kararma durumundan başla
        isSpawning = true; // Spawn işlemi başladı olarak işaretle
        OnAnySpawnStarted?.Invoke(); // Herhangi bir spawn işlemi başladı olayını tetikle
    }

    // Herhangi bir oyuncu öldüğünde spawn işlemini başlatır
    private void PlayerHealth_OnAnyPlayerDied()
    {
        StartRespawn();
    }
}
