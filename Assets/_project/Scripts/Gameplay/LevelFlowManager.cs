using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelFlowManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private List<LevelData> _levels = new List<LevelData>();

    [Header("References")]
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private SuccessPanel _successPanel;

    [Header("Header UI")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private Button _resetButton;

    private int _currentLevelIndex;

    private void Awake()
    {
        if (_boardManager != null)
        {
            _boardManager.OnMovesChanged += HandleMovesChanged;
            _boardManager.OnLevelLoaded += HandleLevelLoaded;
            _boardManager.OnLevelCompleted += HandleLevelCompleted;
        }

        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(ResetCurrentLevel);
        }

        if (_successPanel != null && _successPanel.NextButton != null)
        {
            _successPanel.NextButton.onClick.AddListener(LoadNextLevel);
        }
        if (_successPanel != null && _successPanel.ReplayButton != null)
        {
            _successPanel.ReplayButton.onClick.AddListener(ResetCurrentLevel);
        }
    }

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void OnDestroy()
    {
        if (_boardManager != null)
        {
            _boardManager.OnMovesChanged -= HandleMovesChanged;
            _boardManager.OnLevelLoaded -= HandleLevelLoaded;
            _boardManager.OnLevelCompleted -= HandleLevelCompleted;
        }

        if (_resetButton != null)
        {
            _resetButton.onClick.RemoveListener(ResetCurrentLevel);
        }

        if (_successPanel != null && _successPanel.NextButton != null)
        {
            _successPanel.NextButton.onClick.RemoveListener(LoadNextLevel);
        }

        if (_successPanel != null && _successPanel.ReplayButton != null)
        {
            _successPanel.ReplayButton.onClick.RemoveListener(ResetCurrentLevel);
        }
    }

    private void LoadCurrentLevel()
    {
        if (_levels == null || _levels.Count == 0)
        {
            Debug.LogError("No levels assigned to LevelFlowManager.");
            return;
        }

        if (_boardManager == null)
        {
            Debug.LogError("No BoardManager assigned to LevelFlowManager.");
            return;
        }

        if (_successPanel != null)
        {
            _successPanel.HideInstant();
        }

        _boardManager.LoadLevel(_levels[_currentLevelIndex]);
    }

    public void LoadNextLevel()
    {
        _currentLevelIndex++;

        if (_currentLevelIndex >= _levels.Count)
        {
            _currentLevelIndex = 0;
        }

        LoadCurrentLevel();
    }

    public void ResetCurrentLevel()
    {
        LoadCurrentLevel();
    }

    private void HandleMovesChanged(int moveCount)
    {
        if (_movesText == null || _boardManager == null || _boardManager.CurrentLevel == null)
        {
            return;
        }

        int parMoves = _boardManager.CurrentLevel.parMoves;

        if (parMoves > 0)
        {
            _movesText.text = $"{moveCount}/{parMoves}";
        }
        else
        {
            _movesText.text = moveCount.ToString();
        }
    }
    private void HandleLevelLoaded(LevelData levelData)
    {
        if (_levelText != null && levelData != null)
        {
            _levelText.text = levelData.levelName;
        }
    }

    private void HandleLevelCompleted(int moves, int parMoves)
    {
        if (_successPanel == null)
        {
            return;
        }

        _successPanel.Show(moves, parMoves);
    }
}