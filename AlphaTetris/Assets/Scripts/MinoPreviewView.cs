using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public class MinoPreviewView : MonoBehaviour {
    public enum PreviewMode {
      Hold,
      NextQueue
    }

    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private Transform _previewRoot;
    [SerializeField] private PreviewMode _mode = PreviewMode.NextQueue;
    [SerializeField, Min(1)] private int _maxNextToDisplay = 3;
    [SerializeField, Min(0.1f)] private float _cellSize = 0.5f;
    [SerializeField] private Vector3 _originOffset = Vector3.zero;
    [SerializeField] private float _pieceSpacingInCells = 4.5f;

    private readonly List<GameObject> _spawnedCells = new List<GameObject>();
    private readonly List<Vector2Int> _cellBuffer = new List<Vector2Int>(8);
    private bool _hasHoldCached;
    private Tetrimino.TetriminoType _cachedHoldType;
    private readonly List<Tetrimino.TetriminoType?> _cachedQueueTypes = new List<Tetrimino.TetriminoType?>();
    private readonly List<Tetrimino.TetriminoType?> _queueTypeBuffer = new List<Tetrimino.TetriminoType?>();

    private void Awake() {
      if (_previewRoot == null) {
        _previewRoot = transform;
      }
    }

    private void OnEnable() {
      if (_gameLogic != null) {
        _gameLogic.OnBoardUpdated += HandleBoardUpdated;
        _gameLogic.OnGameOver += HandleBoardUpdated;
      }

      Refresh();
    }

    private void OnDisable() {
      if (_gameLogic != null) {
        _gameLogic.OnBoardUpdated -= HandleBoardUpdated;
        _gameLogic.OnGameOver -= HandleBoardUpdated;
      }

      ClearPreview();
      _hasHoldCached = false;
      _cachedQueueTypes.Clear();
    }

    private void HandleBoardUpdated() {
      Refresh();
    }

    private void Refresh() {
      if (_gameLogic == null || _cellPrefab == null || _previewRoot == null) {
        return;
      }

      switch (_mode) {
        case PreviewMode.Hold:
          RenderHold();
          break;
        case PreviewMode.NextQueue:
          RenderNext();
          break;
      }
    }

    private void RenderHold() {
      var hold = _gameLogic.HoldMino;
      if (hold == null) {
        if (_hasHoldCached) {
          ClearPreview();
          _hasHoldCached = false;
        }
        return;
      }

      if (_hasHoldCached && _cachedHoldType == hold.Type) {
        return;
      }

      ClearPreview();
      BuildPiece(hold, _originOffset);
      _cachedHoldType = hold.Type;
      _hasHoldCached = true;
    }

    private void RenderNext() {
      var preview = _gameLogic.NextPreview;
      if (preview == null || preview.Count == 0) {
        if (_cachedQueueTypes.Count > 0) {
          ClearPreview();
          _cachedQueueTypes.Clear();
        }
        return;
      }

      int count = Mathf.Min(_maxNextToDisplay, preview.Count);
      _queueTypeBuffer.Clear();
      for (int i = 0; i < count; i++) {
        var mino = preview[i];
        _queueTypeBuffer.Add(mino != null ? mino.Type : (Tetrimino.TetriminoType?)null);
      }

      if (AreTypeListsEqual(_queueTypeBuffer, _cachedQueueTypes)) {
        return;
      }

      ClearPreview();
      _cachedQueueTypes.Clear();
      _cachedQueueTypes.AddRange(_queueTypeBuffer);

      for (int i = 0; i < count; i++) {
        var mino = preview[i];
        if (mino == null) {
          continue;
        }

        Vector3 offset = _originOffset + new Vector3(0f, -_pieceSpacingInCells * i * _cellSize, 0f);
        BuildPiece(mino, offset);
      }
    }

    private void BuildPiece(Tetrimino mino, Vector3 origin) {
      if (mino == null) {
        return;
      }

      var shape = Tetrimino.GetPreviewShape(mino.Type);
      _cellBuffer.Clear();
      foreach (var cell in Tetrimino.GetCells(shape)) {
        _cellBuffer.Add(cell);
      }

      if (_cellBuffer.Count == 0) {
        return;
      }

      int minX = int.MaxValue;
      int maxX = int.MinValue;
      int minY = int.MaxValue;
      int maxY = int.MinValue;

      for (int i = 0; i < _cellBuffer.Count; i++) {
        var cell = _cellBuffer[i];
        if (cell.x < minX) minX = cell.x;
        if (cell.x > maxX) maxX = cell.x;
        if (cell.y < minY) minY = cell.y;
        if (cell.y > maxY) maxY = cell.y;
      }

      Vector2 center = new Vector2((minX + maxX + 1) * 0.5f, (minY + maxY + 1) * 0.5f);

      for (int i = 0; i < _cellBuffer.Count; i++) {
        var cell = _cellBuffer[i];
        var instance = Instantiate(_cellPrefab, _previewRoot);
        var transformCache = instance.transform;
        Vector3 local = new Vector3(cell.x + 0.5f - center.x, cell.y + 0.5f - center.y, 0f) * _cellSize;
        transformCache.localPosition = origin + local;
        transformCache.localRotation = Quaternion.identity;
        transformCache.localScale = Vector3.one * _cellSize;
        _spawnedCells.Add(instance);
      }
    }

    private void ClearPreview() {
      if (_spawnedCells.Count == 0) {
        return;
      }

      for (int i = 0; i < _spawnedCells.Count; i++) {
        var obj = _spawnedCells[i];
        if (obj == null) {
          continue;
        }

        if (Application.isPlaying) {
          Destroy(obj);
        } else {
          DestroyImmediate(obj);
        }
      }

      _spawnedCells.Clear();
    }

    private static bool AreTypeListsEqual(List<Tetrimino.TetriminoType?> a, List<Tetrimino.TetriminoType?> b) {
      if (a.Count != b.Count) {
        return false;
      }

      for (int i = 0; i < a.Count; i++) {
        var left = a[i];
        var right = b[i];
        if (left.HasValue != right.HasValue) {
          return false;
        }

        if (left.HasValue && right.HasValue && left.Value != right.Value) {
          return false;
        }
      }

      return true;
    }
  }
}
