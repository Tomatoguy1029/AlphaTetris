using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlphaTetris {
  public class GameLogic : MonoBehaviour {
    public enum GameState {
      PreGame,
      Playing,
      GameOver
    }

    public int width = 10;
    public int height = 20;

    //　状態
    private int[,] _board;
    private Tetrimino _currentMino;
    private Vector2Int _currentPos;

    // プロパティ
    public int[,] RenderBoard { get; private set; }
    public int Score { get; private set; }
    public int Level { get; private set; }
    public int LinesCleared { get; private set; }
    public GameState CurrentState { get; private set; }

    // イベント
    public event Action OnBoardUpdated;
    public event Action OnGameOver;

    private float _fallTimer;
    private float _fallInterval = 1f;

    private const int LevelInterval = 10;

    private void Start() {
      _board = new int[height, width];
      RenderBoard = new int[height, width];

      OnGameOver += () => {
        Debug.Log("GameOver");
        CurrentState = GameState.GameOver;
      };

      CurrentState = GameState.PreGame;
      Debug.Log("Push Space button to play");
    }

    private void Update() {
      if (CurrentState != GameState.Playing) {
        return;
      }

      // 毎フレームの落下処理
      _fallTimer += Time.deltaTime;
      if (_fallTimer >= _fallInterval) {
        Step();
        _fallTimer = 0f;
        PrintBoardToConsole();
      }
    }

    // ======API======
    //　ゲーム開始
    public void StartGame() {
      if (CurrentState != GameState.PreGame) {
        return;
      }

      // 初期化
      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; x++) {
          _board[y, x] = 0;
        }
      }

      Score = 0;
      Level = 0;
      LinesCleared = 0;
      _fallTimer = 0f;
      _fallInterval = 1f;

      // ゲーム開始
      CurrentState = GameState.Playing;
      SpawnMino();
      PrintBoardToConsole();
    }

    public void MoveLeft() {
      if (CurrentState != GameState.Playing) return;
      TryMove(Vector2Int.left);
    }

    public void MoveRight() {
      if (CurrentState != GameState.Playing) return;
      TryMove(Vector2Int.right);
    }

    public void SoftDrop() {
      if (CurrentState != GameState.Playing) return;
      Step();
    }

    public void RotateRight() {
      if (CurrentState != GameState.Playing) return;
      Rotate(1);
    }

    public void RotateLeft() {
      if (CurrentState != GameState.Playing) return;
      Rotate(-1);
    }

    // ======内部処理======
    // テトリミノを落下させる
    private void SpawnMino() {
      _currentMino = Tetrimino.GetRandom();
      _currentPos = new Vector2Int(width / 2 - 2, height - 2);

      // ゲームオーバー処理
      if (!IsValidPosition(_currentPos, _currentMino.Shape)) {
        OnGameOver?.Invoke();
      }

      UpdateRenderedBoard();
    }

    // テトリミノの落下, 毎ターン&↓ボタンで呼ばれる
    private void Step() {
      Vector2Int next = _currentPos + Vector2Int.down;
      if (IsValidPosition(next, _currentMino.Shape)) {
        _currentPos = next;
      } else {
        // ミノが下についた場合
        PlaceMino();
        ClearLines();
        UpdateSpeed();
        SpawnMino();
      }

      UpdateRenderedBoard();
    }

    // テトリミノを左右に移動
    private void TryMove(Vector2Int dir) {
      Vector2Int next = _currentPos + dir;
      if (IsValidPosition(next, _currentMino.Shape)) {
        _currentPos = next;
      }

      UpdateRenderedBoard();
    }

    // テトリミノを回転
    private void Rotate(int dir) {
      int[,] rotated = _currentMino.Rotate(dir);
      if (IsValidPosition(_currentPos, rotated)) {
        _currentMino.Shape = rotated;
      }

      UpdateRenderedBoard();
    }

    // テトリミノを固定
    private void PlaceMino() {
      foreach (var cell in Tetrimino.GetCells(_currentMino.Shape)) {
        var x = _currentPos.x + cell.x;
        var y = _currentPos.y + cell.y;
        if (y >= 0 && y < height && x >= 0 && x < width) {
          _board[y, x] = 1;
        }
      }
    }

    // 一列揃った時に列を削除
    private void ClearLines() {
      int cleared = 0;
      for (int y = 0; y < height; y++) {
        var full = true;
        for (var x = 0; x < width; x++) {
          if (_board[y, x] == 0) {
            full = false;
            break;
          }
        }

        if (full) {
          cleared++;
          for (var yy = y; yy < height - 1; yy++) {
            for (var xx = 0; xx < width; xx++) {
              //　1行上をコピー
              _board[yy, xx] = _board[yy + 1, xx];
            }
          }

          // 最上段を空に
          for (var xx = 0; xx < width; xx++) {
            _board[height - 1, xx] = 0;
          }

          y--;
        }
      }

      if (cleared > 0) {
        int[] scoreTable = { 0, 100, 300, 500, 800 };
        Score += scoreTable[cleared] * (Level + 1);
        LinesCleared += cleared;
        if (LinesCleared / LevelInterval > Level) {
          Level++;
        }
      }
    }

    // 落下スピードを調整
    private void UpdateSpeed() {
      _fallInterval = Mathf.Max(0.1f, 1f - Level * 0.1f);
    }

    // テトリミノの可動域を制御
    private bool IsValidPosition(Vector2Int pos, int[,] shape) {
      foreach (var cell in Tetrimino.GetCells(shape)) {
        var x = pos.x + cell.x;
        var y = pos.y + cell.y;

        // 枠内かどうか
        if (x < 0 || x >= width || y < 0) {
          return false;
        }

        // 上にはみ出した場合はスキップ
        if (y >= height) {
          continue;
        }

        // すでにブロックがあるかどうか
        if (_board[y, x] == 1) {
          return false;
        }
      }

      return true;
    }

    // 盤面更新
    private void UpdateRenderedBoard() {
      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; x++) {
          RenderBoard[y, x] = _board[y, x];
        }
      }

      foreach (var cell in Tetrimino.GetCells(_currentMino.Shape)) {
        int x = _currentPos.x + cell.x;
        int y = _currentPos.y + cell.y;
        if (x >= 0 && x < width && y >= 0 && y < height) {
          // 落下中
          RenderBoard[y, x] = 2;
        }
      }

      OnBoardUpdated?.Invoke();
    }

    // TODO デバッグ用
    public void PrintBoardToConsole() {
      var sb = new StringBuilder();
      sb.AppendLine("\n======== TETRIS ========");

      for (var y = height - 1; y >= 0; y--) {
        for (var x = 0; x < width; x++) {
          sb.Append(RenderBoard[y, x] switch {
            0 => " .", // 0: 空白
            1 => " ■", // 1: 固定されたブロック
            2 => " □", // 2: 操作中のブロック
            _ => " ?"
          });
        }

        sb.AppendLine();
      }

      sb.AppendLine("----------------------");

      sb.AppendLine($"Score: {Score}");
      sb.AppendLine($"Level: {Level}");
      sb.AppendLine($"Lines: {LinesCleared}");
      sb.AppendLine("======================");

      Debug.Log(sb.ToString());
    }
  }
}