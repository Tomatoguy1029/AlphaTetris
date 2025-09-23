using System;
using UnityEngine;

namespace AlphaTetris {
  public class GameLogic : MonoBehaviour {
    public int width = 10;
    public int height = 20;

    public GameLogic(int width, int height) {
      this.width = width;
      this.height = height;
      _board = new int[width, height];
      SpawnMino();
    }

    //　状態
    private int[,] _board;
    private Tetrimino _currentMino;
    private Vector2Int _currentPos;

    // プロパティ
    public int[,] RenderBoard { get; private set; }
    public int Score { get; private set; }
    public int Level { get; private set; }
    public int LinesCleared { get; private set; }

    // イベント
    public event Action OnBoardUpdated;
    public event Action OnGameOver;

    private float _fallTimer;
    private float _fallInterval = 1f;

    private const int LevelInterval = 10;

    private void Start() {
      _board = new int[width, height];
      RenderBoard = new int[width, height];
      SpawnMino();
      UpdateRenderedBoard();
    }

    private void Update() {
      _fallTimer += Time.deltaTime;
      if (_fallTimer >= _fallInterval) {
        // 経過時間がfallIntervalを超えたらミノを1マス落下
        Step();
        _fallTimer = 0f;
      }
    }

    // API、UI側で使用
    public void MoveLeft() => TryMove(Vector2Int.left);
    public void MoveRight() => TryMove(Vector2Int.right);
    public void SoftDrop() => Step();
    public void RotateRight() => Rotate(1);
    public void RotateLeft() => Rotate(-1);

    // =====内部処理=====
    // テトリミノを落下させる
    private void SpawnMino() {
      _currentMino = Tetrimino.GetRandom();
      _currentPos = new Vector2Int(width / 2 - 2, height - 2);

      // ゲームオーバー処理
      if (!IsValidPosition(_currentPos, _currentMino.Shape)) {
        OnGameOver?.Invoke();
        enabled = false;
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
      if (!IsValidPosition(next, _currentMino.Shape)) {
        _currentPos = next;
      }

      UpdateRenderedBoard();
    }

    // テトリミノを回転
    private void Rotate(int dir) {
      int[,] rotated = _currentMino.Rotate(dir);
      if (!IsValidPosition(_currentPos, rotated)) {
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
    //  TODO 現状だと一行ずつしか消えないので改善の余地あり
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
          for (var yy = y; yy < height; yy++) {
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
        if (x < 0 || x > width || y < 0) {
          return false;
        }

        // すでにブロックがあるかどうか
        if (y < height && _board[y, x] == 1) {
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
  }
}