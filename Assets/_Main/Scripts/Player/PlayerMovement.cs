using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // PlayerMovement sınıfının tekil örneği (Singleton)
    public static PlayerMovement Instance { get; private set; }

    // Sabit yerçekimi değeri
    private const float GRAVITY = -9.81f;

    [Header("Referanslar")]
    // Kameranın Transform bileşenine referans
    [SerializeField] private Transform cameraTransform;
    // CharacterController bileşenine referans
    [SerializeField] private CharacterController characterController;

    [Header("Değişkenler")]
    // Oyuncunun hareket hızı
    [SerializeField] private float moveSpeed = 1.0f;
    // Yerçekimi etkisi için çarpan
    [SerializeField] private float gravityMultiplier = 1.5f;

    // Oyuncu kontrolleri için PlayerControls örneği
    private PlayerControls controls;
    // Hareket girdisini saklamak için Vector2
    private Vector2 move;
    // Oyuncunun hareket edip edemeyeceğini kontrol eden bayrak
    private bool canMove = true;

    private void Awake()
    {
        #region Singleton
        // Zaten bir örnek varsa, tekrarı yok et
        if (Instance != null)
        {
            Debug.LogError("PlayerMovement örneği zaten mevcut!");
            Destroy(gameObject);
            return;
        }
        // Mevcut örneği statik Instance özelliğine ata
        Instance = this;
        #endregion

        // PlayerControls örneğini başlat
        controls = new PlayerControls();

        // Hareket girdisi olaylarına abone ol
        controls.Gameplay.Move.performed += context => move = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => move = Vector2.zero;
    }

    private void Update()
    {
        // Hareketi Update yönteminde ele al
        HandleMove();
    }

    private void HandleMove()
    {
        // Oyuncu hareket edemiyorsa, erken dönüş yap
        if (!canMove) return;

        // Kameranın yönelimine ve girdiye göre hareket değerini hesapla
        Vector3 moveValue = cameraTransform.right * move.x + cameraTransform.forward * move.y;
        // Hareket değerine yerçekimi uygulama
        moveValue.y += GRAVITY * gravityMultiplier;

        // CharacterController kullanarak karakteri hareket ettir
        characterController.Move(moveValue * moveSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        // Kontrolleri etkinleştir
        controls.Gameplay.Enable();

        // Spawn olaylarına abone ol
        PlayerSpawner.OnAnySpawnStarted += PlayerSpawner_OnAnySpawnStarted;
        PlayerSpawner.OnAnySpawnCompleted += PlayerSpawner_OnAnySpawnCompleted;
    }

    private void OnDisable()
    {
        // Kontrolleri devre dışı bırak
        controls.Gameplay.Disable();

        // Spawn olaylarından çık
        PlayerSpawner.OnAnySpawnStarted -= PlayerSpawner_OnAnySpawnStarted;
        PlayerSpawner.OnAnySpawnCompleted -= PlayerSpawner_OnAnySpawnCompleted;
    }

    public void DisablePlayerMovement()
    {
        // CharacterController'ı devre dışı bırak ve canMove'u false yap
        characterController.enabled = false;
        canMove = false;
    }

    public void EnablePlayerMovement()
    {
        // CharacterController'ı etkinleştir ve canMove'u true yap
        characterController.enabled = true;
        canMove = true;
    }

    private void PlayerSpawner_OnAnySpawnStarted()
    {
        // Spawn başladığında oyuncu hareketini devre dışı bırak
        DisablePlayerMovement();
    }

    private void PlayerSpawner_OnAnySpawnCompleted()
    {
        // Spawn tamamlandığında oyuncu hareketini etkinleştir
        EnablePlayerMovement();
    }
}
