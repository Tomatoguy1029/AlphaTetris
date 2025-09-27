using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public class NextQueueView : MonoBehaviour {
    [SerializeField] private Transform _root;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField, Min(0.01f)] private float _cellSize = 0.5f;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Color _minoColor = Color.white;

    private readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>();
    private readonly List<Vector2Int> _buffer = new List<Vector2Int>();

    private void Awake() {
      if (_root == null) {
        _root = transform;
      }
    }

    public void DrawMino(Tetrimino mino) {
      if (_cellPrefab == null) {
        Debug.LogWarning("NextQueueView: Cell prefab is not assigned.");
        return;
      }

      if (mino == null) {
        HideAll();
        return;
      }

      _buffer.Clear();
      foreach (var cell in Tetrimino.GetCells(mino.Shape)) {
        _buffer.Add(cell);
      }

      if (_buffer.Count == 0) {
        HideAll();
        return;
      }

      EnsurePoolSize(_buffer.Count);

      int minX = int.MaxValue;
      int maxX = int.MinValue;
      int minY = int.MaxValue;
      int maxY = int.MinValue;
      for (int i = 0; i < _buffer.Count; i++) {
        var cell = _buffer[i];
        if (cell.x < minX) minX = cell.x;
        if (cell.x > maxX) maxX = cell.x;
        if (cell.y < minY) minY = cell.y;
        if (cell.y > maxY) maxY = cell.y;
      }

      float centerX = (minX + maxX) * 0.5f;
      float centerY = (minY + maxY) * 0.5f;

      int index = 0;
      for (; index < _buffer.Count; index++) {
        var renderer = _pool[index];
        var go = renderer.gameObject;
        if (!go.activeSelf) {
          go.SetActive(true);
        }

        var pt = _buffer[index];
        var local = new Vector3((pt.x - centerX) * _cellSize, (pt.y - centerY) * _cellSize, 0f) + _offset;

        var t = renderer.transform;
        t.SetParent(_root, false);
        t.localPosition = local;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one * _cellSize;

        renderer.color = _minoColor;
      }

      for (; index < _pool.Count; index++) {
        var go = _pool[index].gameObject;
        if (go.activeSelf) {
          go.SetActive(false);
        }
      }
    }

    private void HideAll() {
      for (int i = 0; i < _pool.Count; i++) {
        var go = _pool[i].gameObject;
        if (go.activeSelf) {
          go.SetActive(false);
        }
      }
    }

    private void EnsurePoolSize(int target) {
      while (_pool.Count < target) {
        var instance = Instantiate(_cellPrefab, _root);
        instance.SetActive(false);

        var renderer = instance.GetComponentInChildren<SpriteRenderer>();
        if (renderer == null) {
          renderer = instance.AddComponent<SpriteRenderer>();
        }

        renderer.transform.localPosition = Vector3.zero;
        renderer.transform.localRotation = Quaternion.identity;
        renderer.transform.localScale = Vector3.one * _cellSize;

        _pool.Add(renderer);
      }
    }
  }
}
