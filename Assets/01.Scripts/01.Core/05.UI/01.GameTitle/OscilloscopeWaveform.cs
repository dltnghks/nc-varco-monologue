using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class UIHeartbeatGraph : MaskableGraphic
{
    [Header("Heartbeat Settings")]
    public float bpm = 60f;
    public float amplitude = 50f;
    public AnimationCurve beatCurve;
    
    [Header("Graph Settings")]
    public float thickness = 2f;
    [Range(30, 240)] public int graphUpdateRate = 60; // 초당 그래프 업데이트 횟수
    public float resolution = 0.5f;
    [Tooltip("The width (in UI pixels) at each end of the graph where the line will fade in/out.")]
    public float fadeWidth = 50f;

    // 내부 데이터 (List -> Queue로 변경)
    private Queue<float> valueQueue = new Queue<float>();
    private float[] valueArray; // OnPopulateMesh에서 사용할 배열
    private float width;
    private float height;
    
    // 심장 박동 로직용
    private float beatTimer;
    private bool isBeating;
    private float beatProgress;
    private float beatDuration;

    // 시간 누적용
    private float timeAccumulator;
    private float graphUpdateInterval;

    protected override void Awake()
    {
        base.Awake();
        graphUpdateInterval = 1.0f / graphUpdateRate;
        ResetGraph();
    }

    private void ResetGraph()
    {
        if (rectTransform == null) return;
        
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;
        
        int pointCount = Mathf.Max(2, Mathf.FloorToInt(width * resolution));
        valueQueue.Clear();
        for (int i = 0; i < pointCount; i++) valueQueue.Enqueue(0f);

        // valueArray도 크기가 변경될 수 있으므로 다시 생성
        if (valueArray == null || valueArray.Length != pointCount)
        {
            valueArray = new float[pointCount];
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        ResetGraph();
        SetVerticesDirty();
    }

    // Update에서는 시간만 누적하고, 고정된 간격으로 로직을 처리
    void Update()
    {
        timeAccumulator += Time.deltaTime;
        bool needsRedraw = false;

        // 누적된 시간이 업데이트 간격보다 크면, 따라잡을 때까지 로직 반복
        while (timeAccumulator >= graphUpdateInterval)
        {
            // Time.deltaTime 대신 고정된 간격(graphUpdateInterval)을 사용
            HandleHeartbeatLogic(graphUpdateInterval); 
            UpdateGraphData();
            
            timeAccumulator -= graphUpdateInterval;
            needsRedraw = true;
        }

        // 모든 데이터 처리가 끝난 후, UI를 한 번만 갱신
        if (needsRedraw)
        {
            SetVerticesDirty();
        }
    }

    // Time.deltaTime 대신 고정된 시간 간격을 받도록 수정
    void HandleHeartbeatLogic(float fixedDeltaTime)
    {
        beatTimer += fixedDeltaTime;
        float beatInterval = 60f / bpm;

        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval;
            isBeating = true;
            beatProgress = 0f;
            beatDuration = beatInterval * 0.4f;
        }
    }

    // 데이터 처리 로직 (Queue 사용)
    void UpdateGraphData()
    {
        if (valueQueue.Count == 0) return;

        float currentY = 0f;
        if (isBeating)
        {
            // Time.deltaTime 대신 고정된 간격을 사용
            beatProgress += graphUpdateInterval;
            float beatProgressNormalized = beatProgress / beatDuration;

            if (beatCurve != null)
                currentY = beatCurve.Evaluate(beatProgressNormalized) * amplitude;
            
            if (beatProgress >= beatDuration) isBeating = false;
        }
        
        currentY += Random.Range(-1.5f, 1.5f);

        // Queue에서 가장 오래된 데이터 제거 및 새 데이터 추가
        valueQueue.Dequeue();
        valueQueue.Enqueue(currentY);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (valueQueue.Count < 2) return;

        // 그리기 전에 Queue를 배열로 복사 (반복 중 변경 방지)
        valueQueue.CopyTo(valueArray, 0);

        float stepX = width / (valueArray.Length - 1);
        Vector2 pivotOffset = new Vector2(rectTransform.pivot.x * width, rectTransform.pivot.y * height);

        for (int i = 0; i < valueArray.Length - 1; i++)
        {
            float x1 = i * stepX;
            float x2 = (i + 1) * stepX;
            
            Vector2 p1 = new Vector2(x1, valueArray[i]) - pivotOffset;
            Vector2 p2 = new Vector2(x2, valueArray[i+1]) - pivotOffset;

            float alpha1 = CalculateAlphaForPosition(x1);
            float alpha2 = CalculateAlphaForPosition(x2);

            AddLineSegment(vh, p1, p2, thickness, alpha1, alpha2);
        }
    }

    private void AddLineSegment(VertexHelper vh, Vector2 start, Vector2 end, float width, float startAlpha, float endAlpha)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * width * 0.5f;

        UIVertex v = UIVertex.simpleVert;
        Color baseColor = this.color;

        v.position = start - normal; 
        v.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * startAlpha);
        vh.AddVert(v);

        v.position = start + normal; 
        v.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * startAlpha);
        vh.AddVert(v);
        
        v.position = end + normal;   
        v.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * endAlpha);
        vh.AddVert(v);
        
        v.position = end - normal;   
        v.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * endAlpha);
        vh.AddVert(v);

        int idx = vh.currentVertCount;
        vh.AddTriangle(idx - 4, idx - 3, idx - 2);
        vh.AddTriangle(idx - 2, idx - 1, idx - 4);
    }
    
    private float CalculateAlphaForPosition(float xPosition)
    {
        // Clamp fadeWidth to prevent overlap
        float clampedFadeWidth = Mathf.Min(fadeWidth, width / 2.0f);

        // Left side fade-in
        if (xPosition < clampedFadeWidth)
        {
            return Mathf.InverseLerp(0, clampedFadeWidth, xPosition);
        }
        // Right side fade-out
        else if (xPosition > width - clampedFadeWidth)
        {
            return 1.0f - Mathf.InverseLerp(width - clampedFadeWidth, width, xPosition);
        }
        
        // Middle part is fully opaque
        return 1.0f;
    }
    
    public void SetBPM(float newBPM) => bpm = Mathf.Clamp(newBPM, 30, 220);
}