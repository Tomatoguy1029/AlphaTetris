using UnityEngine;

namespace AlphaTetris {
  public class UIPresenter : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private BoardView _boardView;
    [SerializeField] private HoldView _holdView;
    [SerializeField] private NextQueueView _nextQueueView;
    [SerializeField] private StatsView _statsView;
    [SerializeField] private GameObject _preGamePanel;
    [SerializeField] private PauseView _pauseView;
    [SerializeField] private GameOverView _gameOverView;

    private GameState _lastState = GameState.PreGame;

    private void Awake() {
      if (_gameLogic == null) {
        _gameLogic = FindFirstObjectByType<GameLogic>();
      }

      if (_gameLogic != null) {
        _lastState = _gameLogic.CurrentState;
      }
    }

    private void OnEnable() {
      Subscribe();
      RefreshAll();
    }

    private void OnDisable() {
      Unsubscribe();
    }

    private void Update() {
      if (_gameLogic == null) {
        return;
      }

      var currentState = _gameLogic.CurrentState;
      if (currentState != _lastState) {
        _lastState = currentState;
        ApplyState(currentState);
      }
    }

    private void Subscribe() {
      if (_gameLogic == null) {
        _gameLogic = FindFirstObjectByType<GameLogic>();
        if (_gameLogic != null) {
          _lastState = _gameLogic.CurrentState;
        }
      }

      if (_gameLogic == null) {
        return;
      }

      _gameLogic.OnBoardUpdated += HandleBoardUpdated;
      _gameLogic.OnGameOver += HandleGameOver;
    }

    private void Unsubscribe() {
      if (_gameLogic == null) {
        return;
      }

      _gameLogic.OnBoardUpdated -= HandleBoardUpdated;
      _gameLogic.OnGameOver -= HandleGameOver;
    }

    private void RefreshAll() {
      if (_gameLogic == null) {
        return;
      }

      HandleBoardUpdated();
      ApplyState(_gameLogic.CurrentState);
    }

    private void HandleBoardUpdated() {
      if (_gameLogic == null) {
        return;
      }

      _boardView?.Render(_gameLogic.RenderBoard);
      _holdView?.Render(_gameLogic.HoldMino);
      _nextQueueView?.Render(_gameLogic.NextPreview);
      _statsView?.SetValues(_gameLogic.Score, _gameLogic.Level, _gameLogic.LinesCleared);
    }

    private void HandleGameOver() {
      if (_gameLogic == null) {
        return;
      }

      HandleBoardUpdated();
      _gameOverView?.Show(_gameLogic.Score, _gameLogic.Level, _gameLogic.LinesCleared);
      ApplyState(GameState.GameOver);
    }

    private void ApplyState(GameState state) {
      if (_preGamePanel != null) {
        _preGamePanel.SetActive(state == GameState.PreGame);
      }

      if (_pauseView != null) {
        if (state == GameState.Paused) {
          _pauseView.Show();
        } else {
          _pauseView.Hide();
        }
      }

      if (_gameOverView != null && state != GameState.GameOver) {
        _gameOverView.Hide();
      }
    }

    public void OnRetryButtonPressed() {
      if (_gameLogic == null) {
        return;
      }

      _gameLogic.StartGame();
    }
  }
}
