using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public class Tetrimino {
    public int[,] Shape;

    // コンストラクタ
    private Tetrimino(int[,] shape) {
      this.Shape = shape;
    }

    // テトリミノのランダム生成
    // TODO 完全にランダムなので改善の余地あり
    public static Tetrimino GetRandom() {
      var i = UnityEngine.Random.Range(0, Shapes.GetLength(0));
      return new Tetrimino(Shapes[i]);
    }

    // ミノのブロックの相対座標を取得
    public static IEnumerable<Vector2Int> GetCells(int[,] s) {
      for (var y = 0; y < s.GetLength(0); y++) {
        for (int x = 0; x < s.GetLength(1); x++) {
          if (s[y, x] == 1) {
            yield return new Vector2Int(x, (s.GetLength(0) - 1) - y);
          }
        }
      }
    }

    // テトリミノ回転後の盤面を取得
    public int[,] Rotate(int dir) {
      int n = Shape.GetLength(0);
      int[,] result = new int[n, n];

      if (dir > 0) {
        // 時計回り
        for (var y = 0; y < n; y++) {
          for (var x = 0; x < n; x++) {
            // (y,x) = (2,0) -> (0,0)
            result[x, n - 1 - y] = Shape[y, x];
          }
        }
      } else {
        // 反時計回り
        for (var y = 0; y < n; y++) {
          for (var x = 0; x < n; x++) {
            // (y,x) = (2,0) -> (2,2)
            result[n - 1 - x, y] = Shape[y, x];
          }
        }
      }

      return result;
    }

    private static readonly int[][,] Shapes = {
      // I
      new int[,] {
        { 0, 0, 0, 0 },
        { 1, 1, 1, 1 },
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 }
      },

      // O
      new int[,] {
        { 0, 0, 0, 0 },
        { 0, 1, 1, 0 },
        { 0, 1, 1, 0 },
        { 0, 0, 0, 0 }
      },

      // S
      new int[,] {
        { 0, 0, 0, 0 },
        { 0, 1, 1, 0 },
        { 1, 1, 0, 0 },
        { 0, 0, 0, 0 }
      },

      // Z
      new int[,] {
        { 0, 0, 0, 0 },
        { 1, 1, 0, 0 },
        { 0, 1, 1, 0 },
        { 0, 0, 0, 0 }
      },

      // J
      new int[,] {
        { 0, 0, 0, 0 },
        { 1, 0, 0, 0 },
        { 1, 1, 1, 0 },
        { 0, 0, 0, 0 }
      },

      // L
      new int[,] {
        { 0, 0, 0, 0 },
        { 0, 0, 1, 0 },
        { 1, 1, 1, 0 },
        { 0, 0, 0, 0 }
      },

      // T
      new int[,] {
        { 0, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 1, 1, 1, 0 },
        { 0, 0, 0, 0 }
      }
    };
  }
}