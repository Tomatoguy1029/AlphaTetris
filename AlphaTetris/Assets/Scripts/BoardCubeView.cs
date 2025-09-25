using System.Collections.Generic;
using UnityEngine;

namespace AlphaTetris {
  public class BoardCubeView : MonoBehaviour {
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private Transform boardRoot;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField, Min(0.1f)] private float cellSize = 1f;
    [SerializeField] private Vector3 originOffset = Vector3.zero;
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color activeColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private bool drawBoundsGizmo = true;
    [SerializeField] private bool buildTray = true;
    [SerializeField] private Color trayColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [SerializeField] private Material trayMaterial;
    [SerializeField, Min(0.01f)] private float trayDepth = 0.4f;
    [SerializeField, Min(0.01f)] private float trayThickness = 0.1f;
    [SerializeField, Min(0f)] private float trayTopMargin = 0.5f;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static Material fallbackTrayMaterial;

    private readonly List<Renderer> cells = new List<Renderer>();
    private MaterialPropertyBlock propertyBlock;
    private int cachedWidth;
    private int cachedHeight;
    private Transform trayRoot;
    private readonly List<MeshRenderer> trayRenderers = new List<MeshRenderer>();
    private Material trayRuntimeMaterial;

    private void Awake() {
      if (boardRoot == null) {
        boardRoot = transform;
      }
    }

    private void OnEnable() {
      if (gameLogic == null) {
        Debug.LogWarning("BoardCubeView: GameLogic reference is missing.");
        return;
      }

      BuildGrid();

      // 盤面更新イベントに乗ってセル描画を同期する
      gameLogic.OnBoardUpdated += SyncBoard;
      gameLogic.OnGameOver += SyncBoard;
      SyncBoard();
    }

    private void OnDisable() {
      if (gameLogic == null) {
        return;
      }

      gameLogic.OnBoardUpdated -= SyncBoard;
      gameLogic.OnGameOver -= SyncBoard;
    }

    private void OnDestroy() {
      if (trayRuntimeMaterial != null) {
        if (Application.isPlaying) {
          Destroy(trayRuntimeMaterial);
        } else {
          DestroyImmediate(trayRuntimeMaterial);
        }
        trayRuntimeMaterial = null;
      }
    }

