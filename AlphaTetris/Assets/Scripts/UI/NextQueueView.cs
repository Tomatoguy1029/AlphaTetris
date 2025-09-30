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

    private readonly List<MiniBoard> _miniBoards = new();

    private void Awake() {
      if (_container == null) {
        _container = (RectTransform)transform;
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
        if (i < count && queue != null) {
          _miniBoards[i].Root.gameObject.SetActive(true);
          RenderMiniBoard(_miniBoards[i], queue[i]);
        } else {
          _miniBoards[i].Root.gameObject.SetActive(false);
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
        _miniBoards.Add(CreateMiniBoard(_miniBoards.Count));
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

      var cellSize = _cellPrefab != null ? _cellPrefab.rectTransform.sizeDelta : Vector2.zero;
      if (cellSize == Vector2.zero) {
        cellSize = new Vector2(_cellPrefab.rectTransform.rect.width, _cellPrefab.rectTransform.rect.height);
      }
      if (cellSize == Vector2.zero) {
        cellSize = new Vector2(32f, 32f);
      }

      var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
      grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
      grid.startAxis = GridLayoutGroup.Axis.Horizontal;
      grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
      grid.constraintCount = 4;
      grid.childAlignment = TextAnchor.UpperLeft;
      grid.cellSize = cellSize;
      grid.spacing = Vector2.zero;

      rect.sizeDelta = cellSize * 4f;

      var miniBoard = new MiniBoard(rect);
      for (var y = 0; y < 4; y++) {
        for (var x = 0; x < 4; x++) {
          var instance = Instantiate(_cellPrefab, rect);
          var childRect = instance.rectTransform;
          childRect.anchorMin = childRect.anchorMax = new Vector2(0.5f, 0.5f);
          childRect.pivot = new Vector2(0.5f, 0.5f);
          childRect.anchoredPosition = Vector2.zero;
          childRect.localScale = Vector3.one;
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

      for (var i = _container.childCount - 1; i >= 0; i--) {
        var child = _container.GetChild(i);
        if (Application.isPlaying) {
          Destroy(child.gameObject);
        } else {
          DestroyImmediate(child.gameObject);
        }
      }
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

    private sealed class MiniBoard {
      public MiniBoard(RectTransform root) {
        Root = root;
      }

      public readonly RectTransform Root;
      public readonly List<Image> Cells = new();
    }
  }
}
