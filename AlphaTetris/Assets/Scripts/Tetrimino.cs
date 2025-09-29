using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public enum TetriminoType {
    I,
    O,
    S,
    Z,
    J,
    L,
    T
  }

  public class Tetrimino {
    public TetriminoType Type;  // 特定のミノ専用の判定(ex.Tスピン)用、view側での色やプレビュー用
    public int[,] Shape;

    public static Tetrimino CreateByType(TetriminoType type) {
      return new Tetrimino {
        Type = type,
        Shape = ShapeOf(type),
      };
    }

    // テトリミノ回転後の盤面を取得
    // 時計回り (Clockwise)
    public static int[,] RotateCw(int[,] shape) {
      var n = shape.GetLength(0);
      var result = new int[n, n];

      for (var y = 0; y < n; y++) {
        for (var x = 0; x < n; x++) {
          // (y,x) = (2,0) -> (0,0)
          result[x, n - 1 - y] = shape[y, x];
        }
      }

      return result;
    }

    // 反時計回り (Counter ClockWise)
    public static int[,] RotateCcw(int[,] shape) {
      var n = shape.GetLength(0);
      var result = new int[n, n];

      for (var y = 0; y < n; y++) {
        for (var x = 0; x < n; x++) {
          // (y,x) = (2,0) -> (0,0)
          result[x, n - 1 - y] = shape[y, x];
        }
      }

      return result;
    }

    // ミノの二次元配列を取得
    // Unity側で形と4×4の中での相対座標決めておけばいいからこれいらないかも？
    // いやでも列の消去とか考えたら配列で管理する方がいいのか
    // その場合[SerializeField]とかでミノのプレハブアタッチさせればいいんか？
    private static int[,] ShapeOf(TetriminoType type) {
      switch (type) {
        case TetriminoType.I:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 1, 1, 1, 1 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
          };
        case TetriminoType.O:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 0, 1, 1, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 }
          };
        case TetriminoType.S:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 0, 1, 1, 0 },
            { 1, 1, 0, 0 },
            { 0, 0, 0, 0 }
          };
        case TetriminoType.Z:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 1, 1, 0, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 }
          };
        case TetriminoType.J:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 1, 0, 0, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 0, 0 }
          };
        case TetriminoType.L:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 0, 0, 1, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 0, 0 }
          };
        case TetriminoType.T:
          return new int[,] {
            { 0, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 0, 0 }
          };
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}