using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace AlphaTetris {
  public class InputController : MonoBehaviour {
    [SerializeField] private GameLogic logic;

    void Reset() {
      if (logic == null) {
        logic = FindFirstObjectByType<GameLogic>();
      }
    }

    private void Update() {
      if (logic == null) {
        return;
      }

      var kb = Keyboard.current;
      if (kb == null) {
        return;
      }

      if (kb.spaceKey.wasPressedThisFrame) logic.HardDrop();
      if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) logic.MoveLeft();
      if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) logic.MoveRight();
      if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) logic.SoftDrop();
      if (kb.zKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame) logic.RotateLeft();
      if (kb.xKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame) logic.RotateRight();
      if (kb.cKey.wasPressedThisFrame) logic.Hold();

      if (kb.enterKey.wasPressedThisFrame) {
        if (logic.CurrentState is GameState.PreGame or GameState.GameOver) {
          logic.StartGame();
        }
      }
    }
  }
}