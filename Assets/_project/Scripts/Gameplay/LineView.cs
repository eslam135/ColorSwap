using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LineView : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private float _lineThickness = 9f;

    [SerializeField] private Color _normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private Color _correctColor = new Color(0.25f, 0.75f, 0.45f, 1f);

    [SerializeField] private float _normalThicknessMultiplier = 1f;
    [SerializeField] private float _correctThicknessMultiplier = 1.25f;

    [SerializeField] private float _feedbackTweenDuration = 0.15f;

    public int NodeA { get; private set; }
    public int NodeB { get; private set; }

    private RectTransform _rectTransform;
    private Image _image;

    private float _baseDistance;
    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    public void Initialize(int nodeA, int nodeB, Vector2 startPosition, Vector2 endPosition)
    {
        NodeA = nodeA;
        NodeB = nodeB;

        gameObject.name = $"Line_{nodeA}_{nodeB}";

        Vector2 direction = endPosition - startPosition;
        _baseDistance = direction.magnitude;

        _rectTransform.anchoredPosition = startPosition + direction * 0.5f;
        _rectTransform.sizeDelta = new Vector2(_baseDistance, _lineThickness);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

        SetCorrectInstant(false);
    }

    public void SetCorrect(bool isCorrect)
    {
        if (_feedbackRoutine != null)
        {
            StopCoroutine(_feedbackRoutine);
        }

        Color targetColor = isCorrect ? _correctColor : _normalColor;
        float targetThickness = _lineThickness * (isCorrect ? _correctThicknessMultiplier : _normalThicknessMultiplier);

        _feedbackRoutine = StartCoroutine(AnimateFeedback(targetColor, targetThickness));
    }

    public void SetCorrectInstant(bool isCorrect)
    {
        Color targetColor = isCorrect ? _correctColor : _normalColor;
        float targetThickness = _lineThickness * (isCorrect ? _correctThicknessMultiplier : _normalThicknessMultiplier);

        if (_image != null)
        {
            _image.color = targetColor;
        }

        _rectTransform.sizeDelta = new Vector2(_baseDistance, targetThickness);
    }

    private IEnumerator AnimateFeedback(Color targetColor, float targetThickness)
    {
        Color startColor = _image != null ? _image.color : Color.white;
        Vector2 startSize = _rectTransform.sizeDelta;
        Vector2 targetSize = new Vector2(_baseDistance, targetThickness);

        float elapsedTime = 0f;

        while (elapsedTime < _feedbackTweenDuration)
        {
            float t = elapsedTime / _feedbackTweenDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            if (_image != null)
            {
                _image.color = Color.Lerp(startColor, targetColor, t);
            }

            _rectTransform.sizeDelta = Vector2.LerpUnclamped(startSize, targetSize, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (_image != null)
        {
            _image.color = targetColor;
        }

        _rectTransform.sizeDelta = targetSize;
    }
}