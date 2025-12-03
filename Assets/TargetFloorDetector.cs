using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFloorDetector : MonoBehaviour
{
    [Tooltip("The LiftableBox script on the movable cube (usually on block_puzzle_c_tinker).")]
    public LiftableBox liftableBox;

    [Tooltip("How long the box must stay on the target floor to count as complete (seconds).")]
    public float holdDuration = 1.0f;

    [Tooltip("All ring zones on the target (should have TargetRingZone components).")]
    public TargetRingZone[] ringZones;

    [Header("Random target placement")]
    [Tooltip("Move the target to a random spot (relative to the floor reference) after every successful trial.")]
    public bool randomizeTargetOnSuccess = true;

    [Tooltip("Transform that should be moved when randomizing. Defaults to this object.")]
    public Transform targetTransform;

    [Tooltip("Scene floor center/origin used as the randomization reference. Leave null to use target's current position.")]
    public Transform floorCenter;

    [Tooltip("Local offset (world units) applied to the floor center before sampling.")]
    public Vector3 floorOffset = Vector3.zero;

    [Tooltip("Keep the target's current height when repositioning (otherwise use floorCenter height + offset).")]
    public bool preserveTargetHeight = false;

    [Tooltip("Vertical random range (meters) applied when preserveTargetHeight is false.")]
    public float verticalRandomRange = 20.0f;

    [Tooltip("How far from the reference point the target is allowed to move (meters).")]
    public float maxHorizontalRadius = 15.0f;

    [Tooltip("Minimum distance from the reference point to avoid repeatedly spawning at the same spot.")]
    public float minHorizontalRadius = 5f;

    [Header("Physical support")]
    [Tooltip("Optional existing collider that physically supports the box (must have Is Trigger unchecked).")]
    public Collider supportSurfaceCollider;

    [Tooltip("Create a simple support collider automatically if none is assigned.")]
    public bool autoCreateSupportCollider = true;

    [Tooltip("Size of the auto-created support collider when auto-alignment is disabled or ring bounds are unavailable.")]
    public Vector3 supportColliderSize = new Vector3(0.9f, 0.05f, 0.9f);

    [Tooltip("Local offset for the auto-created support collider.")]
    public Vector3 supportColliderOffset = new Vector3(0f, -0.02f, 0f);

    [Tooltip("Physic material applied to the auto-created support collider.")]
    public PhysicMaterial supportPhysicMaterial;

    [Tooltip("Match the support collider footprint to the outermost ring bounds (prefers White > Blue > Red).")]
    public bool autoAlignSupportToOuterRing = true;

    [Tooltip("Minimum thickness (Y size) for the support collider when auto aligning.")]
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
