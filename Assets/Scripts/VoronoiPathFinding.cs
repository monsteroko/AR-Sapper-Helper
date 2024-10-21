using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VoronoiPathFinding : MonoBehaviour
{

    public List<Mine> mines;  // Список мин (с координатами GPS и радиусами взрыва)
    public GPSPoint startPoint; // Начальная точка пользователя
    public float fieldWidth;    // Ширина минного поля
    public float fieldHeight;   // Высота минного поля

    public float cellSize = 50f; // 50 метров, например

    public void StartPathfinding()
    {

        ComputeFieldDimensions();
        // Построение графа на основе диаграммы Вороного
        List<GPSPoint> gridPoints = GenerateGridPoints();

        // Поиск пути от начальной точки до противоположной стороны
        StartCoroutine(FindPath(startPoint, gridPoints));
    }
    void ComputeFieldDimensions()
    {
        if (mines == null || mines.Count == 0)
        {
            Debug.LogError("Не удалось вычислить размеры поля: список мин пуст.");
            return;
        }

        double minLatitude = double.MaxValue;
        double maxLatitude = double.MinValue;
        double minLongitude = double.MaxValue;
        double maxLongitude = double.MinValue;

        // Проходим по всем минам, чтобы найти крайние координаты
        foreach (var mine in mines)
        {
            if (mine.Position.Latitude < minLatitude) minLatitude = mine.Position.Latitude;
            if (mine.Position.Latitude > maxLatitude) maxLatitude = mine.Position.Latitude;
            if (mine.Position.Longitude < minLongitude) minLongitude = mine.Position.Longitude;
            if (mine.Position.Longitude > maxLongitude) maxLongitude = mine.Position.Longitude;
        }

        // Вычисляем ширину и высоту поля в метрах
        fieldWidth = (float)new GPSPoint(minLatitude, minLongitude).DistanceTo(new GPSPoint(minLatitude, maxLongitude));
        fieldHeight = (float)new GPSPoint(minLatitude, minLongitude).DistanceTo(new GPSPoint(maxLatitude, minLongitude));

        Debug.Log($"Ширина минного поля: {fieldWidth} метров");
        Debug.Log($"Высота минного поля: {fieldHeight} метров");
    }

    // Генерация сетки узлов
    List<GPSPoint> GenerateGridPoints()
    {
        List<GPSPoint> gridPoints = new List<GPSPoint>();

        int numColumns = Mathf.CeilToInt(fieldWidth / cellSize);
        int numRows = Mathf.CeilToInt(fieldHeight / cellSize);

        // Генерация узлов на сетке
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                double latitude = startPoint.Latitude + (y * (fieldHeight / numRows));
                double longitude = startPoint.Longitude + (x * (fieldWidth / numColumns));
                GPSPoint point = new GPSPoint(latitude, longitude);

                // Добавляем точку, если она безопасна
                if (IsSafe(point))
                {
                    gridPoints.Add(point);
                }
            }
        }

        return gridPoints;
    }

    // Метод для проверки безопасности точки
    bool IsSafe(GPSPoint point)
    {
        foreach (var mine in mines)
        {
            if (point.DistanceTo(mine.Position) < mine.BlastRadius)
            {
                return false; // Точка находится внутри радиуса взрыва мины
            }
        }
        return true; // Точка безопасна
    }

    // Получение соседних точек для текущей позиции
    List<GPSPoint> GetNeighbors(GPSPoint currentPoint, List<GPSPoint> gridPoints)
    {
        List<GPSPoint> neighbors = new List<GPSPoint>();

        // Максимальное расстояние для соседей (это зависит от размера сетки)
        double maxNeighborDistance = cellSize; // Соседи могут находиться в пределах одной ячейки

        foreach (var point in gridPoints)
        {
            if (!point.Equals(currentPoint) && currentPoint.DistanceTo(point) <= maxNeighborDistance)
            {
                neighbors.Add(point);
            }
        }

        return neighbors;
    }

    // Поиск пути от начальной точки до противоположной стороны (алгоритм A*)
    IEnumerator FindPath(GPSPoint start, List<GPSPoint> gridPoints)
    {
        var openList = new List<Node>();
        var closedList = new HashSet<GPSPoint>();

        var startNode = new Node(start, 0, DistanceToExit(start));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            openList.Sort((nodeA, nodeB) => nodeA.FCost.CompareTo(nodeB.FCost));
            var currentNode = openList[0];
            openList.RemoveAt(0);

            if (IsExit(currentNode.Position))
            {
                Debug.Log("Путь найден!");
                var path = ReconstructPath(currentNode);
                OnPathFound(path);
                yield break;
            }

            closedList.Add(currentNode.Position);

            // Получаем соседей для текущего узла
            var neighbors = GetNeighbors(currentNode.Position, gridPoints);
            foreach (var neighbor in neighbors)
            {
                if (closedList.Contains(neighbor))
                    continue;

                var gCost = currentNode.GCost + currentNode.Position.DistanceTo(neighbor);
                var hCost = DistanceToExit(neighbor);

                var existingNode = openList.Find(n => n.Position.Equals(neighbor));

                if (existingNode == null)
                {
                    var newNode = new Node(neighbor, gCost, hCost)
                    {
                        Parent = currentNode
                    };
                    openList.Add(newNode);
                }
                else if (gCost < existingNode.GCost)
                {
                    existingNode.GCost = gCost;
                    existingNode.Parent = currentNode;
                }
            }

            yield return null;
        }

        Debug.Log("Путь не найден!");
    }

    // Пример метода для вычисления эвристической функции (расстояние до выхода)
    double DistanceToExit(GPSPoint point)
    {
        // Предположим, что выход находится на любой точке противоположной стороны поля (например, самая дальняя точка по широте)
        return Mathf.Abs((float)(fieldHeight - (point.Latitude - startPoint.Latitude)));
    }

    // Метод для проверки, является ли точка выходом
    bool IsExit(GPSPoint point)
    {
        // Выходом считается любая точка на противоположной стороне минного поля (вне мин)
        return point.Latitude >= startPoint.Latitude + fieldHeight;
    }

    // Восстановление пути
    List<GPSPoint> ReconstructPath(Node endNode)
    {
        List<GPSPoint> path = new List<GPSPoint>();
        var currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    // Метод вызывается при нахождении пути
    void OnPathFound(List<GPSPoint> path)
    {
        Debug.Log("Безопасный путь найден:");
        List<Vector3> pathPoints = new List<Vector3>();

        foreach (var point in path)
        {
            Debug.Log($"Latitude: {point.Latitude}, Longitude: {point.Longitude}");
            // Преобразуем координаты в систему координат Unity
            Vector3 worldPoint = GPSToWorldPosition(point);
            pathPoints.Add(worldPoint);
        }

        // Отобразим путь через LineRenderer
        /*pathLine.positionCount = pathPoints.Count;
        pathLine.SetPositions(pathPoints.ToArray());*/

        // Переместим игрока по найденному пути
        StartCoroutine(MovePlayerAlongPath(pathPoints));
    }

    // Пример преобразования GPS координат в систему координат Unity
    Vector3 GPSToWorldPosition(GPSPoint gps)
    {
        // Простое масштабирование для примера
        return new Vector3((float)(gps.Longitude * 100), 0, (float)(gps.Latitude * 100));
    }

    // Перемещение игрока по найденному пути
    IEnumerator MovePlayerAlongPath(List<Vector3> pathPoints)
    {
        foreach (var point in pathPoints)
        {
            Debug.Log(point.x + point.y + point.z);
            yield return new WaitForSeconds(1); // Ждем 1 секунду перед переходом к следующей точке
        }
    }

public class GPSPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public GPSPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }


    // Метод для вычисления расстояния между GPS координатами (в метрах)
    public double DistanceTo(GPSPoint other)
    {
        double R = 6371e3; // Радиус Земли в метрах
        double lat1Rad = Latitude * (Math.PI / 180);
        double lat2Rad = other.Latitude * (Math.PI / 180);
        double deltaLat = (other.Latitude - Latitude) * (Math.PI / 180);
        double deltaLon = (other.Longitude - Longitude) * (Math.PI / 180);

        double a = Mathf.Sin((float)(deltaLat / 2)) * Mathf.Sin((float)(deltaLat / 2)) +
                   Mathf.Cos((float)lat1Rad) * Mathf.Cos((float)lat2Rad) *
                   Mathf.Sin((float)(deltaLon / 2)) * Mathf.Sin((float)(deltaLon / 2));
        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt(1f - (float)a));

        return R * c; // Возвращает расстояние в метрах
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        GPSPoint p = (GPSPoint)obj;
        return Latitude == p.Latitude && Longitude == p.Longitude;
    }

    public override int GetHashCode()
    {
        return Latitude.GetHashCode() ^ Longitude.GetHashCode();
    }
}

public class Mine
{
    public GPSPoint Position { get; set; }
    public double BlastRadius { get; set; } // Радиус взрыва в метрах

    public Mine(GPSPoint position, double blastRadius)
    {
        Position = position;
        BlastRadius = blastRadius;
    }

    public bool IsInBlastRadius(GPSPoint point)
    {
        return Position.DistanceTo(point) <= BlastRadius;
    }
}

public class Node
{
    public GPSPoint Position { get; set; } // Позиция узла (GPS-координаты)
    public double GCost { get; set; }      // Стоимость пути от начальной точки
    public double HCost { get; set; }      // Эвристическая оценка (расстояние до цели)
    public double FCost => GCost + HCost;  // Общая стоимость (G + H)
    public Node Parent { get; set; }       // Родительский узел

    public Node(GPSPoint position, double gCost, double hCost)
    {
        Position = position;
        GCost = gCost;
        HCost = hCost;
        Parent = null;
    }
}


}

