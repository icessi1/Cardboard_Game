using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, ITeamManager
{
    // Hedef türleri
    public enum Target
    {
        Defend,   // Savunma
        Player,   // Oyuncu
        Capture   // Bayrak yakalama
    }

    [SerializeField] private Target target;           // Seçili hedef
    [SerializeField] private Team team;               // Düşmanın takımı
    [SerializeField] private LayerMask attackLayerMask; // Saldırı için katman maskesi

    private NavMeshAgent agent;                       // Navigasyon ajanı
    private EnemyAttack enemyAttack;                   // Düşman saldırı bileşeni
    private EnemyHealth enemyHealth;                   // Düşman sağlık bileşeni
    private Flag targetFlag;                          // Hedef bayrak
    private EnemySpawner.DefencePoint selectedDefencePoint; // Seçilen savunma noktası
    private bool isShooting;                          // Ateş ediyor mu?

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();         // Navigasyon ajanı bileşenini al
        enemyAttack = GetComponent<EnemyAttack>();     // Düşman saldırı bileşenini al
        enemyHealth = GetComponent<EnemyHealth>();     // Düşman sağlık bileşenini al

        agent.SetDestination(GetTargetLocation());     // Başlangıçta hedefe doğru hareket et

        enemyHealth.OnDied += EnemyHealth_OnDied;      // Düşman ölüm olayına abone ol
    }

    private void Update()
    {
        CheckForTargetTask();                         // Hedef görevini kontrol et
    }

    private void CheckForTargetTask()
    {
        // Hedef türüne göre işlem yap
        switch (target)
        {
            case Target.Defend:
                PlayerTarget();
                break;

            case Target.Player:
                PlayerTarget();
                break;

            case Target.Capture:
                PlayerTarget();
                break;
        }
    }

    private Vector3 GetTargetLocation()
    {
        // Hedef türüne göre hedef konumunu döndür
        switch (target)
        {
            case Target.Defend:
                return selectedDefencePoint.defencePosition; // Savunma noktasının konumu

            case Target.Player:
                return PlayerMovement.Instance.transform.position; // Oyuncunun konumu

            case Target.Capture:
                return CaptureTargetPosition(); // Bayrak yakalama hedefinin konumu

            default:
                return Vector3.zero;
        }
    }

    private Vector3 CaptureTargetPosition()
    {
        if (targetFlag == null)
        {
            Debug.LogWarning("Hedef bayrak boş " + transform + " / Hedef değiştiriliyor...");
            target = Target.Player; // Hedef bayrak yoksa oyuncuya yönel
            return Vector3.zero;
        }

        if (targetFlag.IsFlagCaptured())
        {
            if (targetFlag.GetCollectedEntityTransform() == transform)
            {
                // Bayrak benim tarafımdan yakalandı
                return targetFlag.GetLevelCompletionPosition(); // Seviye tamamlama noktasının konumu
            }
            else
            {
                // Bayrak başkası tarafından yakalandı
                target = Target.Player; // Hedefi oyuncuya değiştir
                return Vector3.zero;
            }
        }
        else
        {
            // Bayrağı yakala
            return GetTargetFlagPosition(); // Bayrağın konumu
        }
    }

    private void PlayerTarget()
    {
        AttackPlayer(GetTargetLocation()); // Oyuncuyu hedef al
    }

    private void AttackPlayer(Vector3 targetPosition)
    {
        if (isShooting) return; // Eğer ateş ediliyorsa işlem yapma

        Transform player = PlayerMovement.Instance.transform; // Oyuncunun transform bileşeni

        // Oyuncuya doğru bir raycast yaparak saldırıyı denetle
        if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out RaycastHit hit, 40.0f, attackLayerMask))
        {
            if (hit.transform == player)
            {
                agent.isStopped = true; // Navigasyonu durdur
                transform.LookAt(player, transform.up); // Oyuncuya doğru bak
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0); // Yalnızca y ekseni etrafında dön
                StartCoroutine(Shoot()); // Ateş etmeye başla
            }
            else
            {
                agent.isStopped = false; // Navigasyonu devam ettir
                agent.SetDestination(targetPosition); // Hedef konumuna git
            }
        }
        else
        {
            agent.isStopped = false; // Navigasyonu devam ettir
            agent.SetDestination(targetPosition); // Hedef konumuna git
        }
    }

    private IEnumerator Shoot()
    {
        if (enemyAttack == null)
        {
            Debug.LogWarning("Bu isimde düşman saldırı scripti yok " + this.name);
            yield break;
        }

        isShooting = true; // Ateş ediliyor olarak işaretle

        yield return new WaitForSeconds(Random.Range(0.25f, 0.5f)); // Rasgele bir süre bekle

        enemyAttack.SpawnBullet(); // Mermi oluştur
        enemyAttack.GetCurrentWeapon().weaponSound.Play(); // Silah sesini çal

        yield return new WaitForSeconds(enemyAttack.GetCurrentWeaponData().shootRate); // Silah ateş hızı kadar bekle

        isShooting = false; // Ateş etme durumu sona erdi
    }

    private void EnemyHealth_OnDied()
    {
        if (target == Target.Defend)
        {
            selectedDefencePoint.SetOccupied(false); // Savunma noktasını boş olarak işaretle
            Debug.Log("ÖLÜM");
        }
    }

    public Team GetTeam()
    {
        return team; // Düşmanın takımını döndür
    }

    public void SetTargetFlag(Flag flag)
    {
        targetFlag = flag; // Hedef bayrağı ayarla
    }

    private Vector3 GetTargetFlagPosition()
    {
        return targetFlag.transform.position; // Hedef bayrağın konumunu döndür
    }

    // Belirli hedefleri dışında rastgele bir hedef seç
    public Target SetRandomTarget(Target[] exceptions)
    {
        List<Target> targetList = new List<Target>();

        // İstisna olarak belirtilen hedefler dışındaki tüm hedefleri listeye ekle
        foreach (var value in System.Enum.GetValues(typeof(Target)))
        {
            if (!exceptions.Contains((Target)value))
            {
                targetList.Add((Target)value);
            }
        }

        // Eğer hedef listesi boşsa, hata ver ve oyuncuya hedefi oyuncuya ayarla
        if (targetList.Count <= 0)
        {
            Debug.LogError("Hedef listesi boş! / Oyuncu hedefine geçiliyor!");
            target = Target.Player;
            return Target.Player;
        }

        // Rastgele bir hedef seç ve hedefi ayarla
        int targetIndex = Random.Range(0, targetList.Count);
        target = targetList[targetIndex];
        return target;
    }

    // Savunma noktasını ayarla
    public void SetDefencePoint(EnemySpawner.DefencePoint defencePoint)
    {
        selectedDefencePoint = defencePoint;
    }
}
