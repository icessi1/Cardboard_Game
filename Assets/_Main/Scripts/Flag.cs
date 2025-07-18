using UnityEngine;

public class Flag : MonoBehaviour, ITeamManager
{
    [SerializeField] private Transform parentFlagObjectTransform;  // Bayrak ebeveyn nesnesinin transformu
    [SerializeField] private GameObject flagMesh;                 // Bayrak görselinin oyun nesnesi
    [SerializeField] private LayerMask collectLayerMask;          // Bayrağın toplanabileceği katman maskesi
    [SerializeField] private Team collectTeam;                    // Bayrağın ait olduğu takım
    [SerializeField] private MeshRenderer flagMeshRenderer;       // Bayrak görselinin mesh renderer'ı
    [SerializeField] private Material blueMaterial;               // Mavi takım için malzeme
    [SerializeField] private Material redMaterial;                // Kırmızı takım için malzeme
    [SerializeField] private AudioSource captureSound;            // Bayrak toplama sesi

    private Vector3 startObjectLocalPositionInParent;  // Başlangıçta bayrak konumu
    private IDamageable collectedEntityDamageable;    // Toplanan nesnenin hasar alabilir arabirimi
    private Transform collectedEntityTransform;       // Toplanan nesnenin transformu
    private bool flagCaptured;                        // Bayrağın ele geçirilip ele geçirilmediği
    private LevelCompletion targetLevelCompletion;    // Hedef seviye tamamlama nesnesi

    private void Start()
    {
        startObjectLocalPositionInParent = transform.localPosition;  // Bayrağın başlangıç konumunu kaydet

        SetFlagColor();        // Bayrağın rengini ayarla
        SetLevelCompletion();  // Hedef seviye tamamlama nesnesini belirle
    }

    private void SetFlagColor()
    {
        Material[] flagMaterials = flagMeshRenderer.materials;  // Bayrak görselinin malzemeleri

        // Bayrağın ait olduğu takıma göre malzemeyi ayarla
        if (collectTeam == Team.Red)
        {
            flagMaterials[0] = redMaterial;
        }
        else
        {
            flagMaterials[0] = blueMaterial;
        }

        flagMeshRenderer.materials = flagMaterials;  // Malzemeleri bayrak görseline uygula
    }

    private void SetLevelCompletion()
    {
        // Tüm seviye tamamlama nesnelerini bul
        LevelCompletion[] levelCompletions = FindObjectsOfType<LevelCompletion>();

        // Bayrağın ait olduğu takıma göre hedef seviye tamamlama nesnesini belirle
        foreach (LevelCompletion levelCompletion in levelCompletions)
        {
            if (levelCompletion.GetTeam() == GetTeam())
            {
                targetLevelCompletion = levelCompletion;
                Debug.Log("Hedef seviye tamamlama nesnesi belirlendi: " + targetLevelCompletion.name);
            }
        }

        if (targetLevelCompletion == null)
        {
            Debug.LogWarning("Dikkat: Bayrağın ait olduğu takıma uygun seviye tamamlama nesnesi bulunamadı!");
        }
    }

    // Bayrağı toplama işlemi
    private void CollectFlag(Transform collectedEntityTransform)
    {
        this.collectedEntityTransform = collectedEntityTransform;  // Toplanan nesnenin transformunu kaydet
        transform.SetParent(collectedEntityTransform);            // Bayrağı toplayan nesnenin alt nesnesi yap
        transform.localPosition = Vector3.zero + new Vector3(0, 1, 0);  // Yerleştirme konumunu ayarla

        flagMesh.SetActive(false);  // Bayrak görselini devre dışı bırak
        Debug.Log("Bayrak toplandı ve taşıyan nesnenin altına yerleştirildi: " + collectedEntityTransform.name);

        flagCaptured = true;         // Bayrağın toplandığını işaretle

        captureSound.Play();         // Toplama sesini çal

        // Toplanan nesne hasar alabilir arabirime sahipse
        if (collectedEntityTransform.TryGetComponent(out IDamageable damageable))
        {
            collectedEntityDamageable = damageable;  // Hasar alabilir arabirimi kaydet
            damageable.OnDied += IDamageable_OnDied;  // Ölüm olayına abone ol
        }
        else
        {
            Debug.LogError("Hata: " + collectedEntityTransform.name + " içinde hasar alabilir bir bileşen bulunamadı!");
        }
    }

