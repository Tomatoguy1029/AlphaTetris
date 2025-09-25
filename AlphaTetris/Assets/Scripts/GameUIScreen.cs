using UnityEngine;
using TMPro;

namespace AlphaTetris {
  public class GameUIScreen : MonoBehaviour {
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI hudScoreText;
    [SerializeField] private TextMeshProUGUI hudLevelText;
    [SerializeField] private TextMeshProUGUI hudLinesText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverLevelText;
    [SerializeField] private TextMeshProUGUI gameOverLinesText;

    private void OnEnable() {
      if (gameLogic == null) {
        Debug.LogWarning("GameUIScreen: GameLogic reference is missing.");
        return;
      }

      // ゲームロジックの状態変化に合わせてUIを切り替える
      gameLogic.OnStateChanged += OnState;
      gameLogic.OnGameOver += OnGameOver;
      gameLogic.OnBoardUpdated += OnBoard;

      OnState(gameLogic.CurrentState);
      RefreshHud();
    }

    private void OnDisable() {
      if (gameLogic == null) {
        return;
      }

      gameLogic.OnStateChanged -= OnState;
      gameLogic.OnGameOver -= OnGameOver;
      gameLogic.OnBoardUpdated -= OnBoard;
    }

    public void StartPressed() {
      StartGameUI();
    }

    public void RetryPressed() {
      StartGameUI();
    }

    // UIボタン経由のゲーム開始
    private void StartGameUI() {
      if (gameLogic == null) {
        return;
      }

      gameLogic.StartGame();
    }

    private void OnState(GameLogic.GameState state) {
      if (startPanel != null) {
        startPanel.SetActive(state == GameLogic.GameState.PreGame);
      }

      if (gameOverPanel != null) {
        gameOverPanel.SetActive(state == GameLogic.GameState.GameOver);
      }

      if (state != GameLogic.GameState.GameOver) {
        RefreshGameOver();
      }
    }

    private void OnGameOver() {
      RefreshGameOver();

      if (gameOverPanel != null && !gameOverPanel.activeSelf) {
        gameOverPanel.SetActive(true);
      }
    }

    private void OnBoard() {
      RefreshHud();
    }

    // HUD上のスコア・レベル・ライン数を最新化
    private void RefreshHud() {
      if (gameLogic == null) {
        return;
      }

      if (hudScoreText != null) {
        hudScoreText.text = $"Score: {gameLogic.Score}";
      }

      if (hudLevelText != null) {
        hudLevelText.text = $"Level: {gameLogic.Level}";
      }

      if (hudLinesText != null) {
        hudLinesText.text = $"Lines: {gameLogic.LinesCleared}";
      }
    }

    // リザルト表示を更新
    private void RefreshGameOver() {
      if (gameLogic == null) {
        return;
      }

      if (gameOverScoreText != null) {
        gameOverScoreText.text = $"Score: {gameLogic.Score}";
      }

      if (gameOverLevelText != null) {
        gameOverLevelText.text = $"Level: {gameLogic.Level}";
      }

      if (gameOverLinesText != null) {
        gameOverLinesText.text = $"Lines: {gameLogic.LinesCleared}";
      }
    }
  }
}
