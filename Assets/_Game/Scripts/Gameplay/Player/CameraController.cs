using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Takip Ayarları")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothTime = 0.25f;

    [Header("Sınır Ayarları")]
    [SerializeField] private bool limitCamera = true; // Sınırlandırmayı açıp kapatmak için
    // Kameranın X ve Y ekseninde gidebileceği en küçük ve en büyük değerler
    [SerializeField] private Vector2 minPosition; 
    [SerializeField] private Vector2 maxPosition;

    private Transform _target;
    private Vector3 _currentVelocity = Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.gameObject != null)
            {
                 Destroy(Instance.gameObject);
            }
        }
        Instance = this;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        // Hedef ilk atandığında direkt oraya ışınlanalım (Clamping uygulayarak)
        Vector3 targetPos = _target.position + offset;
        if (limitCamera)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minPosition.x, maxPosition.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minPosition.y, maxPosition.y);
        }
        transform.position = targetPos;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // 1. Hedef pozisyonu hesapla
        Vector3 targetPos = _target.position + offset;

        // 2. Smooth hareket uygula
        Vector3 smoothedPos = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref _currentVelocity,
            smoothTime);

        // 3. HESAPLANAN POZİSYONU SINIRLA (CLAMP)
        if (limitCamera)
        {
            // Mathf.Clamp(değer, min, max) -> Değeri min ve max arasında tutar.
            smoothedPos.x = Mathf.Clamp(smoothedPos.x, minPosition.x, maxPosition.x);
            smoothedPos.y = Mathf.Clamp(smoothedPos.y, minPosition.y, maxPosition.y);
        }

        // 4. Son pozisyonu uygula
        transform.position = smoothedPos;
    }

    // Sadece Unity Editöründe çalışır, sınırları çizmemizi sağlar
    private void OnDrawGizmos()
    {
        if (limitCamera)
        {
            Gizmos.color = Color.red;
            // Sınırları belirleyen bir kutu çiz
            Vector3 center = new Vector3(
                (minPosition.x + maxPosition.x) / 2f, 
                (minPosition.y + maxPosition.y) / 2f, 
                0);
            
            Vector3 size = new Vector3(
                Mathf.Abs(maxPosition.x - minPosition.x), 
                Mathf.Abs(maxPosition.y - minPosition.y), 
                1);

            Gizmos.DrawWireCube(center, size);
        }
    }
}