using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaTetris {
  [RequireComponent(typeof(GridLayoutGroup))]
  public class BoardGridView : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private RectTransform _container;
    [SerializeField] private Image _cellPrefab;
    [SerializeField] private Color _emptyColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color _lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color _activeColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    private readonly List<Image> _cells = new List<Image>();
    private int _cachedWidth;
    private int _cachedHeight;

    private void Awake() {
      if (_container == null) {
        _container = (RectTransform)transform;
      }
    }

    private void OnEnable() {
      if (_gameLogic == null) {
        Debug.LogWarning("BoardGridView: GameLogic reference is missing.");
        return;
      }

      BuildGridIfNeeded();

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

    private void BuildGridIfNeeded() {
      if (_cellPrefab == null) {
        Debug.LogError("BoardGridView: Cell prefab is not assigned.");
        return;
      }

      var board = _gameLogic.RenderBoard;
      if (board == null) {
        return;
      }

      int height = board.GetLength(0);
      int width = board.GetLength(1);

      if (width == _cachedWidth && height == _cachedHeight && _cells.Count == width * height) {
        return;
      }

      foreach (Transform child in _container) {
        if (Application.isPlaying) {
          Destroy(child.gameObject);
        } else {
          DestroyImmediate(child.gameObject);
        }
      }

      _cells.Clear();
      _cachedWidth = width;
      _cachedHeight = height;

      var grid = GetComponent<GridLayoutGroup>();
      grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
      grid.constraintCount = width;

      for (int y = height - 1; y >= 0; y--) {
        for (int x = 0; x < width; x++) {
          var cell = Instantiate(_cellPrefab, _container);
          cell.name = $"Cell_{x}_{y}";
          _cells.Add(cell);
        }
      }
    }

    private void HandleBoardUpdated() {
      var board = _gameLogic.RenderBoard;
      if (board == null || _cells.Count == 0) {
        return;
      }

      int height = board.GetLength(0);
      int width = board.GetLength(1);
      if (width != _cachedWidth || height != _cachedHeight) {
        BuildGridIfNeeded();
        board = _gameLogic.RenderBoard;
        if (board == null) {
          return;
        }
      }

      int index = 0;
      for (int y = height - 1; y >= 0; y--) {
        for (int x = 0; x < width; x++) {
          var value = board[y, x];
          var cell = _cells[index++];
          cell.color = value switch {
            1 => _lockedColor,
            2 => _activeColor,
            _ => _emptyColor
          };
        }
      }
    }
  }
}
