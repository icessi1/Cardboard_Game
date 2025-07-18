using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private Transform endGamePanelTransform;  // Oyun sonu paneli transformu
    [SerializeField] private GameObject textParentObject;      // Metinlerin ebeveyn objesi
    [SerializeField] private TextMeshProUGUI statusText;       // Durum metni (Victory veya Defeat)
    [SerializeField] private TextMeshProUGUI teamInfoText;     // Kazanan takım bilgisi metni
    [SerializeField] private TextMeshProUGUI countdownText;    // Geri sayım metni

    [Header("SOUNDS"), Space(10)]
    [SerializeField] private AudioSource backgroundMusicSource;  // Arkaplan müziği için ses kaynağı
    [SerializeField] private AudioSource victoryMusicSource;     // Zafer müziği için ses kaynağı
    [SerializeField] private AudioSource defeatMusicSource;      // Yenilgi müziği için ses kaynağı

    private float countdownTimer = 5.0f;  // Geri sayım süresi
    private bool startCountdown;          // Geri sayımın başlaması için bayrak
    private bool startScaling;            // Panel ölçeğinin büyütülmesi için bayrak

    private void Start()
    {
        LevelCompletion.OnAnyLevelCompleted += LevelCompletion_OnAnyLevelCompleted;  // Seviye tamamlandı olayına abone ol
    }

    private void OnDestroy()
    {
        LevelCompletion.OnAnyLevelCompleted -= LevelCompletion_OnAnyLevelCompleted;  // Seviye tamamlandı olayından aboneliği kaldır
    }

    private void Update()
    {
        if (startScaling)
        {
            // Paneli büyüt
            endGamePanelTransform.localScale += new Vector3(0.85f * Time.deltaTime, 0, 0);

            // Panel belirli bir büyüklüğe ulaştığında
            if (endGamePanelTransform.localScale.x >= 1.05f)
            {
                startCountdown = true;      // Geri sayım başlat
                textParentObject.SetActive(true);  // Metinleri aktif hale getir
                startScaling = false;       // Ölçeklendirmeyi durdur
            }
        }

        if (startCountdown)
        {
            countdownTimer -= Time.deltaTime;  // Geri sayım süresini azalt

            UpdateCountdown(countdownTimer);   // Geri sayım metnini güncelle

            // Geri sayım tamamlandığında
            if (countdownTimer <= 0)
            {
                startCountdown = false;       // Geri sayımı durdur
                countdownTimer = 5.0f;        // Geri sayım süresini sıfırla
                RestartScene();               // Sahneyi yeniden başlat
            }
        }
    }

    // Seviye tamamlandı olayı tetiklendiğinde
    private void LevelCompletion_OnAnyLevelCompleted(Team teamInfo)
    {
        endGamePanelTransform.gameObject.SetActive(true);  // Oyun sonu panelini aktif hale getir
        UpdateEndGameUI(teamInfo);  // Oyun sonu UI'sını güncelle
        startScaling = true;        // Panel ölçeğini büyütmeye başla

        backgroundMusicSource.Stop();  // Arkaplan müziğini durdur
    }

    // Oyun sonu UI'sını günceller
    private void UpdateEndGameUI(Team victoriousTeam)
    {
        UpdateStatus(Team.Blue, victoriousTeam);  // Durum metnini ve rengini günceller
        UpdateTeamInfo(victoriousTeam);          // Kazanan takım bilgisini günceller
    }

    // Durum metnini ve rengini günceller
    private void UpdateStatus(Team playerTeam, Team victoriousTeam)
    {
        if (victoriousTeam == playerTeam)
        {
            UpdateStatusText("VICTORY", Color.green);  // Zafer durumu metni ve rengi
            victoryMusicSource.Play();                 // Zafer müziğini çal
        }
        else
        {
            UpdateStatusText("DEFEAT", Color.red);     // Yenilgi durumu metni ve rengi
            defeatMusicSource.Play();                  // Yenilgi müziğini çal
        }
    }

    // Durum metnini günceller
    private void UpdateStatusText(string status, Color color)
    {
        statusText.text = status;     // Durum metnini ayarla
        statusText.color = color;     // Durum rengini ayarla
    }

    // Kazanan takım bilgisini günceller
    private void UpdateTeamInfo(Team victoriousTeam)
    {
        switch (victoriousTeam)
        {
            case Team.Red:
                UpdateTeamInfoText("Red Team Won", new Color(1, 0.25f, 0.25f));  // Kırmızı takım kazandı metni ve rengi
                break;
            case Team.Blue:
                UpdateTeamInfoText("Blue Team Won", new Color(0, 0.75f, 1));     // Mavi takım kazandı metni ve rengi
                break;
        }
    }

    // Kazanan takım bilgisi metnini günceller
    private void UpdateTeamInfoText(string teamInfo, Color color)
    {
        teamInfoText.text = teamInfo;  // Takım bilgisi metnini ayarla
        teamInfoText.color = color;    // Takım bilgisi rengini ayarla
    }

    // Geri sayımı günceller
    private void UpdateCountdown(float countdown)
    {
        UpdateCountdownText("Restarting in " + countdown.ToString("0"));  // Geri sayım metnini ayarla
    }

    // Geri sayım metnini ayarlar
    private void UpdateCountdownText(string countdown)
    {
        countdownText.text = countdown;  // Geri sayım metnini ayarla
    }

    // Sahneyi yeniden başlatır
    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Aktif sahneyi yeniden yükle
    }
}
