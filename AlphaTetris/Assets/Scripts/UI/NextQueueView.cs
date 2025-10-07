using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaTetris {
  public class NextQueueView : MonoBehaviour {
    [SerializeField] private RectTransform _container;
    [SerializeField] private Image _cellPrefab;
    [SerializeField] private int _previewCount = 1;
    [SerializeField] private Color _emptyColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color _minoColor = Color.white;
    [SerializeField] private Vector2 _previewOffset = Vector2.zero;
    [SerializeField] private bool _arrangeHorizontally = false;
    [SerializeField] private float _previewSpacing = 8f;
    [SerializeField] private bool _autoDisableContainerLayoutGroup = true;

    private LayoutGroup _cachedLayoutGroup;

    private readonly List<MiniBoard> _miniBoards = new();

    private void Awake() {
      if (_container == null) {
        _container = (RectTransform)transform;
      }

      if (_autoDisableContainerLayoutGroup && _container != null) {
        _cachedLayoutGroup = _container.GetComponent<LayoutGroup>();
        if (_cachedLayoutGroup != null && _cachedLayoutGroup.enabled) {
          _cachedLayoutGroup.enabled = false;
        }
      }

      if (_cellPrefab != null) {
        _cellPrefab.gameObject.SetActive(false);
      }

      ClearExistingPreviews();
    }

    public void Render(IReadOnlyList<Tetrimino> queue) {
      var count = Mathf.Min(_previewCount, queue?.Count ?? 0);
      EnsureMiniBoards(count);

      for (var i = 0; i < _miniBoards.Count; i++) {
        var board = _miniBoards[i];
        LayoutMiniBoard(board);
        PositionMiniBoard(board, i);

        if (i < count && queue != null) {
          board.Root.gameObject.SetActive(true);
          RenderMiniBoard(board, queue[i]);
        } else {
          board.Root.gameObject.SetActive(false);
        }
      }
    }

    private void EnsureMiniBoards(int required) {
      if (_container == null) {
        _container = (RectTransform)transform;
      }

      if (_cellPrefab == null || _container == null) {
        Debug.LogError("NextQueueView: Cell prefab or container is missing.");
        return;
      }

      while (_miniBoards.Count < required) {
        var board = CreateMiniBoard(_miniBoards.Count);
        _miniBoards.Add(board);
        LayoutMiniBoard(board);
        PositionMiniBoard(board, _miniBoards.Count - 1);
      }

      for (var i = _miniBoards.Count - 1; i >= required; i--) {
        var board = _miniBoards[i];
        if (board != null) {
          if (Application.isPlaying) {
            Destroy(board.Root.gameObject);
          } else {
            DestroyImmediate(board.Root.gameObject);
          }
        }

        _miniBoards.RemoveAt(i);
      }
    }

    private MiniBoard CreateMiniBoard(int index) {
      var owner = new GameObject($"Preview_{index}", typeof(RectTransform));
      var rect = owner.GetComponent<RectTransform>();
      rect.SetParent(_container, false);
      rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
      rect.anchoredPosition = _previewOffset;
      rect.localScale = Vector3.one;

      var miniBoard = new MiniBoard(rect) {
        CellSize = ResolveCellSize()
      };

      for (var y = 0; y < 4; y++) {
        for (var x = 0; x < 4; x++) {
          var instance = Instantiate(_cellPrefab, rect);
          var childRect = instance.rectTransform;
          childRect.anchorMin = childRect.anchorMax = childRect.pivot = new Vector2(0.5f, 0.5f);
          childRect.localScale = Vector3.one;
          childRect.sizeDelta = miniBoard.CellSize;
          instance.gameObject.SetActive(true);
          instance.raycastTarget = false;
          miniBoard.Cells.Add(instance);
        }
      }

      return miniBoard;
    }

    private void ClearExistingPreviews() {
      _miniBoards.Clear();

      if (_container == null) {
        return;
      }

      var template = _cellPrefab != null ? _cellPrefab.transform : null;
      for (var i = _container.childCount - 1; i >= 0; i--) {
        var child = _container.GetChild(i);
        if (child == template) {
          continue;
        }

        if (Application.isPlaying) {
          Destroy(child.gameObject);
        } else {
          DestroyImmediate(child.gameObject);
        }
      }
    }

    private void LayoutMiniBoard(MiniBoard board) {
      if (board == null || board.Root == null) {
        return;
      }

      var cellSize = board.CellSize;
      if (cellSize == Vector2.zero) {
        cellSize = ResolveCellSize();
        board.CellSize = cellSize;
      }

      var size = cellSize * 4f;
      board.Root.sizeDelta = size;

      var startX = -size.x * 0.5f + cellSize.x * 0.5f;
      var startY = size.y * 0.5f - cellSize.y * 0.5f;
      var index = 0;

      for (var y = 0; y < 4; y++) {
        for (var x = 0; x < 4; x++) {
          if (index >= board.Cells.Count) {
            return;
          }

          var rect = board.Cells[index++].rectTransform;
          rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
          rect.anchoredPosition = new Vector2(startX + x * cellSize.x, startY - y * cellSize.y);
          rect.sizeDelta = cellSize;
        }
      }
    }

    private void PositionMiniBoard(MiniBoard board, int index) {
      if (board?.Root == null) {
        return;
      }

      var size = board.Root.sizeDelta;
      if (size == Vector2.zero) {
        LayoutMiniBoard(board);
        size = board.Root.sizeDelta;
      }

      var step = _arrangeHorizontally
        ? new Vector2(size.x + _previewSpacing, 0f)
        : new Vector2(0f, -(size.y + _previewSpacing));

      board.Root.anchoredPosition = _previewOffset + step * index;
      board.Root.SetSiblingIndex(index);
    }

    private void RenderMiniBoard(MiniBoard board, Tetrimino tetrimino) {
      var shape = tetrimino.Shape;
      var color = _minoColor;
      var index = 0;
      for (var y = 0; y < shape.GetLength(0); y++) {
        for (var x = 0; x < shape.GetLength(1); x++) {
          var image = board.Cells[index++];
          image.color = shape[y, x] == 0 ? _emptyColor : color;
        }
      }
    }

    private Vector2 ResolveCellSize() {
      if (_cellPrefab == null) {
        return new Vector2(32f, 32f);
      }

      var rect = _cellPrefab.rectTransform;
      var size = rect.rect.size;
      if (size == Vector2.zero) {
        size = rect.sizeDelta;
      }

      if (size == Vector2.zero) {
        size = new Vector2(32f, 32f);
      }

      return size;
    }

#if UNITY_EDITOR
    private void OnValidate() {
      if (_autoDisableContainerLayoutGroup) {
        if (_container == null) {
          _container = (RectTransform)transform;
        }

        var layout = _container != null ? _container.GetComponent<LayoutGroup>() : null;
        if (layout != null && layout.enabled) {
          layout.enabled = false;
        }
      }

      for (var i = 0; i < _miniBoards.Count; i++) {
        LayoutMiniBoard(_miniBoards[i]);
        PositionMiniBoard(_miniBoards[i], i);
      }
    }
#endif

    private sealed class MiniBoard {
      public MiniBoard(RectTransform root) {
        Root = root;
      }

      public readonly RectTransform Root;
      public readonly List<Image> Cells = new();
      public Vector2 CellSize;
    }
  }
}

