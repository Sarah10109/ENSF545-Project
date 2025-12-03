using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFloorDetector : MonoBehaviour
{
    public LiftableBox liftableBox;

    public float holdDuration = 1.0f;

    public TargetRingZone[] ringZones;

    [Header("Random target placement")]
    public bool randomizeTargetOnSuccess = true;

    public Transform targetTransform;

    public Transform floorCenter;

    public Vector3 floorOffset = new Vector3(10f, 0.01f, 0f);

    public bool preserveTargetHeight = false;

    public float verticalRandomRange = 20.0f;

    public float maxHorizontalRadius = 15.0f;

    public float minHorizontalRadius = 5f;

    [Header("Physical support")]
    public Collider supportSurfaceCollider;

    public bool autoCreateSupportCollider = true;

    public Vector3 supportColliderSize = new Vector3(0.9f, 0.05f, 0.9f);

    public Vector3 supportColliderOffset = new Vector3(0f, -0.02f, 0f);

    public PhysicMaterial supportPhysicMaterial;

    public bool autoAlignSupportToOuterRing = true;

    public float supportColliderMinThickness = 0.03f;

    private bool boxInside = false;
    private float insideTimer = 0f;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool initialTransformCaptured = false;

    private void Reset()
    {
        if (liftableBox == null)
            liftableBox = FindObjectOfType<LiftableBox>();
        
        if (ringZones == null || ringZones.Length == 0)
            ringZones = FindObjectsOfType<TargetRingZone>();

        if (floorCenter == null)
        {
            var floor = GameObject.FindWithTag("Floor");
            if (floor != null)
                floorCenter = floor.transform;
        }
    }

    private void Start()
    {
        if (ringZones == null || ringZones.Length == 0)
            ringZones = FindObjectsOfType<TargetRingZone>();

        if (targetTransform == null)
            targetTransform = transform;
        
        if (floorCenter == null)
        {
            var floor = GameObject.FindWithTag("Floor");
            if (floor != null)
                floorCenter = floor.transform;
        }

        CaptureInitialTransform();
        EnsureSupportCollider();
    }

    private void CaptureInitialTransform()
    {
        if (initialTransformCaptured)
            return;

        if (targetTransform == null)
            targetTransform = transform;

        initialLocalPosition = targetTransform.localPosition;
        initialLocalRotation = targetTransform.localRotation;
        initialTransformCaptured = true;
    }

    private void EnsureSupportCollider()
    {
        if (supportSurfaceCollider != null)
        {
            supportSurfaceCollider.isTrigger = false;
            if (supportPhysicMaterial != null)
                supportSurfaceCollider.material = supportPhysicMaterial;
            ConfigureSupportColliderGeometry();
            return;
        }

        if (!autoCreateSupportCollider)
            return;

        // Attempt to reuse an existing non-trigger collider on this object first
        var existingColliders = GetComponents<Collider>();
        foreach (var col in existingColliders)
        {
            if (!col.isTrigger)
            { 
                supportSurfaceCollider = col;
                if (supportPhysicMaterial != null)
                    supportSurfaceCollider.material = supportPhysicMaterial;
                return;
            }
        }

        // Create a simple box collider as physical support
        if (targetTransform == null)
            targetTransform = transform;

        GameObject supportGO = new GameObject("TargetSupportCollider");
        supportGO.transform.SetParent(targetTransform, false);
        supportGO.transform.localPosition = supportColliderOffset;
        supportGO.transform.localRotation = Quaternion.identity;
        supportGO.transform.localScale = Vector3.one;

        BoxCollider box = supportGO.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.size = supportColliderSize;
        box.center = supportColliderOffset;
        if (supportPhysicMaterial != null)
            box.material = supportPhysicMaterial;

        supportSurfaceCollider = box;
        ConfigureSupportColliderGeometry();
    }

    private void ConfigureSupportColliderGeometry()
    {
        if (!(supportSurfaceCollider is BoxCollider box))
            return;

        if (!autoAlignSupportToOuterRing || ringZones == null || ringZones.Length == 0)
        {
            box.center = supportColliderOffset;
            box.size = supportColliderSize;
            return;
        }

        if (!TryGetOuterRingBounds(out Bounds bounds))
        {
            box.center = supportColliderOffset;
            box.size = supportColliderSize;
            return;
        }

        if (targetTransform == null)
            targetTransform = transform;

        Vector3 localCenter = targetTransform.InverseTransformPoint(bounds.center);
        Vector3 localSize = targetTransform.InverseTransformVector(bounds.size);
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));

        box.center = new Vector3(localCenter.x, supportColliderOffset.y, localCenter.z);
        box.size = new Vector3(
            Mathf.Max(localSize.x, supportColliderSize.x),
            Mathf.Max(supportColliderMinThickness, supportColliderSize.y),
            Mathf.Max(localSize.z, supportColliderSize.z)
        );
    }

    private bool TryGetOuterRingBounds(out Bounds bounds)
    {
        bounds = new Bounds();
        bool initialized = false;
        int bestPriority = -1;

        foreach (var zone in ringZones)
        {
            if (zone == null)
                continue;

            Collider col = zone.GetComponent<Collider>();
            if (col == null)
                continue;

            int priority = GetRingPriority(zone.ringColor);
            if (!initialized || priority > bestPriority)
            {
                bounds = col.bounds;
                bestPriority = priority;
                initialized = true;
            }
            else if (priority == bestPriority)
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        return initialized;
    }

    private int GetRingPriority(TargetRingZone.RingColor ringColor)
    {
        switch (ringColor)
        {
            case TargetRingZone.RingColor.White: return 3;
            case TargetRingZone.RingColor.Blue: return 2;
            case TargetRingZone.RingColor.Red: return 1;
            default: return 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (liftableBox == null)
            return;

        if (!liftableBox.TrialActive || liftableBox.TrialCompleted)
            return;

        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb == liftableBox)
        {
            boxInside = true;
            insideTimer = 0f;
            ClearAllZoneContacts();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!boxInside || liftableBox == null)
            return;

        if (!liftableBox.TrialActive || liftableBox.TrialCompleted)
            return;

        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb != liftableBox)
            return;

        insideTimer += Time.deltaTime;

        if (insideTimer >= holdDuration)
        {
            boxInside = false;
            insideTimer = 0f;

            float score = CalculateScore();
            liftableBox.CompleteTrial(score);
            RepositionTarget();
            ClearAllZoneContacts();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LiftableBox lb = other.GetComponentInParent<LiftableBox>();
        if (lb == liftableBox)
        {
            boxInside = false;
            insideTimer = 0f;
            ClearAllZoneContacts();
        }
    }

    private void ClearAllZoneContacts()
    {
        if (ringZones != null)
        {
            foreach (var zone in ringZones)
            {
                if (zone != null)
                    zone.ClearContacts();
            }
        }
    }

    private float CalculateScore()
    {
        if (ringZones == null || ringZones.Length == 0)
            return 0.5f;

        TargetRingZone dominantZone = null;
        int maxContacts = 0;
        
        foreach (var zone in ringZones)
        {
            if (zone != null)
            {
                int contacts = zone.GetContactCount();
                if (contacts > maxContacts)
                {
                    maxContacts = contacts;
                    dominantZone = zone;
                }
            }
        }

        return dominantZone != null ? dominantZone.GetScore() : 0.5f;
    }

    private void RepositionTarget()
    {
        if (!randomizeTargetOnSuccess || targetTransform == null)
            return;

        // Determine reference point using the floor center (if provided)
        Vector3 referencePoint = targetTransform.position;
        if (floorCenter != null)
            referencePoint = floorCenter.position + floorOffset;

        // Clamp values to avoid invalid ranges.
        float radius = Mathf.Max(0f, maxHorizontalRadius);
        float minRadius = Mathf.Clamp(minHorizontalRadius, 0f, radius);

        if (radius <= 0f)
            return;

        // Sample until we get a point that satisfies the minimum radius (limited attempts).
        Vector2 planarOffset = Vector2.zero;
        const int maxAttempts = 8;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            planarOffset = Random.insideUnitCircle * radius;
            if (planarOffset.magnitude >= minRadius)
                break;
        }

        // Position target relative to reference point (optionally keep same Y height)
        Vector3 newPosition = referencePoint + new Vector3(planarOffset.x, 0f, planarOffset.y);

        if (preserveTargetHeight)
        {
            newPosition.y = targetTransform.position.y;
        }
        else
        {
            float baseHeight = referencePoint.y;
            if (verticalRandomRange > 0f)
            {
                baseHeight += Random.Range(-verticalRandomRange, verticalRandomRange);
            }
            newPosition.y = baseHeight;
        }

        targetTransform.position = newPosition;
    }

    public void ResetTargetToStart()
    {
        if (targetTransform == null)
            targetTransform = transform;

        CaptureInitialTransform();
        targetTransform.localPosition = initialLocalPosition;
        targetTransform.localRotation = initialLocalRotation;
    }
}
