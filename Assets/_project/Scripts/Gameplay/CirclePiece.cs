using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CirclePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [System.Serializable]
    private struct PieceSpriteEntry
    {
        public CircleColor color;
        public Sprite sprite;
    }

    [Header("Visual References")]
    [SerializeField] private Image _fillImage;

    [Header("Piece Sprites")]
    [SerializeField] private PieceSpriteEntry[] _pieceSprites;

    public int CurrentSlotIndex { get; private set; }
    public CircleColor CircleColor { get; private set; }

    private BoardManager _boardManager;
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Vector3 _originalScale;
    private bool _isDraggingThisPiece;
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

    public void Initialize(BoardManager boardManager, int slotIndex, CircleColor circleColor, Vector2 anchoredPosition)
    {
        _boardManager = boardManager;
        CurrentSlotIndex = slotIndex;
        CircleColor = circleColor;

        gameObject.name = $"Piece_{slotIndex}_{circleColor}";

        RectTransform.anchoredPosition = anchoredPosition;

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        _originalScale = Vector3.one;
        RectTransform.localScale = _originalScale;

        ApplyVisual(circleColor);
    }

    public void SetSlotIndex(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
        gameObject.name = $"Piece_{slotIndex}_{CircleColor}";
    }

    public void SetAnchoredPosition(Vector2 anchoredPosition)
    {
        RectTransform.anchoredPosition = anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDraggingThisPiece = false;

        if (_boardManager == null || !_boardManager.CanInteract)
        {
            return;
        }

        if (!_boardManager.TryBeginDrag(this))
        {
            return;
        }

        _isDraggingThisPiece = true;

        transform.SetAsLastSibling();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDragStart();
        }

        _boardManager.HighlightConnectedSlots(CurrentSlotIndex);

        RectTransform.localScale = Vector3.one * 1.12f;

        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggingThisPiece)
        {
            return;
        }

        if (_boardManager == null || !_boardManager.CanInteract)
        {
            return;
        }

        if (!_boardManager.IsActiveDraggedPiece(this))
        {
            return;
        }

        float scaleFactor = _canvas != null ? _canvas.scaleFactor : 1f;
        RectTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_boardManager == null)
        {
            return;
        }

        if (!_isDraggingThisPiece)
        {
            return;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = true;
        }

        RectTransform.localScale = _originalScale;

        _boardManager.ClearSlotHighlights();
        _boardManager.EndDrag(this);

        _isDraggingThisPiece = false;

        _boardManager.HandlePieceReleased(this);
    }

    private void ApplyVisual(CircleColor circleColor)
    {
        if (_fillImage == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing Fill Image reference.");
            return;
        }

        Sprite selectedSprite = GetSpriteForColor(circleColor);

        if (selectedSprite == null)
        {
            Debug.LogWarning($"{gameObject.name} has no sprite assigned for {circleColor}.");
            return;
        }

        _fillImage.sprite = selectedSprite;
        _fillImage.color = Color.white;
        _fillImage.preserveAspect = true;
    }

    private Sprite GetSpriteForColor(CircleColor circleColor)
    {
        if (_pieceSprites == null)
        {
            return null;
        }

        for (int i = 0; i < _pieceSprites.Length; i++)
        {
            if (_pieceSprites[i].color == circleColor)
            {
                return _pieceSprites[i].sprite;
            }
        }

        return null;
    }
}