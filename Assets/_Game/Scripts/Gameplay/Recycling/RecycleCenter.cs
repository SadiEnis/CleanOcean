using System;
using Unity.Netcode;
using UnityEngine;

public class RecycleCenter : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer)  return;
        Debug.Log("Trigger Enter: " + other.gameObject.name);
        
        Interfaces.IRecyclable recyclableObject = other.gameObject.GetComponent<Interfaces.IRecyclable>();
        if (recyclableObject != null)
        {
            Debug.Log("Geri dönüştürme işlemi tetiklendi.");
            GameEventsManager.Instance.TriggerOnEnterRecyclableObject(recyclableObject);
            // --> Obje recyclable obje ise o türde bir obje trigger edildiğine dair event abonelerine haber gönderir.
            // Örn Trash bu event'i yakalar kendini yok eder vs ya da belli UI güncellemeleri olacaksa onlara da haber gidebilir.
        }
    }
}
