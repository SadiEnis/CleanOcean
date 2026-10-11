using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    // Unity Editörü'nde sağ tık menüsüne eklemek için bu attribute kullanılır.
    [CreateAssetMenu(fileName = "NewTrashData", menuName = "ScriptableObjects/Trash Data", order = 1)]
    public class TrashSO : ScriptableObject
    {
        [Header("Görsel Ayarlar")]
        [SerializeField] private string _trashName; 
        [SerializeField] private Sprite _trashSprite;

        [Header("Oyun Mantığı")]
        [SerializeField] private int _scoreValue = 10;
        [SerializeField] private TrashType _trashType; 
        
        
        public string TrashName => _trashName;
        public Sprite TrashSprite => _trashSprite;
        public int ScoreValue => _scoreValue;
        public TrashType Type => _trashType;
    }

// Çöp türlerini ayırmak için basit bir Enum (İsteğe bağlı ama projene çok uygun)
    public enum TrashType
    {
        Plastic,
        Metal,
        Glass,
        Organic,
    }
}