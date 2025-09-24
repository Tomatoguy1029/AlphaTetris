using System.Text;
using UnityEngine;
using TMPro;

namespace AlphaTetris {
  [RequireComponent(typeof(TextMeshProUGUI))]
  public class BoardTextView : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private bool _overrideRectTransform = true;
    [SerializeField] private Vector2 _anchorMin = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 _anchorMax = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 _pivot = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 _anchoredPosition = Vector2.zero;
    [SerializeField] private TextAlignmentOptions _alignment = TextAlignmentOptions.Midline;
    [SerializeField] private TMP_FontAsset _fontAsset;
    [SerializeField] private Vector2 _sizeDelta = new Vector2(400f, 600f);

    private readonly StringBuilder _builder = new StringBuilder();

    private void Awake() {
      if (_text == null) {
        _text = GetComponent<TextMeshProUGUI>();
      }

      if (_text != null) {
        if (_fontAsset != null) {
          _text.font = _fontAsset;
        }

        _text.alignment = _alignment;
        _text.enableWordWrapping = false;
        _text.enableKerning = false;

        if (_overrideRectTransform) {
          var rect = _text.rectTransform;
          rect.anchorMin = _anchorMin;
          rect.anchorMax = _anchorMax;
          rect.pivot = _pivot;
          rect.anchoredPosition = _anchoredPosition;
          rect.sizeDelta = _sizeDelta;
        }
      }
    }

    private void OnEnable() {
      if (_gameLogic == null) {
        Debug.LogWarning("BoardTextView: GameLogic reference is missing.");
        return;
      }

      _gameLogic.OnBoardUpdated += HandleBoardUpdated;
      _gameLogic.OnGameOver += HandleBoardUpdated;
      HandleBoardUpdated();
    }

    private void OnDisable() {
      if (_gameLogic == null) {
        return;
      }

      _gameLogic.OnBoardUpdated -= HandleBoardUpdated;
      _gameLogic.OnGameOver -= HandleBoardUpdated;
    }

    private void HandleBoardUpdated() {
      if (_gameLogic == null || _text == null) {
        return;
      }

      var board = _gameLogic.RenderBoard;
      if (board == null) {
        _text.SetText(string.Empty);
        return;
      }

      var height = board.GetLength(0);
      var width = board.GetLength(1);

      _builder.Clear();
      _builder.AppendLine("======== TETRIS ========");

      for (var y = height - 1; y >= 0; y--) {
        for (var x = 0; x < width; x++) {
          _builder.Append(board[y, x] switch {
            0 => " .",
            1 => " ■",
            2 => " □",
            _ => " ?"
          });
        }

        _builder.AppendLine();
      }

      _builder.AppendLine("----------------------");
      _builder.AppendLine($"Score: {_gameLogic.Score}");
      _builder.AppendLine($"Level: {_gameLogic.Level}");
      _builder.AppendLine($"Lines: {_gameLogic.LinesCleared}");
      _builder.AppendLine("======================");

      if (_text != null) {
        _text.SetText(_builder.ToString());
      }
    }
  }
}
