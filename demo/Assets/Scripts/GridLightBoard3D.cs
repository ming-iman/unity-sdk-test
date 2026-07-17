using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class GridCellClickable : MonoBehaviour
{
    public GridPos Pos;
}

public class GridLightBoard3D : MonoBehaviour
{
  public event Action<GridPos> CellClicked;

  private const float CellSize = 1f;
  private static readonly Vector3[] BeamRotations =
  {
    new Vector3(0f, 90f, 0f),
    new Vector3(0f, -90f, 0f),
    new Vector3(0f, 0f, 0f),
    new Vector3(0f, 180f, 0f),
  };

  private Transform _root;
  private Camera _camera;
  private bool _mobileLayout;
  private GridLevelData _level;
  private bool _inputEnabled = true;
  private Material _floorUnlitMat;
  private Material _floorLitMat;
  private Material _floorBaseMat;
  private Material _obstacleMat;
  private Material _bulbMat;
  private Material _bulbConflictMat;
  private Rect _gameplayViewport = new Rect(0f, 0f, 1f, 1f);
  private bool _gameplayViewportSet;

  private readonly Dictionary<GridPos, Renderer> _floorRenderers = new Dictionary<GridPos, Renderer>();
  private readonly Dictionary<GridPos, GameObject> _obstacleObjects = new Dictionary<GridPos, GameObject>();
  private readonly Dictionary<GridPos, LightFixture> _lightFixtures = new Dictionary<GridPos, LightFixture>();

  private class LightFixture
  {
    public GameObject Root;
    public Renderer BulbRenderer;
    public Light PointLight;
    public readonly Light[] Beams = new Light[4];
  }

  public void Initialize(Camera camera, bool mobileLayout)
  {
    _camera = camera;
    _mobileLayout = mobileLayout;
    if (_camera != null)
      _camera.clearFlags = CameraClearFlags.SolidColor;
    if (mobileLayout && !EnhancedTouchSupport.enabled)
      EnhancedTouchSupport.Enable();
    CreateMaterials();
  }

  public void SetBoardVisible(bool visible)
  {
    if (_root != null) _root.gameObject.SetActive(visible);
  }

  public void SetGameplayViewport(Rect normalizedRect)
  {
    _gameplayViewport = normalizedRect;
    _gameplayViewportSet = true;
    if (_camera != null && _level != null)
    {
      var (minX, maxX, minY, maxY) = GridLightLogic.Bounds(_level.Cells);
      FitCamera(minX, maxX, minY, maxY);
    }
  }

  public void SetViewportMode(bool gameplay)
  {
    if (_camera == null) return;
    _camera.clearFlags = CameraClearFlags.SolidColor;
    if (gameplay)
    {
      ApplyGameplayCameraRect();
      _camera.backgroundColor = GridLightTheme.BgGameplay;
    }
    else
    {
      _camera.orthographic = false;
      _camera.rect = new Rect(0f, 0f, 1f, 1f);
      _camera.backgroundColor = GridLightTheme.BgWarm;
    }
  }

  private void ApplyGameplayCameraRect()
  {
    if (_gameplayViewportSet)
      _camera.rect = _gameplayViewport;
    else if (_mobileLayout)
      _camera.rect = new Rect(0f, 0.30f, 1f, 0.52f);
    else
      _camera.rect = new Rect(0f, 0f, 0.74f, 0.90f);
  }

  public void SetInputEnabled(bool enabled) => _inputEnabled = enabled;

  public void BuildLevel(GridLevelData level)
  {
    _level = level;
    ClearBoard();
    if (level == null || level.Cells == null || level.Cells.Count == 0) return;

    EnsureRoot();
    _root.gameObject.SetActive(true);

    var (minX, maxX, minY, maxY) = GridLightLogic.Bounds(level.Cells);
    var centerX = (minX + maxX) * 0.5f;
    var centerZ = (minY + maxY) * 0.5f;
    _root.localPosition = new Vector3(-centerX * CellSize, 0f, -centerZ * CellSize);

    CreateBasePlate(minX, maxX, minY, maxY);

    foreach (var pos in level.Cells)
    {
      var world = GridToWorld(pos);
      if (level.Obstacles.Contains(pos))
      {
        _obstacleObjects[pos] = CreateObstacle(world, pos);
        continue;
      }

      var floor = CreateFloor(world, pos);
      _floorRenderers[pos] = floor;
    }

    FitCamera(minX, maxX, minY, maxY);
  }

