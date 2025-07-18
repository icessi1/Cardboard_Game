using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private const int ENEMY_COUNT_CHECK_FREQUENCY = 10;

    [SerializeField] private Transform spawnPointParent;     // Düşmanların doğacağı noktaların üst ebeveyni
    [SerializeField] private Transform defencePointParent;   // Savunma noktalarının üst ebeveyni
    [SerializeField] private EnemyAI enemyAIPrefab;          // Düşman yapay zeka örneği prefabı
    [SerializeField, Range(1, 20)] private int targetConcurrentEnemyCount = 1;  // Aynı anda doğacak düşman sayısı

    private List<Transform> spawnPointList;       // Doğum noktalarının listesi
    private List<DefencePoint> defencePointList;  // Savunma noktalarının listesi
    private int concurrentEnemyCount;             // Aynı anda var olan düşman sayısı
    private float enemyCheckTimer;                // Düşman sayısını kontrol etme zamanlayıcısı
    private Flag targetFlag;                      // Hedef bayrak

    private void Awake()
    {
        InitializeSpawnPoints();    // Doğum noktalarını başlat
        InitializeDefencePoints();  // Savunma noktalarını başlat
        GetPlayerFlag();            // Oyuncu bayrağını al
    }

    // Doğum noktalarını başlatma
    private bool InitializeSpawnPoints()
    {
        if (spawnPointParent.childCount <= 0)
        {
            Debug.LogError("Doğum noktası üst ebeveyninde doğum noktası bulunamadı! " + spawnPointParent);
            return false;
        }

        spawnPointList = new List<Transform>();  // Doğum noktaları listesini oluştur
        spawnPointList.Clear();

        for (int i = 0; i < spawnPointParent.childCount; i++)
        {
            spawnPointList.Add(spawnPointParent.GetChild(i));  // Tüm çocuk doğum noktalarını listeye ekle
        }

        return true;
    }

    // Savunma noktalarını başlatma
    private bool InitializeDefencePoints()
    {
        if (defencePointParent.childCount <= 0)
        {
            Debug.LogError("Savunma noktası üst ebeveyninde savunma noktası bulunamadı! " + defencePointParent);
            return false;
        }

        defencePointList = new List<DefencePoint>();  // Savunma noktaları listesini oluştur
        defencePointList.Clear();

        for (int i = 0; i < defencePointParent.childCount; i++)
        {
            DefencePoint newDefencePoint = new DefencePoint(defencePointParent.GetChild(i));  // Her savunma noktası için yeni bir savunma noktası oluştur
            defencePointList.Add(newDefencePoint);  // Oluşturulan savunma noktasını listeye ekle
        }

        return true;
    }

    // Oyuncu bayrağını al
    private void GetPlayerFlag()
    {
        Flag[] flags = FindObjectsOfType<Flag>();  // Sahnedeki tüm bayrakları bul

        foreach (Flag flag in flags)
        {
            if (flag.GetTeam() == enemyAIPrefab.GetTeam())  // Bayrağın takımı düşman yapay zekasının takımına eşitse
            {
                targetFlag = flag;  // Hedef bayrağı ata
                break;
            }
        }
    }

    private void Start()
    {
        EnemyHealth.OnAnyEnemyDied += EnemyHealth_OnAnyEnemyDied;  // Herhangi bir düşmanın öldüğü zaman tetiklenecek olayı başlat
    }

    private void OnDestroy()
    {
        EnemyHealth.OnAnyEnemyDied -= EnemyHealth_OnAnyEnemyDied;  // Herhangi bir düşmanın öldüğü zaman tetiklenecek olayı sonlandır
    }

    private void Update()
    {
        enemyCheckTimer -= Time.deltaTime;  // Zamanlayıcıyı güncelle

        if (enemyCheckTimer <= 0)
        {
            CheckForConcurrentEnemyCount();  // Aynı anda var olan düşman sayısını kontrol et
            enemyCheckTimer = ENEMY_COUNT_CHECK_FREQUENCY;  // Zamanlayıcıyı yeniden ayarla
        }
    }

    // Aynı anda var olan düşman sayısını kontrol et
    private void CheckForConcurrentEnemyCount()
    {
        if (concurrentEnemyCount < targetConcurrentEnemyCount)
        {
            SpawnEnemy();  // Yeni düşman doğur
            concurrentEnemyCount++;  // Aynı anda var olan düşman sayısını artır
        }
    }

    // Düşman doğur
    private void SpawnEnemy()
    {
        EnemyAI spawnedEnemyAI = Instantiate(enemyAIPrefab, GetRandomSpawnPointPosition(), Quaternion.identity);  // Düşman yapay zekasını doğur

        EnemyAI.Target[] targetExceptions = { };  // Hedef seçiminde hariç tutulacak hedefler
        EnemyAI.Target selectedTarget = spawnedEnemyAI.SetRandomTarget(targetExceptions);  // Rastgele hedef seç

        spawnedEnemyAI.SetTargetFlag(targetFlag);  // Düşmana hedef bayrağı ata

        if (selectedTarget == EnemyAI.Target.Defend)  // Seçilen hedef savunma ise
            spawnedEnemyAI.SetDefencePoint(GetDefencePoint(spawnedEnemyAI));  // Düşmana bir savunma noktası ata
    }

    // Rastgele bir doğum noktası pozisyonunu al
    private Vector3 GetRandomSpawnPointPosition()
    {
        int randomIndex = Random.Range(0, spawnPointList.Count);  // Doğum noktaları listesinden rastgele bir indeks seç

        return spawnPointList[randomIndex].position;  // Seçilen indeksteki doğum noktasının pozisyonunu döndür
    }

    // Düşmana atanacak savunma noktasını al
    private DefencePoint GetDefencePoint(EnemyAI spawnedEnemyAI)
    {
        int availableIndex = -1;  // Kullanılabilir bir indeks

        for (int i = 0; i < defencePointList.Count; i++)
        {
            if (!defencePointList[i].GetOccupied())  // Savunma noktası işgal edilmemişse
            {
                defencePointList[i].SetOccupied(true);  // Savunma noktasını işgal et
                availableIndex = i;  // Kullanılabilir indeksi güncelle
                break;
            }
        }

        if (availableIndex == -1)  // Eğer kullanılabilir bir indeks bulunamazsa
        {
            EnemyAI.Target[] targetExceptions = { EnemyAI.Target.Defend, EnemyAI.Target.Capture };  // Savunma ve yakalama hedeflerini hariç tut
            spawnedEnemyAI.SetRandomTarget(targetExceptions);  // Yeni bir rastgele hedef ata
            availableIndex = 0;  // İlk savunma noktasını seç
        }

        return defencePointList[availableIndex];  // Seçilen savunma noktasını döndür
    }

    // Herhangi bir düşmanın öldüğü zaman tetiklenen olay
    private void EnemyHealth_OnAnyEnemyDied()
    {
        concurrentEnemyCount--;  // Aynı anda var olan düşman sayısını azalt
    }

    // Savunma noktası sınıfı
    [System.Serializable]
    public class DefencePoint
    {
        public Vector3 defencePosition;  // Savunma noktasının pozisyonu
        public bool isOccupied;          // Savunma noktasının işgal durumu

        public DefencePoint(Transform defencePoint)
        {
            defencePosition = defencePoint.position;  // Savunma noktasının pozisyonunu ata
            isOccupied = false;  // Savunma noktasını işgal edilmemiş olarak ayarla
        }

        // Savunma noktasının işgal durumunu ayarla
        public void SetOccupied(bool isOccupied)
        {
            this.isOccupied = isOccupied;
        }

        // Savunma noktasının işgal durumunu döndür
        public bool GetOccupied()
        {
            return isOccupied;
        }
    }
}
