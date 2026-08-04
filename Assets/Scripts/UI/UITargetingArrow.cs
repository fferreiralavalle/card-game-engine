using System;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem; // Using New Input System

[RequireComponent(typeof(LineRenderer))]
public class UITargetingArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform arrowHead;
    [SerializeField] private Camera mainCamera;

    [Header("Curve Settings")]
    [SerializeField] private int pointCount = 30;
    [SerializeField] private float curveHeightMultiplier = 0.3f;
    [SerializeField] private float textureTileUnit = 1.0f; // Distance per texture loop

    [Header("Animation Settings")]
    [SerializeField] private float flowSpeed = .8f; // Speed of the advancing animation
    [SerializeField] private bool reverseDirection = false; // Toggle flow direction

    [Header("Width Profile")]
    [SerializeField] private float startWidth = 4f;
    [SerializeField] private float endWidth = 8f;

    public Action<UICardEntity, UITargetingArrow> onTargetChoosen;
    public InputAction clickAction;

    private Transform startTransform;
    public bool isTargeting { get; private set; }
    private float currentUvOffset = 0f;

    private void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        if (mainCamera == null) mainCamera = Camera.main;

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.positionCount = pointCount;

        clickAction.Enable();
        clickAction.performed += (ctx) => HandleDragEnd();

        SetTargetingActive(false);
    }

    private void Update()
    {
        if (!isTargeting || startTransform == null) return;

        Vector3 startPoint = startTransform.position;
        Vector3 endPoint = GetMouseWorldPosition();

        // 1. Calculate Control Point for Bezier Curve Arc
        Vector3 midPoint = (startPoint + endPoint) * 0.5f;
        float distance = Vector3.Distance(startPoint, endPoint);
        Vector3 controlPoint = midPoint + Vector3.up * (distance * curveHeightMultiplier);

        // 2. Sample Bezier Points & Calculate Path Length
        Vector3[] points = new Vector3[pointCount];
        float totalLength = 0f;
        Vector3 previousPoint = startPoint;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 currentPoint = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);
            points[i] = currentPoint;

            if (i > 0)
            {
                totalLength += Vector3.Distance(previousPoint, currentPoint);
            }
            previousPoint = currentPoint;
        }

        // 3. Update LineRenderer Positions
        lineRenderer.SetPositions(points);

        // 4. Update Texture Tiling dynamically based on Arc Length
        lineRenderer.material.mainTextureScale = new Vector2(totalLength / textureTileUnit, 1f);

        // 5. ANIMATE TEXTURE (Advancing Effect)
        // Shift UV offset continuously over time
        float direction = reverseDirection ? 1f : -1f;
        currentUvOffset += Time.deltaTime * flowSpeed * direction;

        // Keep offset between 0 and 1 to prevent precision loss over long play sessions
        currentUvOffset %= 1.0f;
        lineRenderer.material.mainTextureOffset = new Vector2(currentUvOffset, 0f);

        // 6. Orient Arrow Head at the Tip
        if (arrowHead != null)
        {
            arrowHead.position = endPoint;

            Vector3 tangent = CalculateQuadraticBezierTangent(1.0f, startPoint, controlPoint, endPoint);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            arrowHead.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void StartTargeting(Transform origin)
    {
        startTransform = origin;
        currentUvOffset = 0f; // Reset offset when starting a drag
        SetTargetingActive(true);
    }

    public void StopTargeting()
    {
        SetTargetingActive(false);
        startTransform = null;
    }

    private void SetTargetingActive(bool active)
    {
        isTargeting = active;
        lineRenderer.enabled = active;
        if (arrowHead != null) arrowHead.gameObject.SetActive(active);
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }

    private Vector3 CalculateQuadraticBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (2 * u * (p1 - p0)) + (2 * t * (p2 - p1));
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mousePos = new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Mathf.Abs(mainCamera.transform.position.z));
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    public void HandleDragEnd()
    {
        UICardEntity targetingEntity = UIEntityPicker.GetHoveredCardFromRaycast();
        onTargetChoosen?.Invoke(targetingEntity, this);
    }
}