using UnityEngine;

public class LaserSight : MonoBehaviour
{
    [SerializeField] private LineRenderer laserLineRenderer; // Çizgi renderleyici
    [SerializeField] private Transform laserStartPoint; // Lazerin başlangıç noktası
    [SerializeField] private LayerMask laserHitLayerMask; // Lazerin çarpabileceği katmanlar

    private void Start() 
    {
        // Lazerin başlangıç noktasını belirle
        laserLineRenderer.SetPosition(0, laserStartPoint.localPosition);
    }

    private void Update() 
    {
        // Lazer, başlangıç noktasından ileri doğru 100 birim mesafeye kadar bir ray çıkar
        if (Physics.Raycast(laserStartPoint.position, laserStartPoint.forward, out RaycastHit hit, 100.0f, laserHitLayerMask))
        {
            // Eğer bir şey çarparsa, lazerin son noktasını bu noktaya ayarla
            SetLaserPoints(new Vector3(0, 0, hit.distance));
        }
        else
        {
            // Eğer hiçbir şey çarpmazsa, lazerin son noktasını 100 birim mesafede ayarla
            SetLaserPoints(new Vector3(0, 0, 100.0f));
        }
    }

    // Lazerin son noktasını belirle
    private void SetLaserPoints(Vector3 position)
    {
        laserLineRenderer.SetPosition(1, position);
    }
}
