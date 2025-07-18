using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 1.0f;  // Mermi hızı
    [SerializeField] private GameObject hitParticle;   // Mermi bir şeye çarptığında oluşturulacak parçacık efekti
    [SerializeField] private LayerMask hitLayer;       // Mermi tarafından vurulabilecek nesnelerin katmanı

    private int bulletDamage = 10;  // Mermi hasarı

    private void Start()
    {
        // Eğer mermi bir şeye çarpmazsa, 5 saniye sonra yok edilir
        Invoke(nameof(KillBullet), 5.0f);
    }

    private void Update()
    {
        Move();  // Mermiyi ileri doğru hareket ettir
    }

    // Mermiyi ileri doğru hareket ettirir
    private void Move()
    {
        transform.position += transform.forward * bulletSpeed * Time.deltaTime;
    }

    // Mermi bir collider ile temas ettiğinde tetiklenir
    private void OnTriggerEnter(Collider other)
    {
        // Çarpışan nesnenin katmanı, hitLayer maskesi içinde ise işlem yapılır
        if (hitLayer.value == (hitLayer.value | (1 << other.gameObject.layer)))
        {
            // Çarpışan nesneden IDamageable bileşenini almayı dener
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(bulletDamage);  // Nesneye hasar verir
            }

            KillBullet();  // Mermiyi çarptıktan sonra yok eder
        }
    }

    // Çarpma parçacık efekti oluşturur ve mermiyi yok eder
    private void KillBullet()
    {
        Instantiate(hitParticle, transform.position, Quaternion.identity);  // Çarpma parçacık efekti oluştur
        Destroy(gameObject);  // Mermi oyun nesnesini yok et
    }

    // Mermi hasar değerini ayarlar
    public void SetBulletDamage(int damage)
    {
        bulletDamage = damage;
    }
}
