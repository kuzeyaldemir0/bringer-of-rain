using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossController))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossSpriteAnimator : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> CachedAnimations = new();

    private const string ResourceFolder = "Enemies/NinjaFrog";
    private const int CellSize = 32;
    private const float PixelsPerUnit = 24f;

    private BossController boss;
    private SpriteRenderer spriteRenderer;
    private Sprite[] activeFrames;
    private string currentClip;
    private float nextFrameAt;
    private float frameDuration;
    private int frameIndex;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetClip("Idle", true);
    }

    private void Update()
    {
        string nextClip = ResolveClip();
        if (nextClip != currentClip)
        {
            SetClip(nextClip, true);
        }

        if (activeFrames == null || activeFrames.Length == 0)
        {
            return;
        }

        if (Time.time >= nextFrameAt)
        {
            frameIndex = (frameIndex + 1) % activeFrames.Length;
            spriteRenderer.sprite = activeFrames[frameIndex];
            nextFrameAt = Time.time + frameDuration;
        }
    }

    private string ResolveClip()
    {
        if (boss.IsDefeated)
        {
            return "Hit";
        }

        if (boss.IsExposed)
        {
            return "Hit";
        }

        if (boss.IsSlamming || boss.IsDashing)
        {
            return "Run";
        }

        return "Idle";
    }

    private void SetClip(string clipName, bool forceRestart)
    {
        if (!forceRestart && clipName == currentClip)
        {
            return;
        }

        currentClip = clipName;
        activeFrames = LoadFrames(clipName);
        frameDuration = clipName switch
        {
            "Hit" => 0.07f,
            "Run" => boss.IsDashing ? 0.05f : 0.07f,
            _ => boss.IsTelegraphing ? 0.1f : 0.16f
        };

        frameIndex = 0;
        if (activeFrames.Length > 0)
        {
            spriteRenderer.sprite = activeFrames[0];
        }
        nextFrameAt = Time.time + frameDuration;
    }

    private Sprite[] LoadFrames(string clipName)
    {
        string cacheKey = $"{ResourceFolder}/{clipName}";
        if (CachedAnimations.TryGetValue(cacheKey, out Sprite[] cached))
        {
            return cached;
        }

        Texture2D texture = Resources.Load<Texture2D>(cacheKey);
        if (texture == null)
        {
            Debug.LogWarning($"Boss animation strip not found at Resources/{cacheKey}.");
            Sprite[] fallback = { PrimitiveSpriteLibrary.SquareSprite };
            CachedAnimations[cacheKey] = fallback;
            return fallback;
        }

        texture.filterMode = FilterMode.Point;
        int frameCount = Mathf.Max(1, texture.width / CellSize);
        Sprite[] frames = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            Rect rect = new(i * CellSize, 0f, CellSize, CellSize);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.name = $"Boss_{clipName}_{i}";
            frames[i] = sprite;
        }

        CachedAnimations[cacheKey] = frames;
        return frames;
    }
}