    // ゲームロジックの盤面サイズに合わせてセルを生成し直す
    private void BuildGrid() {
      if (cellPrefab == null) {
        Debug.LogError("BoardCubeView: Cell prefab is not assigned.");
        return;
      }

      if (gameLogic == null) {
        return;
      }

      var board = gameLogic.RenderBoard;
      if (board == null) {
        return;
      }

      int height = board.GetLength(0);
      int width = board.GetLength(1);

      if (width == cachedWidth && height == cachedHeight && cells.Count == width * height) {
        SyncTray(width, height);
        return;
      }

      ClearCells();

      cachedWidth = width;
      cachedHeight = height;

      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          var cell = Instantiate(cellPrefab, boardRoot);
          cell.name = $"Cube_{x}_{y}";
          var transformCache = cell.transform;
          transformCache.localPosition = originOffset + new Vector3(x * cellSize, y * cellSize, 0f);
          transformCache.localRotation = Quaternion.identity;
          transformCache.localScale = Vector3.one * cellSize;

          var renderer = cell.GetComponentInChildren<Renderer>();
          if (renderer == null) {
            Debug.LogWarning($"BoardCubeView: Instantiated cell '{cell.name}' has no Renderer component.");
          }

          cells.Add(renderer);
        }
      }

      SyncTray(width, height);
    }

    // 描画用セルの有効・色を盤面状態から反映する
    private void SyncBoard() {
      if (gameLogic == null) {
        return;
      }

      var board = gameLogic.RenderBoard;
      if (board == null) {
        HideCells();
        ToggleTray(false);
        return;
      }

      int height = board.GetLength(0);
      int width = board.GetLength(1);

      if (width != cachedWidth || height != cachedHeight || cells.Count != width * height) {
        BuildGrid();
        board = gameLogic.RenderBoard;
        if (board == null) {
          return;
        }
        height = board.GetLength(0);
        width = board.GetLength(1);
      }

      SyncTray(width, height);

      if (propertyBlock == null) {
        propertyBlock = new MaterialPropertyBlock();
      }

      int index = 0;
      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          Renderer renderer = null;
          if (index < cells.Count) {
            renderer = cells[index];
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
          propertyBlock.Clear();
          var colorToApply = lockedColor;
          if (value != 1) {
            colorToApply = activeColor;
          }
          propertyBlock.SetColor(ColorId, colorToApply);
          renderer.SetPropertyBlock(propertyBlock);
        }
      }
    }

    // セルとトレイをいったん破棄してクリーンな状態にする
    private void ClearCells() {
      foreach (Transform child in boardRoot) {
        if (trayRoot != null && child == trayRoot) {
          continue;
        }

        if (Application.isPlaying) {
          Destroy(child.gameObject);
        } else {
          DestroyImmediate(child.gameObject);
        }
      }

      cells.Clear();
      cachedWidth = 0;
      cachedHeight = 0;
      ToggleTray(false);
    }

    // セルを非表示にするだけで参照は残す
    private void HideCells() {
      foreach (var renderer in cells) {
        if (renderer != null) {
          renderer.enabled = false;
        }
      }
    }

    // 盤面サイズに合わせてトレイの有無と形状を更新する
    private void SyncTray(int width, int height) {
      if (!buildTray) {
        ToggleTray(false);
        return;
      }

      if (trayRoot == null) {
        trayRoot = new GameObject("BoardTray").transform;
        trayRoot.SetParent(boardRoot, false);
        CreateTray();
      }

      LayoutTray(width, height);
      ApplyTray();
      ToggleTray(true);
    }

    private void CreateTray() {
      trayRenderers.Clear();
      string[] names = { "TrayBottom", "TrayLeft", "TrayRight", "TrayBack" };
      for (int i = 0; i < names.Length; i++) {
        var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = names[i];
        segment.transform.SetParent(trayRoot, false);
        var collider = segment.GetComponent<Collider>();
        if (collider != null) {
          if (Application.isPlaying) {
            Destroy(collider);
          } else {
            DestroyImmediate(collider);
          }
        }
        var renderer = segment.GetComponent<MeshRenderer>();
        trayRenderers.Add(renderer);
      }
    }

    // トレイ各パーツの位置とスケールを計算
    private void LayoutTray(int width, int height) {
      if (trayRoot == null) {
        return;
      }

      float boardWidth = width * cellSize;
      float boardHeight = height * cellSize;
      float wallThickness = trayThickness;
      float depth = trayDepth;
      float extraHeight = trayTopMargin;
      float halfWidth = boardWidth * 0.5f;
      float halfHeight = boardHeight * 0.5f;
      Vector3 center = originOffset + new Vector3((width - 1) * cellSize * 0.5f, (height - 1) * cellSize * 0.5f, 0f);
      float bottomY = center.y - halfHeight;
      float baseZ = depth * 0.5f;

      Transform bottom = trayRoot.GetChild(0);
      bottom.localPosition = new Vector3(center.x, bottomY - wallThickness * 0.5f, baseZ);
      bottom.localScale = new Vector3(boardWidth + wallThickness * 2f, wallThickness, depth);

      float wallHeight = boardHeight + extraHeight;
      float wallCenterY = bottomY + wallHeight * 0.5f;

      Transform left = trayRoot.GetChild(1);
      left.localPosition = new Vector3(center.x - halfWidth - wallThickness * 0.5f, wallCenterY, baseZ);
      left.localScale = new Vector3(wallThickness, wallHeight, depth);

      Transform right = trayRoot.GetChild(2);
      right.localPosition = new Vector3(center.x + halfWidth + wallThickness * 0.5f, wallCenterY, baseZ);
      right.localScale = new Vector3(wallThickness, wallHeight, depth);

      Transform back = trayRoot.GetChild(3);
      back.localPosition = new Vector3(center.x, wallCenterY, depth + wallThickness * 0.5f);
      back.localScale = new Vector3(boardWidth + wallThickness * 2f, wallHeight, wallThickness);
    }

    // 共有マテリアルを用意してトレイに適用
    private void ApplyTray() {
      if (trayRenderers.Count == 0) {
        return;
      }

      var sourceMaterial = trayMaterial != null ? trayMaterial : GetTrayFallback();
      if (trayRuntimeMaterial == null || trayRuntimeMaterial.shader != sourceMaterial.shader) {
        if (trayRuntimeMaterial != null) {
          if (Application.isPlaying) {
            Destroy(trayRuntimeMaterial);
          } else {
            DestroyImmediate(trayRuntimeMaterial);
          }
        }
        trayRuntimeMaterial = new Material(sourceMaterial);
      }

      trayRuntimeMaterial.color = trayColor;

      foreach (var renderer in trayRenderers) {
        if (renderer == null) {
          continue;
        }
        renderer.sharedMaterial = trayRuntimeMaterial;
      }
    }

    private void ToggleTray(bool isActive) {
      if (trayRoot != null) {
        trayRoot.gameObject.SetActive(isActive && buildTray);
      }
    }

    private static Material GetTrayFallback() {
      if (fallbackTrayMaterial == null) {
        const string shaderName = "Universal Render Pipeline/Lit";
        var shader = Shader.Find(shaderName);
        if (shader == null) {
          fallbackTrayMaterial = new Material(Shader.Find("Standard"));
        } else {
          fallbackTrayMaterial = new Material(shader);
        }
      }

      return fallbackTrayMaterial;
    }

// トレイの位置がScence画面で表示
#if UNITY_EDITOR
    private void OnDrawGizmos() {
      if (!drawBoundsGizmo) {
        return;
      }

      if (!TryGetSize(out int width, out int height)) {
        return;
      }

      Gizmos.color = Color.cyan;
      var size = new Vector3(width * cellSize, height * cellSize, cellSize);
      var offset = originOffset + new Vector3((width - 1) * cellSize * 0.5f, (height - 1) * cellSize * 0.5f, 0f);
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.DrawWireCube(offset, size);
    }

    private bool TryGetSize(out int width, out int height) {
      width = 0;
      height = 0;

      if (cachedWidth > 0 && cachedHeight > 0) {
        width = cachedWidth;
        height = cachedHeight;
        return true;
      }

      if (gameLogic != null) {
        width = gameLogic.width;
        height = gameLogic.height;
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
