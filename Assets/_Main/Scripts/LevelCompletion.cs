using System;
using UnityEngine;

public class LevelCompletion : MonoBehaviour
{
    public static event Action<Team> OnAnyLevelCompleted; // Herhangi bir seviyenin tamamlandığını duyuran event

    [SerializeField] private LayerMask collectLayerMask; // Toplanacak nesnelerin katman maskesi
    [SerializeField] private Team collectTeam; // Toplama ekibini belirten Team tipinde değişken

    private static bool isLevelCompleted = false; // Seviyenin tamamlandığını belirten static flag

    private void Start()
    {
        isLevelCompleted = false; // Başlangıçta seviyenin tamamlanmadığına emin ol
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLevelCompleted) return; // Seviye zaten tamamlanmışsa işlemi durdur

        if (!CheckLayerMask(other.gameObject)) return; // Toplanacak nesne katmanına sahip değilse işlemi durdur

        if (!CheckTeam(other.gameObject)) return; // Toplama ekibiyle eşleşmiyorsa işlemi durdur

        CompleteLevel(); // Seviyeyi tamamla
    }

    private bool CheckLayerMask(GameObject collectedObject)
    {
        // Toplanacak nesnenin katmanı, belirlenen katman maskesine uygunsa true döner
        if (collectLayerMask.value == (collectLayerMask.value | (1 << collectedObject.layer)))
        {
            return true;
        }

        return false;
    }

    private bool CheckTeam(GameObject collectedObject)
    {
        // Toplanacak nesne ITeamManager arayüzünü içeriyorsa, ekibi kontrol eder
        if (!collectedObject.TryGetComponent(out ITeamManager teamManager))
        {
            Debug.LogError("No Team Manager found in " + collectedObject.name); // ITeamManager bulunamazsa hata verir
            return false;
        }

        // Nesnenin ekibi, toplama ekibine eşitse true döner
        if (teamManager.GetTeam() == this.collectTeam)
        {
            return true;
        }

        return false;
    }

    private void CompleteLevel()
    {
        Debug.Log("LEVEL COMPLETED"); // Seviye tamamlandı bilgisini konsola yazdır

        OnAnyLevelCompleted?.Invoke(collectTeam); // Herhangi bir seviyenin tamamlandığını dinleyen eventi tetikler

        isLevelCompleted = true; // Seviyenin tamamlandığını işaretler
    }

    public Team GetTeam()
    {
        return collectTeam; // Toplama ekibini döndürür
    }
}
