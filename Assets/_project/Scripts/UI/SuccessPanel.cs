using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuccessPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private Image _blockerImage;

    [Header("Panel")]
    [SerializeField] private RectTransform _panelCard;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _movesSummaryText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _replayButton;

    [Header("Stars")]
    [SerializeField] private Image _star1;
    [SerializeField] private Image _star2;
    [SerializeField] private Image _star3;

    [Header("Animation")]
    [SerializeField] private float _hiddenY = -800f;
    [SerializeField] private float _shownY = 280f;
    [SerializeField] private float _animationDuration = 0.45f;
    [SerializeField] private float _starPopDuration = 0.12f;
    [SerializeField] private float _delayBetweenStars = 0.08f;

    [Header("Colors")]
    [SerializeField] private Color _dimStarColor = new Color(0.35f, 0.35f, 0.35f, 0.45f);
    [SerializeField] private Color _litStarColor = new Color(1f, 0.82f, 0.24f, 1f);

    public Button NextButton => _nextButton;
    public Button ReplayButton => _replayButton;

    private Coroutine _showRoutine;

    private void Awake()
    {
        if (_panelRoot == null)
        {
            _panelRoot = gameObject;
        }

        HideInstant();

    }

    public void Show(int moves, int parMoves)
    {
        if (_panelRoot == null)
        {
            return;
        }

        _panelRoot.SetActive(true);

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        int starCount = CalculateStarCount(moves, parMoves);

        if (_titleText != null)
        {
            _titleText.text = "LEVEL COMPLETE";
        }

        if (_movesSummaryText != null)
        {
            _movesSummaryText.text = parMoves > 0
                ? $"{moves}/{parMoves} MOVES"
                : $"{moves} MOVES";
        }

        SetAllStarsDimInstant();
        SetPanelHiddenInstant();

        if (_blockerImage != null)
        {
            Color color = _blockerImage.color;
            color.a = 0f;
            _blockerImage.color = color;
        }

        _showRoutine = StartCoroutine(ShowRoutine(starCount));
    }

    public void HideInstant()
    {
        if (_panelCard != null)
        {
            SetPanelHiddenInstant();
        }

        if (_blockerImage != null)
        {
            Color color = _blockerImage.color;
            color.a = 0f;
            _blockerImage.color = color;
        }

        SetAllStarsDimInstant();

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private IEnumerator ShowRoutine(int starCount)
    {
        Vector2 startPosition = _panelCard.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, _shownY);

        float targetBlockerAlpha = 0.45f;
        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            float t = elapsedTime / _animationDuration;
            t = EaseOutCubic(t);

            if (_panelCard != null)
            {
                _panelCard.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
            }

            if (_blockerImage != null)
            {
                Color color = _blockerImage.color;
                color.a = Mathf.Lerp(0f, targetBlockerAlpha, t);
                _blockerImage.color = color;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (_panelCard != null)
        {
            _panelCard.anchoredPosition = targetPosition;
        }

        if (_blockerImage != null)
        {
            Color color = _blockerImage.color;
            color.a = targetBlockerAlpha;
            _blockerImage.color = color;
        }

        yield return new WaitForSeconds(0.05f);

        if (starCount >= 1)
        {
            yield return LightStar(_star1);
            yield return new WaitForSeconds(_delayBetweenStars);
        }

        if (starCount >= 2)
        {
            yield return LightStar(_star2);
            yield return new WaitForSeconds(_delayBetweenStars);
        }

        if (starCount >= 3)
        {
            yield return LightStar(_star3);
        }
    }

    private IEnumerator LightStar(Image starImage)
    {
        if (starImage == null)
        {
            yield break;
        }

        RectTransform starRect = starImage.rectTransform;

        Vector3 originalScale = Vector3.one;
        Vector3 popScale = Vector3.one * 1.28f;

        starImage.color = _litStarColor;
        starRect.localScale = originalScale;

        float elapsedTime = 0f;

        while (elapsedTime < _starPopDuration)
        {
            float t = elapsedTime / _starPopDuration;
            t = EaseOutBack(t);

            starRect.localScale = Vector3.LerpUnclamped(originalScale, popScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        starRect.localScale = popScale;

        elapsedTime = 0f;

        while (elapsedTime < _starPopDuration)
        {
            float t = elapsedTime / _starPopDuration;
            t = EaseOutCubic(t);

            starRect.localScale = Vector3.LerpUnclamped(popScale, originalScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        starRect.localScale = originalScale;
        starImage.color = _litStarColor;
    }

    private void SetAllStarsDimInstant()
    {
        SetStarDim(_star1);
        SetStarDim(_star2);
        SetStarDim(_star3);
    }

    private void SetStarDim(Image starImage)
    {
        if (starImage == null)
        {
            return;
        }

        starImage.color = _dimStarColor;
        starImage.rectTransform.localScale = Vector3.one;
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

    private int CalculateStarCount(int moves, int parMoves)
    {
        if (parMoves <= 0)
        {
            return 3;
        }

        if (moves <= parMoves)
        {
            return 3;
        }

        if (moves == parMoves + 1)
        {
            return 2;
        }

        return 1;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}