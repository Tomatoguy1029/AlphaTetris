#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AlphaTetris {
  public class GameBoard {
    public readonly int Width;
    public readonly int Height;

    private readonly int[,] _board;
    private Tetrimino _currentMino;
    private Vector2Int _currentPos; // 左上基準
    private bool _holdUsed;
    private Tetrimino? _holdMino;
    private readonly Queue<Tetrimino> _nextMino = new();

    private float _fallTimer;
    private float _fallInterval = 1.0f;
    private const int LevelInterval = 10;

    private const int MinoHeight = 4;
    private const int MinoWidth = 4;

    // 結果表示用テキスト
    public int Score { get; private set; }
    public int Level { get; private set; } = 1;
    public int TotalLinesCleared { get; private set; }

    // 公開プロパティ
    public Tetrimino? HoldMino => _holdMino;
    public IReadOnlyList<Tetrimino> NextPreview => new List<Tetrimino>(_nextMino);

    // イベント
    public event Action? OnBoardUpdated;
    public event Action? OnGameOver;

    // コンストラクタ
    public GameBoard(int width, int height) {
      Width = width;
      Height = height;
      _board = new int[Height, Width];
      ResetAll();
    }

    // 初期化
    public void ResetAll() {
      Array.Clear(_board, 0, _board.Length);
      Score = 0;
      Level = 1;
      TotalLinesCleared = 0;
      _holdMino = null;
      _holdUsed = false;
      _nextMino.Clear();
      _fallTimer = 0f;
      _fallInterval = LevelToInterval(Level);
      RefillBag();
    }

    /// <summary>
    /// Board, Current, Posから操作中の盤面を返すプロパティ
    /// </summary>
    public int[,] RenderBoard {
      get　{
        var copy = new int[Height, Width];
        for (var y = 0; y < Height; y++) {
          for (var x = 0; x < Width; x++) {
            copy[y, x] = _board[y, x];
          }
        }

        // ミノの位置を反映
        if (_currentMino.Shape != null) {
          for (var y = 0; y < MinoHeight; y++) {
            for (var x = 0; x < MinoWidth; x++) {
              if (_currentMino.Shape[y, x] == 0) {
                continue;
              }

              var gx = _currentPos.x + x;
              var gy = _currentPos.y + y;
              if (InBoard(gx, gy)) {
                copy[gy, gx] = 2;
              }
            }
          }
        }

        return copy;
      }
    }

    // ===============外部用API==============
    /// <summary>
    /// 時間進行用メソッド
    /// </summary>
    public void Tick(float deltaTime) {
      _fallTimer += deltaTime;
      if (_fallTimer >= _fallInterval) {
        _fallTimer = 0f;
        Step();
      }
    }

    /// <summary>
    /// ミノの生成用メソッド
    /// </summary>
    public void SpawnMino() {
      if (_nextMino.Count < 7) {
        RefillBag();
      }

      _currentMino = _nextMino.Dequeue();
      _currentPos = GetSpawnPosition(_currentMino);

      if (!IsValidPosition(_currentMino.Shape, _currentPos)) {
        OnGameOver?.Invoke();
        return;
      }

      OnBoardUpdated?.Invoke();
    }

    public void TryMoveHorizontal(int dir) {
      var next = new Vector2Int(_currentPos.x + dir, _currentPos.y);
      if (IsValidPosition(_currentMino.Shape, next)) {
        _currentPos = next;
        OnBoardUpdated?.Invoke();
      }
    }

    public void Rotate(int dir) {
      var rotated = (dir >= 0) ? Tetrimino.RotateCw(_currentMino.Shape) :  Tetrimino.RotateCcw(_currentMino.Shape);

      // 回転した時に既存のブロックと重なる場合左右に移動させる(kick)
      Vector2Int[] kick = { Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(-1, 0) };
      foreach (var k in kick) {
        var nextPos = _currentPos + k;
        if (IsValidPosition(rotated, nextPos)) {
          _currentMino = new Tetrimino{Type = _currentMino.Type, Shape = rotated};
          _currentPos = nextPos;
          OnBoardUpdated?.Invoke();
          return;
        }
      }
    }

    // テトリミノの落下処理, 毎ターン&↓ボタンで呼ばれる
    public void Step() {
      var nextPos = new Vector2Int(_currentPos.x, _currentPos.y + 1);
      if (IsValidPosition(_currentMino.Shape, nextPos)) {
        _currentPos = nextPos;
      } else {
        // ミノが下についた場合
        if (!IsMinoFullyVisible()) {
          OnGameOver?.Invoke();
          return;
        }

        PlaceMino();

        int cleared = ClearLines();
        if (cleared > 0) {
          TotalLinesCleared += cleared;
          Score += ScoreFor(cleared, softDrop: false, hardDrop: false);
          if (TotalLinesCleared / LevelInterval + 1 > Level) { // _levelIntervalだけ列を消すごとにLevelを増加
            Level = TotalLinesCleared / LevelInterval + 1;
            _fallInterval = LevelToInterval(Level);
          }
        }

        _holdUsed = false;
        SpawnMino();
      }

      OnBoardUpdated?.Invoke();
    }

    // テトリミノの即時落下
    public void HardDrop() {
      var dropCount = 0;
      var currentPos = _currentPos;
      while (true) {
        var nextPos = new Vector2Int(currentPos.x, currentPos.y + 1);
        if (IsValidPosition(_currentMino.Shape, nextPos)) {
          dropCount++;
          currentPos = nextPos;
        } else {
          break;
        }
      }

      _currentPos.y += dropCount;

      PlaceMino();
      int cleared = ClearLines();
      TotalLinesCleared += cleared;
      Score += ScoreFor(cleared, softDrop: false, hardDrop: true) + dropCount * 2;　// 公式ルール的には落下した距離に比例するらしい
      if (TotalLinesCleared / LevelInterval + 1 > Level) {
        Level = TotalLinesCleared / LevelInterval + 1;
        _fallInterval = LevelToInterval(Level);
      }

      _holdUsed = false;
      SpawnMino();
      OnBoardUpdated?.Invoke();
    }

    // ミノのホールド
    public void Hold() {
      if (_holdUsed) {
        return;
      }

      if (_holdMino == null) {
        _holdMino = _currentMino;
        SpawnMino();
      } else {
        var tmp = _holdMino;
        _holdUsed = true;
        _currentMino = tmp;
        _currentPos = GetSpawnPosition(_currentMino);
        if (!IsValidPosition(_currentMino.Shape, _currentPos)) {
          OnGameOver?.Invoke();
          return;
        }
      }

      OnBoardUpdated?.Invoke();
    }

    // ===============内部処理用==============
    // 落下スピードを調整
    private float LevelToInterval(int level) {
      return Mathf.Max(0.1f, 1f - Level * 0.1f);
    }

    // テトリミノを固定
    private void PlaceMino() {
      for (var y = 0; y < MinoHeight; y++) {
        for (var x = 0; x < MinoWidth; x++) {
          if (_currentMino.Shape[y, x] == 0) {
            continue;
          }

          var nextX = _currentPos.x + x;
          var nextY = _currentPos.y + y;

          // 固定時に盤面からはみ出していたらGameOver
          if (nextY < 0) {
            OnGameOver?.Invoke();
            return;
          }

          if (InBoard(nextX, nextY)) {
            _board[nextY, nextX] = 1;
          }
        }
      }
    }

    // スポーン位置の計算
    private Vector2Int GetSpawnPosition(Tetrimino t) {
      // 盤面外にはみ出す部分の長さ
      var shift = 0;
      for (var y = 0; y < 4; y++) {
        for (var x = 0; x < 4; x++)
          if (t.Shape[y, x] != 0)
            shift = y;
      }

      // 中央上部を指定
      return new Vector2Int((Width - 2) / 2, -shift);
    }

    //　一列消去
    private int ClearLines() {
      var cleared = 0;
      for (var y = Height - 1; y >= 0; y--) {
        var full = true;
        for (var x = 0; x < Width; x++) {
          if (_board[y, x] == 0) {
            full = false;
            break;
          }
        }

        if (full) {
          cleared++;
          for (var yy = y; yy > 0; yy--) {
            for (var xx = 0; xx < Width; xx++) {
              //　1行上をコピー
              _board[yy, xx] = _board[yy - 1, xx];
            }
          }

          // 最上段はコピー元がないため明示的に空にする
          for (var xx = 0; xx < Width; xx++) {
            _board[0, xx] = 0;
          }

          y++;
        }
      }

      return cleared;
    }

    private int ScoreFor(int cleared, bool softDrop, bool hardDrop) {
      switch (cleared) {
        case 1: return 100 * Level;
        case 2: return 300 * Level;
        case 3: return 500 * Level;
        case 4: return 800 * Level;
        default:
          if (softDrop) {
            return 1;
          }

          if (hardDrop) {
            return 2;
          }

          return 0;
      }
    }

    // 7種類のミノをランダムに並べ替えたリストを作成してキューに入れる
    private void RefillBag() {
      var list = new List<TetriminoType> {
        TetriminoType.I, TetriminoType.O, TetriminoType.T, TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L 
      };
      for (var i = 0; i < list.Count; i++) {
        var j = UnityEngine.Random.Range(i, list.Count);
        (list[i], list[j]) = (list[j], list[i]);
      }

      foreach (var t in list) {
        _nextMino.Enqueue(Tetrimino.CreateByType(t));
      }
    }

    // ミノが全て表示領域内かどうか(GameOver判定用)
    private bool IsMinoFullyVisible() {
      for (var y = 0; y < MinoHeight; y++) {
        for (var x = 0; x < MinoWidth; x++) {
          if (_currentMino.Shape[y, x] == 0) {
            continue;
          }

          // 画面外(y < 0)であればfalse
          if (_currentPos.y + y < 0) {
            return false;
          }
        }
      }

      return true;
    }

    // テトリミノが移動可能な位置かどうか(移動ルールの判定用)
    private bool IsValidPosition(int[,] shape, Vector2Int pos) {
      for (var y = 0; y < MinoHeight; y++) {
        for (var x = 0; x < MinoWidth; x++) {
          if (shape[y, x] == 0) {
            continue;
          }

          var nextX = pos.x + x;
          var nextY = pos.y + y;

          // 左右の枠内かどうか(In)
          if (nextX < 0 || nextX >= Width) {
            return false;
          }

          // 下にはみ出す場合
          if (nextY >= Height) {
            return false;
          }

          // 次のマスにブロックがある場合
          if (nextY >= 0 && _board[nextY, nextX] == 1) {
            return false;
          }
        }
      }

      return true;
    }

    // 盤面内にあるかどうか
    private bool InBoard(int x, int y) {
      return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    // TODO デバッグ用
    public void PrintBoardToConsole() {
      var sb = new StringBuilder();
      sb.AppendLine("\n======== TETRIS ========");

      for (var y = 0; y < Height; y++) {
        for (var x = 0; x < Width; x++) {
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
      sb.AppendLine($"Lines: {TotalLinesCleared}");
      sb.AppendLine("======================");

      Debug.Log(sb.ToString());
    }
  }
}