using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private RectTransform _linesRoot;
    [SerializeField] private RectTransform _slotsRoot;
    [SerializeField] private RectTransform _piecesRoot;

    [Header("Prefabs")]
    [SerializeField] private LineView _linePrefab;
    [SerializeField] private CircleSlot _slotPrefab;
    [SerializeField] private CirclePiece _piecePrefab;

    [Header("Drag Settings")]
    [SerializeField] private float _dropDetectionRadius = 90f;
    [SerializeField] private float _moveTweenDuration = 0.18f;

    private readonly List<LineView> _lines = new List<LineView>();
    private readonly List<CircleSlot> _slots = new List<CircleSlot>();
    private readonly List<CirclePiece> _pieces = new List<CirclePiece>();
    private readonly List<EdgeData> _edges = new List<EdgeData>();

    private LevelData _currentLevel;
    private GameState _gameState = GameState.Loading;
    private int _moveCount;
    private CirclePiece _activeDraggedPiece;

    public bool CanInteract => _gameState == GameState.Playing;
    public int MoveCount => _moveCount;
    public LevelData CurrentLevel => _currentLevel;

    public event Action<int> OnMovesChanged;
    public event Action<LevelData> OnLevelLoaded;
    public event Action<int, int> OnLevelCompleted;

    public void LoadLevel(LevelData levelData)
    {
        _currentLevel = levelData;
        _moveCount = 0;
        _activeDraggedPiece = null;

        _gameState = GameState.Loading;

        ClearBoard();

        if (_currentLevel == null)
        {
            Debug.LogError("BoardManager tried to load a null level.");
            return;
        }

        SpawnSlots();
        SpawnLines();
        SpawnPieces();
        ClearSlotHighlights();
        UpdateLineFeedbackInstant();

        OnMovesChanged?.Invoke(_moveCount);
        OnLevelLoaded?.Invoke(_currentLevel);

        _gameState = GameState.Playing;
    }

    public void HandlePieceReleased(CirclePiece releasedPiece)
    {
        if (releasedPiece == null || !CanInteract)
        {
            return;
        }

        CircleSlot targetSlot = GetClosestSlotToPiece(releasedPiece);

        if (targetSlot == null)
        {
            StartCoroutine(TweenPieceBack(releasedPiece));
            return;
        }

        if (targetSlot.SlotIndex == releasedPiece.CurrentSlotIndex)
        {
            StartCoroutine(TweenPieceBack(releasedPiece));
            return;
        }

        if (!AreSlotsConnected(releasedPiece.CurrentSlotIndex, targetSlot.SlotIndex))
        {
            StartCoroutine(TweenPieceBack(releasedPiece));
            return;
        }

        CirclePiece targetPiece = GetPieceAtSlot(targetSlot.SlotIndex);

        if (targetPiece == null)
        {
            StartCoroutine(TweenPieceBack(releasedPiece));
            return;
        }

        StartCoroutine(SwapPiecesRoutine(releasedPiece, targetPiece));
    }

    private IEnumerator TweenPieceBack(CirclePiece piece)
    {
        _gameState = GameState.Animating;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayInvalidMove();
        }

        yield return ShakePiece(piece);

        Vector2 targetPosition = GetSlotPosition(piece.CurrentSlotIndex);
        yield return TweenPieceToPosition(piece, targetPosition, _moveTweenDuration);

        _gameState = GameState.Playing;
    }
    private IEnumerator SwapPiecesRoutine(CirclePiece draggedPiece, CirclePiece targetPiece)
    {
        _gameState = GameState.Animating;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwap();
        }
        int draggedOldSlotIndex = draggedPiece.CurrentSlotIndex;
        int targetOldSlotIndex = targetPiece.CurrentSlotIndex;

        Vector2 draggedTargetPosition = GetSlotPosition(targetOldSlotIndex);
        Vector2 targetTargetPosition = GetSlotPosition(draggedOldSlotIndex);

        draggedPiece.SetSlotIndex(targetOldSlotIndex);
        targetPiece.SetSlotIndex(draggedOldSlotIndex);

        Coroutine draggedTween = StartCoroutine(TweenPieceToPosition(draggedPiece, draggedTargetPosition, _moveTweenDuration));
        Coroutine targetTween = StartCoroutine(TweenPieceToPosition(targetPiece, targetTargetPosition, _moveTweenDuration));

        yield return draggedTween;
        yield return targetTween;

        Coroutine draggedPop = StartCoroutine(PopPiece(draggedPiece));
        Coroutine targetPop = StartCoroutine(PopPiece(targetPiece));

        yield return draggedPop;
        yield return targetPop;

        _moveCount++;

        OnMovesChanged?.Invoke(_moveCount);
        UpdateLineFeedback();

        if (IsSolved())
        {
            _gameState = GameState.Completed;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelComplete();
            }

            OnLevelCompleted?.Invoke(_moveCount, _currentLevel != null ? _currentLevel.parMoves : 0);
        }
        else
        {
            _gameState = GameState.Playing;
        }
    }

    private bool IsSolved()
    {
        for (int i = 0; i < _edges.Count; i++)
        {
            EdgeData edge = _edges[i];

            CirclePiece pieceA = GetPieceAtSlot(edge.nodeA);
            CirclePiece pieceB = GetPieceAtSlot(edge.nodeB);

            if (pieceA == null || pieceB == null)
            {
                return false;
            }

            if (pieceA.CircleColor == pieceB.CircleColor)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator TweenPieceToPosition(CirclePiece piece, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = piece.RectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = EaseOutBack(t);

            piece.SetAnchoredPosition(Vector2.LerpUnclamped(startPosition, targetPosition, t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        piece.SetAnchoredPosition(targetPosition);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private CircleSlot GetClosestSlotToPiece(CirclePiece piece)
    {
        CircleSlot closestSlot = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < _slots.Count; i++)
        {
            CircleSlot slot = _slots[i];

            float distance = Vector2.Distance(
                piece.RectTransform.anchoredPosition,
                slot.RectTransform.anchoredPosition
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }

        if (closestDistance <= _dropDetectionRadius)
        {
            return closestSlot;
        }

        return null;
    }

    private bool AreSlotsConnected(int slotA, int slotB)
    {
        for (int i = 0; i < _edges.Count; i++)
        {
            EdgeData edge = _edges[i];

            bool directMatch = edge.nodeA == slotA && edge.nodeB == slotB;
            bool reverseMatch = edge.nodeA == slotB && edge.nodeB == slotA;

            if (directMatch || reverseMatch)
            {
                return true;
            }
        }

        return false;
    }

    private CirclePiece GetPieceAtSlot(int slotIndex)
    {
        for (int i = 0; i < _pieces.Count; i++)
        {
            if (_pieces[i].CurrentSlotIndex == slotIndex)
            {
                return _pieces[i];
            }
        }

        return null;
    }

    private Vector2 GetSlotPosition(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogWarning($"Tried to get invalid slot position: {slotIndex}");
            return Vector2.zero;
        }

        return _slots[slotIndex].RectTransform.anchoredPosition;
    }

    private void ClearBoard()
    {
        ClearRoot(_linesRoot);
        ClearRoot(_slotsRoot);
        ClearRoot(_piecesRoot);

        _slots.Clear();
        _pieces.Clear();
        _edges.Clear();
        _lines.Clear();
    }

    private void ClearRoot(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private void SpawnSlots()
    {
        for (int i = 0; i < _currentLevel.nodes.Count; i++)
        {
            NodeData nodeData = _currentLevel.nodes[i];

            CircleSlot slot = Instantiate(_slotPrefab, _slotsRoot);
            slot.Initialize(i, nodeData.anchoredPosition);

            _slots.Add(slot);
        }
    }

    private void SpawnLines()
    {
        for (int i = 0; i < _currentLevel.edges.Count; i++)
        {
            EdgeData edge = _currentLevel.edges[i];

            if (!IsValidSlotIndex(edge.nodeA) || !IsValidSlotIndex(edge.nodeB))
            {
                Debug.LogWarning($"Invalid edge found in {_currentLevel.levelName}: {edge.nodeA} - {edge.nodeB}");
                continue;
            }

            CircleSlot slotA = _slots[edge.nodeA];
            CircleSlot slotB = _slots[edge.nodeB];

            LineView line = Instantiate(_linePrefab, _linesRoot);
            line.Initialize(
                edge.nodeA,
                edge.nodeB,
                slotA.RectTransform.anchoredPosition,
                slotB.RectTransform.anchoredPosition
            );

            _edges.Add(edge);
            _lines.Add(line);

        }
    }

    private void SpawnPieces()
    {
        for (int i = 0; i < _currentLevel.nodes.Count; i++)
        {
            NodeData nodeData = _currentLevel.nodes[i];

            CirclePiece piece = Instantiate(_piecePrefab, _piecesRoot);
            piece.Initialize(this, i, nodeData.startingColor, nodeData.anchoredPosition);

            _pieces.Add(piece);
        }
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < _slots.Count;
    }

    public void HighlightConnectedSlots(int sourceSlotIndex)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            bool shouldHighlight = AreSlotsConnected(sourceSlotIndex, _slots[i].SlotIndex);
            _slots[i].SetHighlighted(shouldHighlight);
        }
    }

    public void ClearSlotHighlights()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetHighlighted(false);
        }
    }

    private IEnumerator ShakePiece(CirclePiece piece)
    {
        Vector2 originalPosition = piece.RectTransform.anchoredPosition;

        float duration = 0.16f;
        float strength = 18f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float damping = 1f - progress;

            float offsetX = Mathf.Sin(progress * Mathf.PI * 6f) * strength * damping;

            piece.SetAnchoredPosition(originalPosition + new Vector2(offsetX, 0f));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        piece.SetAnchoredPosition(originalPosition);
    }

    private IEnumerator PopPiece(CirclePiece piece)
    {
        RectTransform rectTransform = piece.RectTransform;

        Vector3 originalScale = Vector3.one;
        Vector3 popScale = Vector3.one * 1.12f;

        float upDuration = 0.08f;
        float downDuration = 0.08f;

        float elapsedTime = 0f;

        while (elapsedTime < upDuration)
        {
            float t = elapsedTime / upDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.localScale = Vector3.LerpUnclamped(originalScale, popScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = popScale;

        elapsedTime = 0f;

        while (elapsedTime < downDuration)
        {
            float t = elapsedTime / downDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.localScale = Vector3.LerpUnclamped(popScale, originalScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    private void UpdateLineFeedback()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            LineView line = _lines[i];

            bool isCorrect = IsEdgeCorrect(line.NodeA, line.NodeB);
            line.SetCorrect(isCorrect);
        }
    }

    private void UpdateLineFeedbackInstant()
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            LineView line = _lines[i];

            bool isCorrect = IsEdgeCorrect(line.NodeA, line.NodeB);
            line.SetCorrectInstant(isCorrect);
        }
    }

    private bool IsEdgeCorrect(int nodeA, int nodeB)
    {
        CirclePiece pieceA = GetPieceAtSlot(nodeA);
        CirclePiece pieceB = GetPieceAtSlot(nodeB);

        if (pieceA == null || pieceB == null)
        {
            return false;
        }

        return pieceA.CircleColor != pieceB.CircleColor;
    }
    public bool TryBeginDrag(CirclePiece piece)
    {
        if (piece == null || !CanInteract)
        {
            return false;
        }

        if (_activeDraggedPiece != null && _activeDraggedPiece != piece)
        {
            return false;
        }

        _activeDraggedPiece = piece;
        return true;
    }

    public bool IsActiveDraggedPiece(CirclePiece piece)
    {
        return _activeDraggedPiece == piece;
    }

    public void EndDrag(CirclePiece piece)
    {
        if (_activeDraggedPiece == piece)
        {
            _activeDraggedPiece = null;
        }
    }
}