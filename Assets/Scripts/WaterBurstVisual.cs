using System.Collections.Generic;
using UnityEngine;

public class WaterBurstVisual : MonoBehaviour
{
    private readonly List<SpriteRenderer> segmentRenderers = new();
    private readonly List<Vector3> startPositions = new();
    private readonly List<Vector3> endPositions = new();
    private readonly List<Vector3> startScales = new();
    private readonly List<Vector3> endScales = new();

    private float lifetime;
    private float elapsed;

    public static void Spawn(Vector2 position, Vector2 size, float facingSign)
    {
        GameObject visualObject = new("WaterBurstVisual");
        visualObject.transform.position = new Vector3(position.x, position.y, -0.5f);

        WaterBurstVisual visual = visualObject.AddComponent<WaterBurstVisual>();
        visual.Initialize(size, facingSign);
    }

    private void Initialize(Vector2 size, float facingSign)
    {
        lifetime = 0.16f;

        float sign = facingSign == 0f ? 1f : Mathf.Sign(facingSign);
        CreateSegment(new Vector3(0.18f * sign, -0.06f, 0f), new Vector3(size.x * 0.2f * sign, size.y * 0.76f, 1f), new Vector3(0.55f * sign, 0f, 0f), new Vector3(size.x * 0.24f * sign, size.y * 0.86f, 1f), new Color(0.12f, 0.72f, 0.92f, 0.5f), 14);
        CreateSegment(new Vector3(0.82f * sign, -0.01f, 0f), new Vector3(size.x * 0.28f * sign, size.y * 0.68f, 1f), new Vector3(1.35f * sign, 0.08f, 0f), new Vector3(size.x * 0.34f * sign, size.y * 0.8f, 1f), new Color(0.18f, 0.86f, 1f, 0.64f), 15);
        CreateSegment(new Vector3(1.6f * sign, 0.08f, 0f), new Vector3(size.x * 0.34f * sign, size.y * 0.52f, 1f), new Vector3(2.18f * sign, 0.14f, 0f), new Vector3(size.x * 0.42f * sign, size.y * 0.6f, 1f), new Color(0.7f, 1f, 1f, 0.8f), 16);
        CreateSegment(new Vector3(2.54f * sign, 0.15f, 0f), new Vector3(size.x * 0.14f * sign, size.y * 0.34f, 1f), new Vector3(3.02f * sign, 0.22f, 0f), new Vector3(size.x * 0.18f * sign, size.y * 0.4f, 1f), new Color(0.92f, 1f, 1f, 0.9f), 17);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / lifetime);

        for (int i = 0; i < segmentRenderers.Count; i++)
        {
            SpriteRenderer renderer = segmentRenderers[i];
            renderer.transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], progress);
            renderer.transform.localScale = Vector3.Lerp(startScales[i], endScales[i], progress);

            Color color = renderer.color;
            color.a = Mathf.Lerp(color.a, 0f, progress);
            renderer.color = color;
        }

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void CreateSegment(Vector3 localStartPosition, Vector3 localStartScale, Vector3 localEndPosition, Vector3 localEndScale, Color color, int sortingOrder)
    {
        GameObject segment = new("Segment");
        segment.transform.SetParent(transform, false);
        segment.transform.localPosition = localStartPosition;
        segment.transform.localScale = localStartScale;

        SpriteRenderer segmentRenderer = segment.AddComponent<SpriteRenderer>();
        segmentRenderer.sprite = PrimitiveSpriteLibrary.SquareSprite;
        segmentRenderer.color = color;
        segmentRenderer.sortingOrder = sortingOrder;

        segmentRenderers.Add(segmentRenderer);
        startPositions.Add(localStartPosition);
        endPositions.Add(localEndPosition);
        startScales.Add(localStartScale);
        endScales.Add(localEndScale);
    }
}
