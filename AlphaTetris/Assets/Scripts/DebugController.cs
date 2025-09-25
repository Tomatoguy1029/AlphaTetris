using UnityEngine;
using UnityEngine.InputSystem;

namespace AlphaTetris {
  // TODO デバッグ用コントローラー
  public class DebugInputController : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;

    private void Update() {
      var currentState = _gameLogic.CurrentState;

      // スペースキーでゲーム開始
      if (currentState != GameLogic.GameState.Playing && Keyboard.current.spaceKey.wasPressedThisFrame) {
        _gameLogic.StartGame();
      }

      // ミノ操作
      if (currentState == GameLogic.GameState.Playing) {
        var kb = Keyboard.current;
        if (kb.leftArrowKey.wasPressedThisFrame) _gameLogic.MoveLeft();
        if (kb.rightArrowKey.wasPressedThisFrame) _gameLogic.MoveRight();
        if (kb.downArrowKey.wasPressedThisFrame) _gameLogic.SoftDrop();
        if (kb.zKey.wasPressedThisFrame) _gameLogic.RotateLeft();
        if (kb.xKey.wasPressedThisFrame) _gameLogic.RotateRight();
      }
    }
  }
}