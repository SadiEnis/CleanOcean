using System;
using Unity.Netcode;

namespace _Game.Scripts.Gameplay.Spawning
{
    [Serializable]
    public struct SpawnableObject
    {
        public string Name;
        public NetworkObject Prefab;

        [UnityEngine.Range(0, 100)] // Oluşturulma ihtimali -- Şuan rastgele ama ileride lazım olacak..
        public int SpawnWeight;
    }
}