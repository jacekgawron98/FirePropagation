using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireState
{
    Normal,
    OnFire,
    BurntDown
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HeatSource))]
public class FireComponent : MonoBehaviour
{
    public GameObject firePrefab;
    public FireSettings fireSettings;
    public int gridSizeX;
    
    private FireState _state;
    private Renderer _meshRenderer;
    private SpatialGrid _grid;
    private List<FireCell> _burningCells;
    private Dictionary<Vector3, GameObject> _flames;
    private HeatSource _heatSource;
    private bool _hasStartedFire = false;

    public bool HasStartedFire { get { return _hasStartedFire; } }
    public FireState State { get { return _state; } }
    void Start()
    {
        _state = FireState.Normal;
        _heatSource = GetComponent<HeatSource>();
        _heatSource.enabled = false;
        _meshRenderer = GetComponent<Renderer>();
        _burningCells = new List<FireCell>();
    }

    void Update()
    {
        if (_grid != null)
        {
            foreach (var cell in _grid.gridCells)
            {
                if (_burningCells.Contains(cell.Value))
                {
                    if (cell.Value.state == FireState.Normal && cell.Value.HasReachedBurningTemperature())
                    {
                        _flames.Add(cell.Value.position,
                            Instantiate(firePrefab, cell.Value.position, Quaternion.identity, transform));
                        _burningCells.AddRange(GetCellNeighbours(cell.Value));
                        _state = FireState.OnFire;
                        _heatSource.enabled = true;
                        break;
                    }
                    else if (!cell.Value.IsStillBurning(Time.deltaTime))
                    {
                        Destroy(_flames[cell.Key]);
                        _flames.Remove(cell.Key);
                        _burningCells.Remove(cell.Value);
                    }
                }
            }
        }
        if (_burningCells.Count == 0 && _state == FireState.OnFire)
        {
            _heatSource.enabled = false;
            _state = FireState.BurntDown;
        }
    }

    public void StartFire(Vector3 heatSourcePosition)
    {
        if (_state == FireState.Normal)
        {
            _hasStartedFire = true;
            _flames = new Dictionary<Vector3, GameObject>();
            FindAllCells();
            FireCell nearestCell = FindNearestCell(heatSourcePosition);
            _burningCells.Add(nearestCell);
        }
    }

    private void FindAllCells()
    {
        _grid = new SpatialGrid();
        _grid.radius = _meshRenderer.bounds.size.x / gridSizeX;
        _grid.radius /= 2;

        float multiplier = gridSizeX % 2 == 0 ? 1.5f : 2;
        float startPointX = _grid.radius * multiplier * (gridSizeX / 2);
        float startPointY = ((_meshRenderer.bounds.size.y / (_grid.radius * 2)) / 2) * multiplier * _grid.radius;
        float startPointZ = ((_meshRenderer.bounds.size.z / (_grid.radius * 2)) / 2) * multiplier * _grid.radius;
        float interval = _grid.radius * 2;

        for (float x = -startPointX; x <= startPointX; x += interval)
        {
            for (float y = -startPointY; y <= startPointY; y += interval)
            {
                for (float z = -startPointZ; z <= startPointZ; z += interval)
                {
                    Vector3 position = new Vector3(x, y, z) + _meshRenderer.bounds.center;
                    Collider[] hitColliders = Physics.OverlapSphere(position, _grid.radius);
                    foreach (Collider collider in hitColliders)
                    {
                        if (collider.gameObject == gameObject)
                        {
                            AddCellToGrid(position);
                        }
                    }
                }
            }
        }
    }

    public void AddCellToGrid(Vector3 position)
    {
        FireCell cell = new FireCell();
        if (cell != null)
        {
            cell.radius = _grid.radius;
            cell.position = position;
            cell.settings = fireSettings;
            if (!_grid.gridCells.ContainsKey(position))
            {
                _grid.gridCells.Add(position, cell);
            }
        }
    }

    private FireCell FindNearestCell(Vector3 heatSourcePosition)
    {
        FireCell nearestCell = null;
        float nearestDistance = 0;
        foreach (var cell in _grid.gridCells)
        {
            if (nearestCell == null)
            {
                nearestCell = cell.Value;
                nearestDistance = Vector3.Distance(cell.Key, heatSourcePosition);
            }
            else
            {
                if (Vector3.Distance(cell.Key, heatSourcePosition) < nearestDistance)
                {
                    nearestCell = cell.Value;
                    nearestDistance = Vector3.Distance(cell.Key, heatSourcePosition);
                }
            }
        }
        return nearestCell;
    }

    public List<FireCell> GetCellNeighbours(FireCell cell)
    {
        List<FireCell> neighbours = new List<FireCell>();
        for (int i = -1; i <= 1; i += 2)
        {
            Vector3 neighbourPositionX = cell.position + new Vector3(i * _grid.radius * 2, 0, 0);
            Vector3 neighbourPositionY = cell.position + new Vector3(0, i * _grid.radius * 2, 0);
            Vector3 neighbourPositionZ = cell.position + new Vector3(0, 0, i * _grid.radius * 2);
            foreach (var c in _grid.gridCells)
            {
                if (c.Key == neighbourPositionX && !_burningCells.Contains(c.Value))
                {
                    neighbours.Add(c.Value);
                }
                else if (c.Key == neighbourPositionY && !_burningCells.Contains(c.Value))
                {
                    neighbours.Add(c.Value);
                }
                else if (c.Key == neighbourPositionZ && !_burningCells.Contains(c.Value))
                {
                    neighbours.Add(c.Value);
                }
            }
        }
        return neighbours;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (_grid != null)
        {
            foreach (var cell in _grid.gridCells)
            {
                Gizmos.DrawWireSphere(cell.Key, cell.Value.radius);
            }
        }
    }
}
