using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaTetris {
  [RequireComponent(typeof(RectTransform))]
  public class BoardView : MonoBehaviour {
    [SerializeField] private RectTransform _root;
    [SerializeField] private Image _cellPrefab;
    [SerializeField] private Color _emptyColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color _lockedColor = Color.white;
    [SerializeField] private Color _activeColor = Color.white;

    private readonly List<Image> _cells = new();
    private int _width;
    private int _height;

    private void Awake() {
      if (_root == null) {
        _root = (RectTransform)transform;
      }

      if (_cellPrefab != null) {
        _cellPrefab.gameObject.SetActive(false);
      }
    }

    public void Clear() {
      for (var i = 0; i < _cells.Count; i++) {
        _cells[i].color = _emptyColor;
      }
    }

    public void Render(int[,] board) {
      if (board == null || board.Length == 0) {
        Clear();
        return;
      }

      var height = board.GetLength(0);
      var width = board.GetLength(1);
      EnsureGrid(width, height);

      if (_cells.Count < width * height) {
        Debug.LogWarning($"BoardView: insufficient cell instances (have {_cells.Count}, need {width * height}). Check _cellPrefab assignment.");
        return;
      }

      var index = 0;
      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; x++) {
          var cell = _cells[index++];
          cell.color = board[y, x] switch {
            1 => _lockedColor,
            2 => _activeColor,
            _ => _emptyColor
          };
          cell.enabled = board[y, x] != 0;
        }
      }
    }

    private void EnsureGrid(int width, int height) {
      if (_root == null) {
        _root = (RectTransform)transform;
      }

      if (_cellPrefab == null || _root == null) {
        return;
      }

      if (_width == width && _height == height && _cells.Count == width * height) {
        return;
      }

      foreach (var cell in _cells) {
        if (cell != null) {
          if (Application.isPlaying) {
            Destroy(cell.gameObject);
          } else {
            DestroyImmediate(cell.gameObject);
          }
        }
      }

      _cells.Clear();
      _width = width;
      _height = height;

      // Instantiate grid from top row to bottom row so GridLayoutGroup lays out correctly.
      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; x++) {
          var instance = Instantiate(_cellPrefab, _root);
          instance.gameObject.SetActive(true);
          instance.name = $"Cell_{x}_{y}";
          instance.raycastTarget = false;
          _cells.Add(instance);
        }
      }
    }
  }
}