    // Bayrağı düşürme işlemi
    private void DropFlag()
    {
        transform.SetParent(parentFlagObjectTransform);  // Bayrağı ebeveyn nesneye geri yerleştir
        transform.localPosition = startObjectLocalPositionInParent;  // Başlangıç konumunu ayarla
        Debug.Log("Bayrak düşürüldü ve başlangıç konumuna yerleştirildi.");

        flagMesh.SetActive(true);  // Bayrak görselini aktif hale getir

        // Toplanan nesne hasar alabilir arabirime sahipse
        if (collectedEntityDamageable != null)
        {
            collectedEntityDamageable.OnDied -= IDamageable_OnDied;  // Ölüm olayından aboneliği kaldır
        }
        collectedEntityTransform = null;
        collectedEntityDamageable = null;
        flagCaptured = false;
    }

    // Bir nesne bayrağa temas ettiğinde
    private void OnTriggerEnter(Collider other)
    {
        // Bayrağın zaten ele geçirilip ele geçirilmediğini kontrol et
        if (IsFlagCaptured())
        {
            Debug.Log("Bayrak zaten toplandığı için temas etkinliği geçersiz.");
            return;
        }

        // Temas eden nesnenin katman maskesini kontrol et
        if (!CheckLayerMask(other.gameObject))
        {
            Debug.Log("Geçersiz katman: " + LayerMask.LayerToName(other.gameObject.layer));
            return;
        }

        // Temas eden nesnenin takımını kontrol et
        if (!CheckTeam(other.gameObject))
        {
            Debug.Log("Geçersiz takım: " + other.gameObject.name);
            return;
        }

        // Bayrağı topla
        CollectFlag(other.transform);
    }

    // Hasar alabilir nesne öldüğünde
    private void IDamageable_OnDied()
    {
        // Bayrağı düşür
        DropFlag();
    }

    // Katman maskesini kontrol et
    private bool CheckLayerMask(GameObject collectedObject)
    {
        // Temas eden nesne bayrağın toplanabilir katman maskesi ile uyumlu mu?
        if (collectLayerMask.value == (collectLayerMask.value | (1 << collectedObject.layer)))
        {
            return true;  // Uyumluluğu doğrula
        }

        return false;  // Uyumsuzsa false döndür
    }

    // Temas eden nesnenin takımını kontrol et
    private bool CheckTeam(GameObject collectedObject)
    {
        // Temas eden nesne ITeamManager arabirimine sahip değilse
        if (!collectedObject.TryGetComponent(out ITeamManager teamManager))
        {
            Debug.LogError("Hata: " + collectedObject.name + " içinde takım yöneticisi bulunamadı!");
            return false;  // False döndür
        }

        // Temas eden nesnenin takımı bayrağın takımı ile aynı mı?
        if (teamManager.GetTeam() == this.collectTeam)
        {
            return true;  // Aynı takımsa true döndür
        }

        return false;  // Değilse false döndür
    }

    // Bayrağın takımını döndür
    public Team GetTeam()
    {
        return collectTeam;  // Bayrağın takımını döndür
    }

    // Bayrağın ele geçirilip ele geçirilmediğini döndür
    public bool IsFlagCaptured()
    {
        return flagCaptured;  // Bayrağın ele geçirilip ele geçirilmediğini döndür
    }

    // Toplanan nesnenin transformunu döndür
    public Transform GetCollectedEntityTransform()
    {
        return collectedEntityTransform;  // Toplanan nesnenin transformunu döndür
    }

    // Hedef seviye tamamlama pozisyonunu döndür
    public Vector3 GetLevelCompletionPosition()
    {
        // Hedef seviye tamamlama nesnesi yoksa hata mesajı göster ve boş vektör döndür
        if (targetLevelCompletion == null)
        {
            Debug.LogError("Hata: Seviye tamamlama nesnesi bulunamadı!");
            return Vector3.zero;
        }

        return targetLevelCompletion.transform.position;  // Hedef seviye tamamlama nesnesinin pozisyonunu döndür
    }
}

// Takım enumu
public enum Team
{
    Red,   // Kırmızı takım
    Blue   // Mavi takım
}