  public void UpdateVisuals(HashSet<GridPos> playerLights, HashSet<GridPos> conflicts)
  {
    if (_level == null) return;

    var lit = GridLightLogic.GetIlluminated(playerLights, _level.Cells, _level.Obstacles);

    foreach (var kv in _floorRenderers)
    {
      kv.Value.sharedMaterial = lit.Contains(kv.Key) ? _floorLitMat : _floorUnlitMat;
    }

    var toRemove = new List<GridPos>();
    foreach (var kv in _lightFixtures)
    {
      if (!playerLights.Contains(kv.Key)) toRemove.Add(kv.Key);
    }
    foreach (var pos in toRemove)
    {
      Destroy(_lightFixtures[pos].Root);
      _lightFixtures.Remove(pos);
    }

    foreach (var pos in playerLights)
    {
      if (_lightFixtures.ContainsKey(pos)) UpdateFixture(_lightFixtures[pos], pos, conflicts.Contains(pos));
      else _lightFixtures[pos] = CreateFixture(pos, conflicts.Contains(pos));
    }
  }

  private void Update()
  {
    if (!_inputEnabled || _camera == null || _level == null) return;
    if (!TryGetPointerPress(out var screenPos, out var pointerId)) return;
    if (IsPointerOverUi(screenPos, pointerId)) return;
    if (!IsInsideBoardViewport(screenPos)) return;

    var ray = _camera.ScreenPointToRay(screenPos);
    if (!Physics.Raycast(ray, out var hit, 200f)) return;

    var clickable = hit.collider.GetComponent<GridCellClickable>();
    if (clickable == null) return;
    if (_level.Obstacles.Contains(clickable.Pos)) return;

    CellClicked?.Invoke(clickable.Pos);
  }

  private bool IsInsideBoardViewport(Vector2 screenPos)
  {
    var rect = _camera.pixelRect;
    return screenPos.x >= rect.xMin && screenPos.x <= rect.xMax
        && screenPos.y >= rect.yMin && screenPos.y <= rect.yMax;
  }

  private static bool TryGetPointerPress(out Vector2 screenPos, out int pointerId)
  {
    screenPos = default;
    pointerId = -1;

    if (EnhancedTouchSupport.enabled)
    {
      foreach (var touch in Touch.activeTouches)
      {
        if (!touch.began) continue;
        screenPos = touch.screenPosition;
        pointerId = touch.touchId;
        return true;
      }
    }

    if (Touchscreen.current != null)
    {
      var touch = Touchscreen.current.primaryTouch;
      if (touch.press.wasPressedThisFrame)
      {
        screenPos = touch.position.ReadValue();
        pointerId = touch.touchId.ReadValue();
        return true;
      }
    }

    if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
    {
      screenPos = Pointer.current.position.ReadValue();
      pointerId = Pointer.current.device.deviceId;
      return true;
    }

    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    {
      screenPos = Mouse.current.position.ReadValue();
      pointerId = -1;
      return true;
    }

    return false;
  }

  private static readonly List<RaycastResult> UiRaycastBuffer = new List<RaycastResult>();

  private static bool IsPointerOverUi(Vector2 screenPos, int pointerId)
  {
    if (EventSystem.current == null) return false;

    var data = new PointerEventData(EventSystem.current)
    {
      position = screenPos,
      pointerId = pointerId,
    };
    UiRaycastBuffer.Clear();
    EventSystem.current.RaycastAll(data, UiRaycastBuffer);
    foreach (var result in UiRaycastBuffer)
    {
      if (result.gameObject.GetComponentInParent<Button>() != null) return true;
      var image = result.gameObject.GetComponent<Image>();
      if (image != null && image.raycastTarget) return true;
    }

    return false;
  }

  private void ClearBoard()
  {
    _floorRenderers.Clear();
    _obstacleObjects.Clear();
    foreach (var fixture in _lightFixtures.Values)
    {
      if (fixture.Root != null) Destroy(fixture.Root);
    }
    _lightFixtures.Clear();

    if (_root == null) return;
    Destroy(_root.gameObject);
    _root = null;
  }

  private void EnsureRoot()
  {
    if (_root != null) return;
    _root = new GameObject("BoardRoot").transform;
    _root.SetParent(transform, false);
  }

  private Vector3 GridToWorld(GridPos pos)
  {
    return new Vector3(pos.X * CellSize, 0f, pos.Y * CellSize);
  }

