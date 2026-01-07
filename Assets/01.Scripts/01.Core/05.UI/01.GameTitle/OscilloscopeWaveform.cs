using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // For Queue.ToArray()

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
    [Range(0.01f, 1f)] public float resolution = 0.5f; // 점의 밀도 (높을수록 점이 많음)
    [Tooltip("The width (in UI pixels) at each end of the graph where the line will fade in/out.")]
    public float fadeWidth = 50f;

    [Header("Variability")]
    [Range(0, 0.5f)] public float bpmVariability = 0.05f; // BPM의 무작위 변화 비율 (예: 0.05f = ±5%)
    [Range(0, 0.5f)] public float amplitudeVariability = 0.1f; // Amplitude의 무작위 변화 비율 (예: 0.1f = ±10%)


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

    // 변동성 적용된 값
    private float currentBeatAmplitude;
    private float currentBeatInterval; // 다음 박동까지의 시간

    // 시간 누적용
    private float timeAccumulator;
    private float graphUpdateInterval;

    protected override void Awake()
    {
        base.Awake();
        graphUpdateInterval = 1.0f / graphUpdateRate;
        ResetGraph();
        
        // 초기 박동 간격 및 진폭 설정
        CalculateNextBeatParameters();
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
        SetVerticesDirty(); // 크기 변경 시 그래프도 다시 그리도록 요청
    }

    // Update에서는 시간만 누적하고, 고정된 간격으로 로직을 처리
    void Update()
    {
        timeAccumulator += Time.deltaTime;
        bool needsRedraw = false;

        // 누적된 시간이 업데이트 간격보다 크면, 따라잡을 때까지 로직 반복
        while (timeAccumulator >= graphUpdateInterval)
        {
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

    // 다음 박동의 간격과 진폭을 계산 (변동성 적용)
    private void CalculateNextBeatParameters()
    {
        // BPM 변동성 적용
        float randomBpmFactor = Random.Range(-bpmVariability, bpmVariability);
        float variedBpm = bpm * (1.0f + randomBpmFactor) * (1.0f + GameManager.Instance.CurrentDangerLevelRate);
        currentBeatInterval = 60f / variedBpm;

        // Amplitude 변동성 적용
        float randomAmplitudeFactor = Random.Range(-amplitudeVariability, amplitudeVariability);
        currentBeatAmplitude = amplitude * (1.0f + randomAmplitudeFactor) * (1.0f + GameManager.Instance.CurrentDangerLevelRate);
    }

    // Time.deltaTime 대신 고정된 시간 간격을 받도록 수정
    void HandleHeartbeatLogic(float fixedDeltaTime)
    {
        beatTimer += fixedDeltaTime;

        if (beatTimer >= currentBeatInterval)
        {
            beatTimer -= currentBeatInterval; // 초과된 시간은 다음 박동 계산에 반영
            isBeating = true;
            beatProgress = 0f;
            beatDuration = currentBeatInterval * 0.4f; // 변동성 적용된 박동 간격에 따라 지속 시간 설정
            
            // 다음 박동을 위해 새로운 변동성 값 계산
            CalculateNextBeatParameters();
        }
    }

    // 데이터 처리 로직 (Queue 사용)
    void UpdateGraphData()
    {
        if (valueQueue.Count == 0) return;

        float currentY = 0f;
        if (isBeating)
        {
            beatProgress += graphUpdateInterval;
            float beatProgressNormalized = beatProgress / beatDuration;

            if (beatCurve != null)
                currentY = beatCurve.Evaluate(beatProgressNormalized) * currentBeatAmplitude;
            
            if (beatProgress >= beatDuration) isBeating = false;
        }
        
        currentY += Random.Range(-1.5f, 1.5f); // 노이즈 (감소시킴)

        // Queue에서 가장 오래된 데이터 제거 및 새 데이터 추가
        valueQueue.Dequeue();
        valueQueue.Enqueue(currentY);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (valueQueue.Count < 2) return;

        // 그리기 전에 Queue를 배열로 복사 (반복 중 변경 방지)
        // ToArray()는 매번 새 배열을 생성하므로 성능에 영향을 줄 수 있음.
        // 더 최적화하려면 valueArray를 미리 할당하고 수동으로 복사하는 방법을 고려。
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
        float clampedFadeWidth = Mathf.Min(fadeWidth, width / 2.0f);

        if (xPosition < clampedFadeWidth)
        {
            return Mathf.InverseLerp(0, clampedFadeWidth, xPosition);
        }
        else if (xPosition > width - clampedFadeWidth)
        {
            return 1.0f - Mathf.InverseLerp(width - clampedFadeWidth, width, xPosition);
        }
        
        return 1.0f;
    }
    
    public void SetBPM(float newBPM) => bpm = Mathf.Clamp(newBPM, 30, 220);
}