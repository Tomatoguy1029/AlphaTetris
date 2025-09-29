using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public enum GameState {
    PreGame,
    Playing,
    Paused,
    GameOver
  }

  public class GameLogic : MonoBehaviour {
    [Header("Board Size")] [SerializeField]
    private int width = 10;

    [SerializeField] private int height = 20;

    private static readonly int[,] Empty = new int[0, 0];

    //　状態
    private GameBoard _gameBoard;
    private GameState _state = GameState.PreGame;

    // 公開プロパティ
    public GameState CurrentState => _state;
    public int[,] RenderBoard => _gameBoard?.RenderBoard ?? Empty;
    public Tetrimino HoldMino => _gameBoard?.HoldMino;
    public IReadOnlyList<Tetrimino> NextPreview => _gameBoard?.NextPreview;
    public int Score => _gameBoard?.Score ?? 0;
    public int Level => _gameBoard?.Level ?? 1;
    public int LinesCleared => _gameBoard?.TotalLinesCleared ?? 0;

    // UIPresenterで購読するイベント
    public event Action OnBoardUpdated;
    public event Action OnGameOver;

    private void Awake() {
      Debug.Log("Press Enter to start game");
      _gameBoard = new GameBoard(width, height);
      _gameBoard.OnBoardUpdated += () => {
        OnBoardUpdated?.Invoke();
        _gameBoard.PrintBoardToConsole(); // デバッグ用
      };
      _gameBoard.OnGameOver += () => {
        _state = GameState.GameOver;
        Debug.Log("GameOver!");
        OnGameOver?.Invoke();// UI側に通知
      };
    }

    private void Update() {
      if (_state != GameState.Playing) {
        return;
      }

      // フレームごとの処理を走らせる
      _gameBoard.Tick(Time.deltaTime);
    }

    // ==========ゲームフローの管理用API==========
    // GameStateを変えるときはこれを呼び出す
    public void StartGame() {
      _gameBoard.ResetAll();
      _gameBoard.SpawnMino();
      _state = GameState.Playing;
      OnBoardUpdated?.Invoke();
    }

    public void PauseGame() {
      if (_state != GameState.Playing) {
        return;
      }

      _state = GameState.Paused;
    }

    public void ResumeGame() {
      if (_state != GameState.Paused) {
        return;
      }

      _state = GameState.Playing;
    }

    // =========操作用API==========
    public void MoveLeft() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.TryMoveHorizontal(-1);
      }
    }

    public void MoveRight() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.TryMoveHorizontal(+1);
      }
    }

    public void SoftDrop() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.Step();
      }
    }

    public void HardDrop() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.HardDrop();
      }
    }

    public void RotateRight() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.Rotate(1);
      }
    }

    public void RotateLeft() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.Rotate(-1);
      }
    }

    public void Hold() {
      if (CurrentState == GameState.Playing) {
        _gameBoard.Hold();
      }
    }
  }
}