  private void CreateBasePlate(int minX, int maxX, int minY, int maxY)
  {
    var width = (maxX - minX + 3) * CellSize;
    var depth = (maxY - minY + 3) * CellSize;
    var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
    plate.name = "BasePlate";
    plate.transform.SetParent(_root, false);
    plate.transform.localScale = new Vector3(width, 0.12f, depth);
    plate.transform.localPosition = new Vector3((minX + maxX) * 0.5f * CellSize, -0.08f, (minY + maxY) * 0.5f * CellSize);
    plate.GetComponent<Renderer>().sharedMaterial = _floorBaseMat;
    Destroy(plate.GetComponent<Collider>());
  }

  private Renderer CreateFloor(Vector3 world, GridPos pos)
  {
    var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
    floor.name = $"Floor_{pos.X}_{pos.Y}";
    floor.transform.SetParent(_root, false);
    floor.transform.localPosition = world + new Vector3(0f, 0.04f, 0f);
    floor.transform.localScale = new Vector3(0.9f, 0.08f, 0.9f);
    floor.GetComponent<Renderer>().sharedMaterial = _floorUnlitMat;

    var collider = floor.GetComponent<BoxCollider>();
    collider.center = Vector3.zero;
    collider.size = new Vector3(1f, 4f, 1f);

    floor.AddComponent<GridCellClickable>().Pos = pos;
    return floor.GetComponent<Renderer>();
  }

  private GameObject CreateObstacle(Vector3 world, GridPos pos)
  {
    var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
    block.name = $"Obstacle_{pos.X}_{pos.Y}";
    block.transform.SetParent(_root, false);
    block.transform.localPosition = world + new Vector3(0f, 0.22f, 0f);
    block.transform.localScale = new Vector3(0.86f, 0.44f, 0.86f);
    block.GetComponent<Renderer>().sharedMaterial = _obstacleMat;
    Destroy(block.GetComponent<Collider>());
    return block;
  }

  private LightFixture CreateFixture(GridPos pos, bool conflict)
  {
    var world = GridToWorld(pos);
    var fixture = new LightFixture();
    fixture.Root = new GameObject($"Light_{pos.X}_{pos.Y}");
    fixture.Root.transform.SetParent(_root, false);
    fixture.Root.transform.localPosition = world + new Vector3(0f, 0.12f, 0f);

    var basePost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    basePost.name = "Post";
    basePost.transform.SetParent(fixture.Root.transform, false);
    basePost.transform.localPosition = new Vector3(0f, 0.06f, 0f);
    basePost.transform.localScale = new Vector3(0.22f, 0.06f, 0.22f);
    basePost.GetComponent<Renderer>().sharedMaterial = _obstacleMat;
    Destroy(basePost.GetComponent<Collider>());

    var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    bulb.name = "Bulb";
    bulb.transform.SetParent(fixture.Root.transform, false);
    bulb.transform.localPosition = new Vector3(0f, 0.18f, 0f);
    bulb.transform.localScale = Vector3.one * 0.36f;
    fixture.BulbRenderer = bulb.GetComponent<Renderer>();
    fixture.BulbRenderer.sharedMaterial = conflict ? _bulbConflictMat : _bulbMat;
    Destroy(bulb.GetComponent<Collider>());

    fixture.PointLight = fixture.Root.AddComponent<Light>();
    fixture.PointLight.type = LightType.Point;
    fixture.PointLight.color = conflict ? new Color(1f, 0.45f, 0.55f) : new Color(1f, 0.92f, 0.65f);
    fixture.PointLight.intensity = conflict ? 1.6f : 2.4f;
    fixture.PointLight.range = 1.6f;
    fixture.PointLight.shadows = _mobileLayout ? LightShadows.None : LightShadows.Soft;

    for (var i = 0; i < GridLightLogic.Dirs.Length; i++)
    {
      var dir = GridLightLogic.Dirs[i];
      var len = GridLightLogic.RayLength(pos, dir, _level.Cells, _level.Obstacles);
      var beamGo = new GameObject($"Beam_{dir.x}_{dir.y}");
      beamGo.transform.SetParent(fixture.Root.transform, false);
      beamGo.transform.localPosition = new Vector3(0f, 0.18f, 0f);
      beamGo.transform.localRotation = Quaternion.Euler(BeamRotations[i]);

      var beam = beamGo.AddComponent<Light>();
      beam.type = LightType.Spot;
      beam.color = conflict ? new Color(1f, 0.5f, 0.6f) : new Color(1f, 0.95f, 0.75f);
      beam.intensity = conflict ? 2.2f : 3.2f;
      beam.range = Mathf.Max(1.2f, (len + 0.6f) * CellSize);
      beam.spotAngle = 72f;
      beam.innerSpotAngle = 48f;
      beam.shadows = _mobileLayout ? LightShadows.None : LightShadows.Soft;
      fixture.Beams[i] = beam;
    }

    return fixture;
  }

