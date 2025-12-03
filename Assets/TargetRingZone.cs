using System.Collections.Generic;
using UnityEngine;

public class TargetRingZone : MonoBehaviour
{
    public enum RingColor { Red, Blue, White }
    
    [Header("Ring Settings")]
    public RingColor ringColor = RingColor.White;
    
    private HashSet<Collider> contactingColliders = new HashSet<Collider>();
    
    public float GetScore()
    {
        switch (ringColor)
        {
            case RingColor.Red: return 1.0f;
            case RingColor.Blue: return 0.7f;
            case RingColor.White: return 0.5f;
            default: return 0.5f;
        }
    }
    
    public int GetContactCount()
    {
        return contactingColliders.Count;
    }
    
    public void ClearContacts()
    {
        contactingColliders.Clear();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb != null)
        {
            contactingColliders.Add(other);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        contactingColliders.Remove(other);
    }
}
