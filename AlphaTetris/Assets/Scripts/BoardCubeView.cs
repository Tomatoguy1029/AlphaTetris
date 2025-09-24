using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public class BoardCubeView : MonoBehaviour {
    [SerializeField] private GameLogic _gameLogic;
    [SerializeField] private Transform _boardRoot;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField, Min(0.1f)] private float _cellSize = 1f;
    [SerializeField] private Vector3 _originOffset = Vector3.zero;
    [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color _activeColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private bool _drawBoundsGizmo = true;
    [SerializeField] private bool _buildTray = true;
    [SerializeField] private Color _trayColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Material _trayMaterial;
    [SerializeField, Min(0.01f)] private float _trayDepth = 0.4f;
    [SerializeField, Min(0.01f)] private float _trayThickness = 0.1f;
    [SerializeField, Min(0f)] private float _trayTopMargin = 0.5f;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static Material _fallbackTrayMaterial;

    private readonly List<Renderer> _cells = new List<Renderer>();
    private MaterialPropertyBlock _propertyBlock;
    private int _cachedWidth;
    private int _cachedHeight;
    private Transform _trayRoot;
    private readonly List<MeshRenderer> _trayRenderers = new List<MeshRenderer>();
    private Material _trayRuntimeMaterial;

    private void Awake() {
      if (_boardRoot == null) {
        _boardRoot = transform;
      }
    }

    private void OnEnable() {
      if (_gameLogic == null) {
        Debug.LogWarning("BoardCubeView: GameLogic reference is missing.");
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

    private void OnDestroy() {
      if (_trayRuntimeMaterial != null) {
        if (Application.isPlaying) {
          Destroy(_trayRuntimeMaterial);
        } else {
          DestroyImmediate(_trayRuntimeMaterial);
        }
        _trayRuntimeMaterial = null;
      }
    }

    private void BuildGridIfNeeded() {
      if (_cellPrefab == null) {
        Debug.LogError("BoardCubeView: Cell prefab is not assigned.");
        return;
      }

      if (_gameLogic == null) {
        return;
      }

      var board = _gameLogic.RenderBoard;
      if (board == null) {
        return;
      }

      int height = board.GetLength(0);
      int width = board.GetLength(1);

      if (width == _cachedWidth && height == _cachedHeight && _cells.Count == width * height) {
        EnsureTray(width, height);
        return;
      }

      ClearExistingCells();

      _cachedWidth = width;
      _cachedHeight = height;

      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          var cell = Instantiate(_cellPrefab, _boardRoot);
          cell.name = $"Cube_{x}_{y}";
          var transformCache = cell.transform;
          transformCache.localPosition = _originOffset + new Vector3(x * _cellSize, y * _cellSize, 0f);
          transformCache.localRotation = Quaternion.identity;
          transformCache.localScale = Vector3.one * _cellSize;

          var renderer = cell.GetComponentInChildren<Renderer>();
          if (renderer == null) {
            Debug.LogWarning($"BoardCubeView: Instantiated cell '{cell.name}' has no Renderer component.");
          }

          _cells.Add(renderer);
        }
      }

      EnsureTray(width, height);
    }

    private void HandleBoardUpdated() {
      if (_gameLogic == null) {
        return;
      }

      var board = _gameLogic.RenderBoard;
      if (board == null) {
        DisableAllCells();
        SetTrayActive(false);
        return;
      }

      int height = board.GetLength(0);
      int width = board.GetLength(1);

      if (width != _cachedWidth || height != _cachedHeight || _cells.Count != width * height) {
        BuildGridIfNeeded();
        board = _gameLogic.RenderBoard;
        if (board == null) {
          return;
        }
        height = board.GetLength(0);
        width = board.GetLength(1);
      }

      EnsureTray(width, height);

      if (_propertyBlock == null) {
        _propertyBlock = new MaterialPropertyBlock();
      }

      int index = 0;
      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          Renderer renderer = null;
          if (index < _cells.Count) {
            renderer = _cells[index];
          }
          index++;

          if (renderer == null) {
            continue;
          }

          var value = board[y, x];
          if (value == 0) {
            renderer.enabled = false;
            continue;
          }

          renderer.enabled = true;
          _propertyBlock.Clear();
          var colorToApply = _lockedColor;
          if (value != 1) {
            colorToApply = _activeColor;
          }
          _propertyBlock.SetColor(ColorId, colorToApply);
          renderer.SetPropertyBlock(_propertyBlock);
        }
      }
    }

    private void ClearExistingCells() {
      foreach (Transform child in _boardRoot) {
        if (_trayRoot != null && child == _trayRoot) {
          continue;
        }

        if (Application.isPlaying) {
          Destroy(child.gameObject);
        } else {
          DestroyImmediate(child.gameObject);
        }
      }

      _cells.Clear();
      _cachedWidth = 0;
      _cachedHeight = 0;
      SetTrayActive(false);
    }

    private void DisableAllCells() {
      foreach (var renderer in _cells) {
        if (renderer != null) {
          renderer.enabled = false;
        }
      }
    }

    private void EnsureTray(int width, int height) {
      if (!_buildTray) {
        SetTrayActive(false);
        return;
      }

      if (_trayRoot == null) {
        _trayRoot = new GameObject("BoardTray").transform;
        _trayRoot.SetParent(_boardRoot, false);
        CreateTraySegments();
      }

      UpdateTray(width, height);
      ApplyTrayMaterial();
      SetTrayActive(true);
    }

    private void CreateTraySegments() {
      _trayRenderers.Clear();
      string[] names = { "TrayBottom", "TrayLeft", "TrayRight", "TrayBack" };
      for (int i = 0; i < names.Length; i++) {
        var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = names[i];
        segment.transform.SetParent(_trayRoot, false);
        var collider = segment.GetComponent<Collider>();
        if (collider != null) {
          if (Application.isPlaying) {
            Destroy(collider);
          } else {
            DestroyImmediate(collider);
          }
        }
        var renderer = segment.GetComponent<MeshRenderer>();
        _trayRenderers.Add(renderer);
      }
    }

    private void UpdateTray(int width, int height) {
      if (_trayRoot == null) {
        return;
      }

      float boardWidth = width * _cellSize;
      float boardHeight = height * _cellSize;
      float wallThickness = _trayThickness;
      float depth = _trayDepth;
      float extraHeight = _trayTopMargin;
      float halfWidth = boardWidth * 0.5f;
      float halfHeight = boardHeight * 0.5f;
      Vector3 center = _originOffset + new Vector3((width - 1) * _cellSize * 0.5f, (height - 1) * _cellSize * 0.5f, 0f);
      float bottomY = center.y - halfHeight;
      float baseZ = depth * 0.5f;

      Transform bottom = _trayRoot.GetChild(0);
      bottom.localPosition = new Vector3(center.x, bottomY - wallThickness * 0.5f, baseZ);
      bottom.localScale = new Vector3(boardWidth + wallThickness * 2f, wallThickness, depth);

      float wallHeight = boardHeight + extraHeight;
      float wallCenterY = bottomY + wallHeight * 0.5f;

      Transform left = _trayRoot.GetChild(1);
      left.localPosition = new Vector3(center.x - halfWidth - wallThickness * 0.5f, wallCenterY, baseZ);
      left.localScale = new Vector3(wallThickness, wallHeight, depth);

      Transform right = _trayRoot.GetChild(2);
      right.localPosition = new Vector3(center.x + halfWidth + wallThickness * 0.5f, wallCenterY, baseZ);
      right.localScale = new Vector3(wallThickness, wallHeight, depth);

      Transform back = _trayRoot.GetChild(3);
      back.localPosition = new Vector3(center.x, wallCenterY, depth + wallThickness * 0.5f);
      back.localScale = new Vector3(boardWidth + wallThickness * 2f, wallHeight, wallThickness);
    }

    private void ApplyTrayMaterial() {
      if (_trayRenderers.Count == 0) {
        return;
      }

      var sourceMaterial = _trayMaterial != null ? _trayMaterial : GetFallbackTrayMaterial();
      if (_trayRuntimeMaterial == null || _trayRuntimeMaterial.shader != sourceMaterial.shader) {
        if (_trayRuntimeMaterial != null) {
          if (Application.isPlaying) {
            Destroy(_trayRuntimeMaterial);
          } else {
            DestroyImmediate(_trayRuntimeMaterial);
          }
        }
        _trayRuntimeMaterial = new Material(sourceMaterial);
      }

      _trayRuntimeMaterial.color = _trayColor;

      foreach (var renderer in _trayRenderers) {
        if (renderer == null) {
          continue;
        }
        renderer.sharedMaterial = _trayRuntimeMaterial;
      }
    }

    private void SetTrayActive(bool isActive) {
      if (_trayRoot != null) {
        _trayRoot.gameObject.SetActive(isActive && _buildTray);
      }
    }

    private static Material GetFallbackTrayMaterial() {
      if (_fallbackTrayMaterial == null) {
        const string shaderName = "Universal Render Pipeline/Lit";
        var shader = Shader.Find(shaderName);
        if (shader == null) {
          _fallbackTrayMaterial = new Material(Shader.Find("Standard"));
        } else {
          _fallbackTrayMaterial = new Material(shader);
        }
      }

      return _fallbackTrayMaterial;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
      if (!_drawBoundsGizmo) {
        return;
      }

      if (!TryGetBoardSize(out int width, out int height)) {
        return;
      }

      Gizmos.color = Color.cyan;
      var size = new Vector3(width * _cellSize, height * _cellSize, _cellSize);
      var offset = _originOffset + new Vector3((width - 1) * _cellSize * 0.5f, (height - 1) * _cellSize * 0.5f, 0f);
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.DrawWireCube(offset, size);
    }

    private bool TryGetBoardSize(out int width, out int height) {
      width = 0;
      height = 0;

      if (_cachedWidth > 0 && _cachedHeight > 0) {
        width = _cachedWidth;
        height = _cachedHeight;
        return true;
      }

      if (_gameLogic != null) {
        width = _gameLogic.width;
        height = _gameLogic.height;
        if (width > 0 && height > 0) {
          return true;
        }
      }

      const int defaultWidth = 10;
      const int defaultHeight = 20;
      width = defaultWidth;
      height = defaultHeight;
      return true;
    }
#endif
  }
}
