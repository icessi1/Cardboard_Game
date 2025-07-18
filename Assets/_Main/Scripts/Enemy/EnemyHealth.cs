using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public event Action OnDied;           // Bu düşman öldüğünde tetiklenen olay
    public static event Action OnAnyEnemyDied;  // Herhangi bir düşmanın öldüğünde tetiklenen statik olay

    [SerializeField] private int maxHealth;                 // Düşmanın maksimum sağlığı
    [SerializeField] private Renderer enemyMeshRenderer;    // Düşmanın görsel bileşeni
    [SerializeField] private GameObject enemyDeathParticlePrefabObject;  // Ölüm efektleri için prefab

    private int currentHealth;   // Düşmanın mevcut sağlığı

    private void Start()
    {
        currentHealth = maxHealth;  // Başlangıçta mevcut sağlığı maksimum sağlık değerine eşitle
    }

    public void Damage(int damage)
    {
        currentHealth -= damage;  // Aldığı hasar kadar mevcut sağlığı azalt

        if (currentHealth <= 0)
        {
            // Düşman öldü
            OnDied?.Invoke();  // Belirli düşmanın ölümü için OnDied olayını tetikle
            OnAnyEnemyDied?.Invoke();  // Herhangi bir düşmanın ölümü için statik OnAnyEnemyDied olayını tetikle

            currentHealth = 0;  // Sağlığı sıfırla
            DeathParticle();    // Ölüm efektlerini oynat
            Destroy(gameObject);  // Düşman objesini yok et
        }

        StartCoroutine(ColorFlick());  // Renk değişimi efektini başlat
    }

    private IEnumerator ColorFlick()
    {
        Material enemyMaterial = enemyMeshRenderer.material;  // Düşmanın materyalini al

        enemyMaterial.color = Color.white;  // Materyali beyaz yap
        enemyMaterial.SetColor("_EmissionColor", Color.white);  // Işık rengini beyaz yap

        enemyMeshRenderer.material = enemyMaterial;  // Materyali düşmana uygula

        yield return new WaitForSeconds(0.1f);  // 0.1 saniye bekle

        enemyMaterial.color = Color.red;  // Materyali kırmızı yap
        enemyMaterial.SetColor("_EmissionColor", Color.red);  // Işık rengini kırmızı yap

        enemyMeshRenderer.material = enemyMaterial;  // Materyali düşmana uygula
    }

    private void DeathParticle()
    {
        Instantiate(enemyDeathParticlePrefabObject, transform.position + new Vector3(0, 1.0f, 0), Quaternion.identity);  // Ölüm efektlerini oluştur
    }
}
