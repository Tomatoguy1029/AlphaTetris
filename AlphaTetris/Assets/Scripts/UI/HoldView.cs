using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaTetris {
  [RequireComponent(typeof(RectTransform))]
  public class HoldView : MonoBehaviour {
    [SerializeField] private RectTransform _root;
    [SerializeField] private Image _cellPrefab;
    [SerializeField] private Color _emptyColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color _minoColor = Color.white;

    private readonly List<Image> _cells = new();

    private void Awake() {
      if (_root == null) {
        _root = (RectTransform)transform;
      }

      if (_cellPrefab != null) {
        _cellPrefab.gameObject.SetActive(false);
      }

      EnsureGrid();
    }

    public void Render(Tetrimino? mino) {
      EnsureGrid();

      if (mino == null || mino.Shape == null) {
        SetAllCells(_emptyColor);
        return;
      }

      var color = _minoColor;
      var shape = mino.Shape;
      var index = 0;
      for (var y = 0; y < shape.GetLength(0); y++) {
        for (var x = 0; x < shape.GetLength(1); x++) {
          _cells[index++].color = shape[y, x] == 0 ? _emptyColor : color;
        }
      }
    }

    private void EnsureGrid() {
      if (_root == null) {
        _root = (RectTransform)transform;
      }

      if (_cellPrefab == null || _root == null) {
        return;
      }

      if (_cells.Count == 16) {
        return;
      }

      for (var i = 0; i < _cells.Count; i++) {
        if (_cells[i] != null) {
          if (Application.isPlaying) {
            Destroy(_cells[i].gameObject);
          } else {
            DestroyImmediate(_cells[i].gameObject);
          }
        }
      }

      _cells.Clear();
      for (var y = 0; y < 4; y++) {
        for (var x = 0; x < 4; x++) {
          var instance = Instantiate(_cellPrefab, _root);
          instance.gameObject.SetActive(true);
          instance.name = $"HoldCell_{x}_{y}";
          instance.raycastTarget = false;
          _cells.Add(instance);
        }
      }
    }

    private void SetAllCells(Color color) {
      foreach (var cell in _cells) {
        cell.color = color;
      }
    }
  }
}
