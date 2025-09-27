using UnityEngine;

namespace AlphaTetris {
  public class UIPresenter : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private BoardView _boardView;
    [SerializeField] private ScoreView _scoreView;
    [SerializeField] private LevelView _levelView;
    [SerializeField] private HoldView _holdView;
    [SerializeField] private NextQueueView _nextQueueView;
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _gameOverPanel;

    private void OnEnable() {
      if (_gameLogic == null) {
        Debug.LogWarning("UIPresenter: GameLogic reference is missing.");
        return;
      }

      _gameLogic.OnBoardUpdated += HandleBoardUpdated;
      _gameLogic.OnStateChanged += HandleStateChanged;
      _gameLogic.OnGameOver += HandleGameOver;

      HandleBoardUpdated();
      HandleStateChanged(_gameLogic.CurrentState);
    }

    private void OnDisable() {
      if (_gameLogic == null) {
        return;
      }

      _gameLogic.OnBoardUpdated -= HandleBoardUpdated;
      _gameLogic.OnStateChanged -= HandleStateChanged;
      _gameLogic.OnGameOver -= HandleGameOver;
    }

    private void HandleBoardUpdated() {
      if (_gameLogic == null) {
        return;
      }

      _boardView?.Draw(_gameLogic.RenderBoard);
      _scoreView?.UpdateText(_gameLogic.Score);
      _levelView?.UpdateText(_gameLogic.Level);

      // TODO: Hook up hold/next data when GameLogic exposes it.
      _holdView?.DrawMino(Tetrimino.GetRandom());
      _nextQueueView?.DrawMino(Tetrimino.GetRandom());
    }

    private void HandleStateChanged(GameLogic.GameState state) {
      switch (state) {
        case GameLogic.GameState.PreGame:
          TogglePanel(_startPanel, true);
          TogglePanel(_gameOverPanel, false);
          break;
        case GameLogic.GameState.Playing:
          TogglePanel(_startPanel, false);
          TogglePanel(_gameOverPanel, false);
          break;
        case GameLogic.GameState.GameOver:
          TogglePanel(_startPanel, false);
          TogglePanel(_gameOverPanel, true);
          break;
      }
    }

    private void HandleGameOver() {
      TogglePanel(_gameOverPanel, true);
    }

    public void OnClickStart() {
      _gameLogic?.StartGame();
    }

    public void OnClickRetry() {
      _gameLogic?.StartGame();
    }

    private static void TogglePanel(GameObject panel, bool isActive) {
      if (panel == null) {
        return;
      }

      if (panel.activeSelf != isActive) {
        panel.SetActive(isActive);
      }
    }
  }
}
