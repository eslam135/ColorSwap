using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CircleSlot : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color _highlightColor = new Color(1f, 0.95f, 0.45f, 1f);
    [SerializeField] private float _normalScale = 1f;
    [SerializeField] private float _highlightScale = 1.12f;
    [SerializeField] private float _highlightTweenDuration = 0.12f;

    public int SlotIndex { get; private set; }

    private RectTransform _rectTransform;
    private Image _image;
    private Coroutine _highlightRoutine;

    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void Initialize(int slotIndex, Vector2 anchoredPosition)
    {
        SlotIndex = slotIndex;
        RectTransform.anchoredPosition = anchoredPosition;
        RectTransform.localScale = Vector3.one * _normalScale;

        if (_image != null)
        {
            _image.color = _normalColor;
        }

        gameObject.name = $"Slot_{slotIndex}";
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_highlightRoutine != null)
        {
            StopCoroutine(_highlightRoutine);
        }

        Color targetColor = highlighted ? _highlightColor : _normalColor;
        float targetScale = highlighted ? _highlightScale : _normalScale;

        _highlightRoutine = StartCoroutine(AnimateHighlight(targetColor, targetScale));
    }

    private IEnumerator AnimateHighlight(Color targetColor, float targetScale)
    {
        Color startColor = _image != null ? _image.color : Color.white;
        Vector3 startScale = RectTransform.localScale;
        Vector3 targetScaleVector = Vector3.one * targetScale;

        float elapsedTime = 0f;

        while (elapsedTime < _highlightTweenDuration)
        {
            float t = elapsedTime / _highlightTweenDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            if (_image != null)
            {
                _image.color = Color.Lerp(startColor, targetColor, t);
            }

            RectTransform.localScale = Vector3.LerpUnclamped(startScale, targetScaleVector, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (_image != null)
        {
            _image.color = targetColor;
        }

        RectTransform.localScale = targetScaleVector;
    }
}