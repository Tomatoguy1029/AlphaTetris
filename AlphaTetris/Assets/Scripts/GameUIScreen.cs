using UnityEngine;
using TMPro;

namespace AlphaTetris {
  public class GameUIScreen : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _hudScoreText;
    [SerializeField] private TextMeshProUGUI _hudLevelText;
    [SerializeField] private TextMeshProUGUI _hudLinesText;
    [SerializeField] private TextMeshProUGUI _gameOverScoreText;
    [SerializeField] private TextMeshProUGUI _gameOverLevelText;
    [SerializeField] private TextMeshProUGUI _gameOverLinesText;

    private void OnEnable() {
      if (_gameLogic == null) {
        Debug.LogWarning("GameUIScreen: GameLogic reference is missing.");
        return;
      }

      _gameLogic.OnStateChanged += HandleStateChanged;
      _gameLogic.OnGameOver += HandleGameOver;
      _gameLogic.OnBoardUpdated += HandleBoardUpdated;

      HandleStateChanged(_gameLogic.CurrentState);
      UpdateHudTexts();
    }

    private void OnDisable() {
      if (_gameLogic == null) {
        return;
      }

      _gameLogic.OnStateChanged -= HandleStateChanged;
      _gameLogic.OnGameOver -= HandleGameOver;
      _gameLogic.OnBoardUpdated -= HandleBoardUpdated;
    }

    public void OnStartButtonPressed() {
      StartGameFromUI();
    }

    public void OnRetryButtonPressed() {
      StartGameFromUI();
    }

    private void StartGameFromUI() {
      if (_gameLogic == null) {
        return;
      }

      _gameLogic.StartGame();
    }

    private void HandleStateChanged(GameLogic.GameState state) {
      if (_startPanel != null) {
        _startPanel.SetActive(state == GameLogic.GameState.PreGame);
      }

      if (_gameOverPanel != null) {
        _gameOverPanel.SetActive(state == GameLogic.GameState.GameOver);
      }

      if (state != GameLogic.GameState.GameOver) {
        UpdateGameOverTexts();
      }
    }

    private void HandleGameOver() {
      UpdateGameOverTexts();

      if (_gameOverPanel != null && !_gameOverPanel.activeSelf) {
        _gameOverPanel.SetActive(true);
      }
    }

    private void HandleBoardUpdated() {
      UpdateHudTexts();
    }

    private void UpdateHudTexts() {
      if (_gameLogic == null) {
        return;
      }

      if (_hudScoreText != null) {
        _hudScoreText.text = $"Score: {_gameLogic.Score}";
      }

      if (_hudLevelText != null) {
        _hudLevelText.text = $"Level: {_gameLogic.Level}";
      }

      if (_hudLinesText != null) {
        _hudLinesText.text = $"Lines: {_gameLogic.LinesCleared}";
      }
    }

    private void UpdateGameOverTexts() {
      if (_gameLogic == null) {
        return;
      }

      if (_gameOverScoreText != null) {
        _gameOverScoreText.text = $"Score: {_gameLogic.Score}";
      }

      if (_gameOverLevelText != null) {
        _gameOverLevelText.text = $"Level: {_gameLogic.Level}";
      }

      if (_gameOverLinesText != null) {
        _gameOverLinesText.text = $"Lines: {_gameLogic.LinesCleared}";
      }
    }
  }
}