  private void UpdateFixture(LightFixture fixture, GridPos pos, bool conflict)
  {
    fixture.BulbRenderer.sharedMaterial = conflict ? _bulbConflictMat : _bulbMat;
    fixture.PointLight.color = conflict ? new Color(1f, 0.45f, 0.55f) : new Color(1f, 0.92f, 0.65f);
    fixture.PointLight.intensity = conflict ? 1.6f : 2.4f;

    for (var i = 0; i < GridLightLogic.Dirs.Length; i++)
    {
      var len = GridLightLogic.RayLength(pos, GridLightLogic.Dirs[i], _level.Cells, _level.Obstacles);
      var beam = fixture.Beams[i];
      beam.color = conflict ? new Color(1f, 0.5f, 0.6f) : new Color(1f, 0.95f, 0.75f);
      beam.intensity = conflict ? 2.2f : 3.2f;
      beam.range = Mathf.Max(1.2f, (len + 0.6f) * CellSize);
    }
  }

  private void FitCamera(int minX, int maxX, int minY, int maxY)
  {
    if (_camera == null) return;

    ApplyGameplayCameraRect();

    // Include base-plate padding so the whole board stays inside the viewport.
    var width = (maxX - minX + 3) * CellSize;
    var depth = (maxY - minY + 3) * CellSize;
    var center = new Vector3((minX + maxX) * 0.5f * CellSize, 0f, (minY + maxY) * 0.5f * CellSize) + _root.position;

    // Pure top-down: facing the floor, board fully framed in the gameplay viewport.
    const float margin = 1.04f;
    _camera.orthographic = true;
    _camera.nearClipPlane = 0.1f;
    _camera.farClipPlane = 80f;
    _camera.transform.position = center + Vector3.up * 24f;
    _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

    var aspect = Mathf.Max(0.01f, _camera.aspect);
    var halfDepth = depth * 0.5f * margin;
    var halfWidth = width * 0.5f * margin;
    _camera.orthographicSize = Mathf.Max(halfDepth, halfWidth / aspect);
    _camera.backgroundColor = GridLightTheme.BgGameplay;
  }

  private Material _shaderTemplate;

  private void CreateMaterials()
  {
    _floorUnlitMat = CreateMat(GridLightTheme.FloorUnlit, GridLightTheme.FloorUnlitEmission, 0.18f);
    _floorLitMat = CreateMat(GridLightTheme.FloorLit, GridLightTheme.FloorLitEmission, 0.55f);
    _floorBaseMat = CreateMat(GridLightTheme.FloorBase, GridLightTheme.FloorUnlitEmission, 0.08f);
    _obstacleMat = CreateMat(new Color32(72, 86, 98, 255), new Color32(28, 34, 40, 255), 0.05f);
    _bulbMat = CreateMat(new Color32(255, 220, 130, 255), new Color32(255, 195, 80, 255), 1.6f);
    _bulbConflictMat = CreateMat(new Color32(230, 100, 90, 255), new Color32(200, 60, 50, 255), 1.2f);
  }

  private void EnsureShaderTemplate()
  {
    if (_shaderTemplate != null) return;
    var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
    _shaderTemplate = temp.GetComponent<Renderer>().sharedMaterial;
    Destroy(temp);
  }

  private Shader ResolveShader()
  {
    var candidates = new[]
    {
      "Unlit/Color",
      "Mobile/Unlit (Supports Lightmap)",
      "Standard",
      "Mobile/Diffuse",
      "Legacy Shaders/Diffuse",
      "Diffuse",
    };

    foreach (var name in candidates)
    {
      var shader = Shader.Find(name);
      if (shader != null) return shader;
    }

    EnsureShaderTemplate();
    return _shaderTemplate != null ? _shaderTemplate.shader : null;
  }

  private Material CreateMat(Color baseColor, Color emission, float emissionStrength)
  {
    EnsureShaderTemplate();
    var shader = ResolveShader();
    Material mat;
    if (shader != null)
    {
      mat = new Material(shader) { color = baseColor };
    }
    else
    {
      mat = new Material(_shaderTemplate) { color = baseColor };
    }

    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.25f);
    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
    if (emissionStrength > 0f && mat.HasProperty("_EmissionColor"))
    {
      mat.EnableKeyword("_EMISSION");
      mat.SetColor("_EmissionColor", emission * emissionStrength);
      mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    return mat;
  }
}
