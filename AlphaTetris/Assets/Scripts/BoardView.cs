using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public class BoardView : MonoBehaviour {
    [SerializeField] private Transform _root;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private RectTransform _trayRect;
    [SerializeField] private SpriteRenderer _traySprite;
    [SerializeField] private Vector2 _trayPadding = Vector2.zero;
    [SerializeField, Min(0.1f)] private float _cellSize = 1f;
    [SerializeField] private Vector2 _origin;
    [SerializeField] private bool _useRectTransformLayout = true;
    [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color _activeColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>();

    private void Awake() {
      if (_root == null) {
        _root = transform;
      }
    }

    public float CellSize => _cellSize;
    public Vector2 Origin => _origin;

    public void Draw(int[,] boardData) {
      if (_cellPrefab == null) {
        Debug.LogError("BoardView: Cell prefab is not assigned.");
        return;
      }

      if (boardData == null) {
        HideAll();
        ResizeTray(0, 0);
        return;
      }

      int height = boardData.GetLength(0);
      int width = boardData.GetLength(1);
      int required = width * height;

      ResizeTray(width, height);

      EnsurePoolSize(required);

      int index = 0;
      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++, index++) {
          var renderer = _pool[index];
          var go = renderer.gameObject;
          int value = boardData[y, x];

          if (value == 0) {
            if (go.activeSelf) {
              go.SetActive(false);
            }
            continue;
          }

          if (!go.activeSelf) {
            go.SetActive(true);
          }

          var t = renderer.transform;
          t.SetParent(_root, false);
          if (_useRectTransformLayout && t is RectTransform rect) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = _origin + new Vector2(x * _cellSize, y * _cellSize);
            rect.sizeDelta = Vector2.one * _cellSize;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
          } else {
            float half = _cellSize * 0.5f;
            t.localPosition = new Vector3(_origin.x + (x * _cellSize) + half, _origin.y + (y * _cellSize) + half, 0f);
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one * _cellSize;
          }

          renderer.color = value == 1 ? _lockedColor : _activeColor;
        }
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

    private void ResizeTray(int width, int height) {
      if (_trayRect != null) {
        if (width <= 0 || height <= 0) {
          _trayRect.gameObject.SetActive(false);
          return;
        }

        _trayRect.gameObject.SetActive(true);
        var size = new Vector2(width * _cellSize, height * _cellSize);
        _trayRect.sizeDelta = size + _trayPadding * 2f;
        _trayRect.anchoredPosition = _origin;
        _trayRect.SetAsFirstSibling();
        return;
      }

      if (_traySprite != null) {
        if (width <= 0 || height <= 0) {
          _traySprite.gameObject.SetActive(false);
          return;
        }

        _traySprite.gameObject.SetActive(true);
        var boardSize = new Vector2(width * _cellSize, height * _cellSize);
        var size = boardSize + _trayPadding * 2f;
        var sprite = _traySprite.sprite;
        if (sprite != null) {
          Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
          if (spriteSize.x > 0f && spriteSize.y > 0f) {
            var scale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
            _traySprite.transform.localScale = scale;
          }
        }

        var bottomLeft = _origin - _trayPadding;
        var center = bottomLeft + size * 0.5f;
        _traySprite.transform.localPosition = new Vector3(center.x, center.y, 0f);
      }
    }
  }
}
