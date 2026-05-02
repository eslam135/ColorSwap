using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private Image _blockerImage;

    [Header("Panel")]
    [SerializeField] private RectTransform _panelCard;
    [SerializeField] private TMP_Text _instructionsText;
    [SerializeField] private Button _closeButton;

    [Header("Animation")]
    [SerializeField] private float _hiddenY = -850f;
    [SerializeField] private float _shownY = 260f;
    [SerializeField] private float _animationDuration = 0.42f;
    [SerializeField] private float _blockerTargetAlpha = 0.55f;

    [Header("Content")]
    [TextArea(4, 8)]
    [SerializeField]
    private string _defaultInstructions =
        "Drag a piece onto a connected piece to swap them.\n\n" +
        "You can only swap pieces connected by a line.\n\n" +
        "To solve the puzzle, every line must connect two different pieces.\n\n" +
        "Finish in fewer moves to earn more stars.";

    private Coroutine _animationRoutine;

    private void Awake()
    {
        if (_panelRoot == null)
        {
            _panelRoot = gameObject;
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Hide);
        }

        HideInstant();
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Hide);
        }
    }

    public void Show()
    {
        if (_panelRoot == null)
        {
            return;
        }

        _panelRoot.SetActive(true);
        _panelRoot.transform.SetAsLastSibling();

        if (_instructionsText != null)
        {
            _instructionsText.text = _defaultInstructions;
        }

        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
        }

        SetPanelHiddenInstant();
        SetBlockerAlphaInstant(0f);

        _animationRoutine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
        }

        _animationRoutine = StartCoroutine(HideRoutine());
    }

    public void HideInstant()
    {
        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
        }

        SetPanelHiddenInstant();
        SetBlockerAlphaInstant(0f);

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private IEnumerator ShowRoutine()
    {
        Vector2 startPosition = _panelCard.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, _shownY);

        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            float t = elapsedTime / _animationDuration;

            float panelT = EaseOutBack(t);
            float fadeT = EaseOutCubic(t);

            if (_panelCard != null)
            {
                _panelCard.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, panelT);
            }

            SetBlockerAlphaInstant(Mathf.Lerp(0f, _blockerTargetAlpha, fadeT));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (_panelCard != null)
        {
            _panelCard.anchoredPosition = targetPosition;
        }

        SetBlockerAlphaInstant(_blockerTargetAlpha);
    }

    private IEnumerator HideRoutine()
    {
        Vector2 startPosition = _panelCard.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, _hiddenY);

        float startAlpha = _blockerImage != null ? _blockerImage.color.a : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration * 0.75f)
        {
            float t = elapsedTime / (_animationDuration * 0.75f);
            t = EaseInCubic(t);

            if (_panelCard != null)
            {
                _panelCard.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
            }

            SetBlockerAlphaInstant(Mathf.Lerp(startAlpha, 0f, t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetPanelHiddenInstant();
        SetBlockerAlphaInstant(0f);

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private void SetPanelHiddenInstant()
    {
        if (_panelCard == null)
        {
            return;
        }

        Vector2 position = _panelCard.anchoredPosition;
        position.y = _hiddenY;
        _panelCard.anchoredPosition = position;
    }

    private void SetBlockerAlphaInstant(float alpha)
    {
        if (_blockerImage == null)
        {
            return;
        }

        Color color = _blockerImage.color;
        color.a = alpha;
        _blockerImage.color = color;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}