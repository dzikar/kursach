using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для UI элементов
using Button = UnityEngine.UI.Button; // Явное указание
using Text = UnityEngine.UI.Text; // Явное указание
using InputField = UnityEngine.UI.InputField; // Явное указание

public class GraphController : MonoBehaviour
{
    public Graph graph = new Graph();

    [Header("UI References")]
    public InputField startVertexInput;
    public InputField endVertexInput;
    public Button findPathButton;
    public Text resultText;
    public Transform graphContainer;

    [Header("Prefabs")]
    public GameObject vertexPrefab;
    public GameObject edgePrefab;
    //public GameObject weightTextPrefab; //

    private List<GameObject> visualElements = new List<GameObject>();
    private int selectedStartVertex = -1;
    private int selectedEndVertex = -1;

    [Header("Label Settings")]
    public Font labelFont;
    public int labelFontSize = 14;
    public Color labelColor = Color.black;
    public Vector2 labelOffset = new Vector2(0, -0.7f);

    void Start()
    {
        //InitializeTestGraph();
        DrawGraph();

        findPathButton.onClick.AddListener(FindShortestPath);
    }

    /*void InitializeTestGraph()
    {
        // Добавление вершин
        graph.vertices.Add(new Vertex(0, "A", new Vector2(-2, 1)));
        graph.vertices.Add(new Vertex(1, "B", new Vector2(0, 2)));
        graph.vertices.Add(new Vertex(2, "C", new Vector2(2, 1)));
        graph.vertices.Add(new Vertex(3, "D", new Vector2(0, 0)));
        graph.vertices.Add(new Vertex(4, "E", new Vector2(-2, -1)));

        // Добавление рёбер
        graph.edges.Add(new Edge(0, 1, 4));
        graph.edges.Add(new Edge(0, 3, 2));
        graph.edges.Add(new Edge(1, 2, 3));
        graph.edges.Add(new Edge(1, 3, 1));
        graph.edges.Add(new Edge(2, 3, 5));
        graph.edges.Add(new Edge(3, 4, 6));
        graph.edges.Add(new Edge(4, 0, 3));
    }*/

    public void ClearGraph()
    {
        graph.vertices.Clear();
        graph.edges.Clear();
        DrawGraph();
    }

    public void DrawGraph()
    {
        ClearVisualElements();

        // Отрисовка рёбер
        foreach (var edge in graph.edges)
        {
            Vertex fromVertex = graph.GetVertex(edge.from);
            Vertex toVertex = graph.GetVertex(edge.to);

            GameObject edgeObj = Instantiate(edgePrefab, graphContainer);
            LineRenderer lineRenderer = edgeObj.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, fromVertex.position);
            lineRenderer.SetPosition(1, toVertex.position);
            lineRenderer.startColor = edge.color;
            lineRenderer.endColor = edge.color;

            // Добавление текста с весом через EdgeWeightDisplay
            EdgeWeightDisplay weightDisplay = edgeObj.GetComponent<EdgeWeightDisplay>();
            if (weightDisplay != null)
            {
                Vector3 midPoint = (fromVertex.position + toVertex.position) / 2;
                // Смещаем текст немного в сторону для лучшей читаемости
                Vector3 direction = (toVertex.position - fromVertex.position).normalized;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
                Vector3 textPosition = midPoint + perpendicular * weightDisplay.offset;

                weightDisplay.Initialize(edge.weight, textPosition);
                weightDisplay.SetColor(edge.color);
            }
            else
            {
                // Если компонент EdgeWeightDisplay отсутствует, используем старый метод
                CreateWeightText(edge.weight, fromVertex.position, toVertex.position, edge.color);
            }

            visualElements.Add(edgeObj);
        }

        // Отрисовка вершин
        foreach (var vertex in graph.vertices)
        {
            GameObject vertexObj = Instantiate(vertexPrefab, graphContainer);
            vertexObj.transform.position = vertex.position;

            SpriteRenderer renderer = vertexObj.GetComponent<SpriteRenderer>();
            renderer.color = vertex.color;

            VertexClickHandler clickHandler = vertexObj.GetComponent<VertexClickHandler>();
            clickHandler.Initialize(this, vertex.id);

            CreateVertexLabel(vertex, vertexObj);

            visualElements.Add(vertexObj);
        }
    }

    public void RemoveVertex(int vertexId)
    {
        graph.vertices.RemoveAll(v => v.id == vertexId);
        graph.edges.RemoveAll(e => e.from == vertexId || e.to == vertexId);
        DrawGraph();
    }

    public void RemoveEdge(int fromId, int toId)
    {
        graph.edges.RemoveAll(e => e.from == fromId && e.to == toId);
        DrawGraph();
    }

    public void ResetSelection()
    {
        selectedStartVertex = -1;
        selectedEndVertex = -1;
        startVertexInput.text = "";
        endVertexInput.text = "";
        resultText.text = "";
    }

    void ClearVisualElements()
    {
        foreach (var element in visualElements)
        {
            if (element != null)
            {
                Destroy(element);
            }
        }
        visualElements.Clear();

        foreach (var vertex in graph.vertices)
        {
            vertex.labelObject = null;
        }
    }

    public void OnVertexClicked(int vertexId)
    {
        if (selectedStartVertex == -1)
        {
            selectedStartVertex = vertexId;
            startVertexInput.text = graph.GetVertex(vertexId).name;
        }
        else if (selectedEndVertex == -1)
        {
            selectedEndVertex = vertexId;
            endVertexInput.text = graph.GetVertex(vertexId).name;
        }
        else
        {
            selectedStartVertex = vertexId;
            selectedEndVertex = -1;
            startVertexInput.text = graph.GetVertex(vertexId).name;
            endVertexInput.text = "";
        }
    }

    public void FindShortestPath()
    {
        if (selectedStartVertex == -1 || selectedEndVertex == -1)
        {
            resultText.text = "Выберите начальную и конечную вершины!";
            return;
        }

        var (distances, predecessors) = ShortestPathFinder.FloydWarshall(graph);
        List<int> path = ShortestPathFinder.ReconstructPath(predecessors, selectedStartVertex, selectedEndVertex);

        if (path.Count == 0)
        {
            resultText.text = "Путь не существует!";
            return;
        }

        // ПРАВИЛЬНЫЙ расчет общей длины:
        // Берем готовое расстояние из матрицы от start до end
        float totalDistance = distances[selectedStartVertex, selectedEndVertex];

        // Отображение результата
        string pathString = "";
        for (int i = 0; i < path.Count; i++)
        {
            pathString += graph.GetVertex(path[i]).name;
            if (i < path.Count - 1)
                pathString += " → ";
        }

        resultText.text = $"Кратчайший путь: {pathString}\nОбщая длина: {totalDistance}";

        // Визуальное выделение пути
        HighlightPath(path);
    }

    void HighlightPath(List<int> path)
    {
        // Сброс цветов
        foreach (var vertex in graph.vertices)
        {
            vertex.color = Color.white;
        }
        foreach (var edge in graph.edges)
        {
            edge.color = Color.white;
        }

        // Подсветка вершин пути
        foreach (int vertexId in path)
        {
            graph.GetVertex(vertexId).color = Color.green;
        }

        // Подсветка рёбер пути
        for (int i = 0; i < path.Count - 1; i++)
        {
            int from = path[i];
            int to = path[i + 1];

            Edge edge = graph.edges.Find(e => e.from == from && e.to == to);
            if (edge != null)
            {
                edge.color = Color.green;
            }
        }

        DrawGraph();
    }

    /*public void AddRandomVertex()
    {
        int newId = graph.vertices.Count;
        Vector2 randomPos = new Vector2(
            UnityEngine.Random.Range(-5f, 5f),
            UnityEngine.Random.Range(-3f, 3f)
        );
        graph.vertices.Add(new Vertex(newId, $"V{newId}", randomPos));
        DrawGraph();
    }

    public void AddRandomEdge()
    {
        if (graph.vertices.Count < 2) return;

        int from = UnityEngine.Random.Range(0, graph.vertices.Count);
        int to = UnityEngine.Random.Range(0, graph.vertices.Count);

        if (from != to)
        {
            float weight = UnityEngine.Random.Range(1, 10);
            graph.edges.Add(new Edge(from, to, weight));
            DrawGraph();
        }
    }*/

    void CreateWeightText(float weight, Vector2 fromPos, Vector2 toPos, Color color)
    {
        // Создаем объект для текста
        GameObject textObj = new GameObject("WeightText");
        textObj.transform.SetParent(graphContainer);

        // Добавляем TextMesh
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        MeshRenderer meshRenderer = textObj.GetComponent<MeshRenderer>();

        // Настройки текста
        textMesh.text = weight.ToString("F1");
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        // Загрузка шрифта legacyruntime.ttf
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Настройка материала для лучшего отображения текста
        meshRenderer.material = textMesh.font.material;
        meshRenderer.material.color = color;

        // Оптимизация рендеринга текста
        meshRenderer.material.SetInt("_ZWrite", 1);
        meshRenderer.material.EnableKeyword("_ALPHABLEND_ON");
        meshRenderer.material.renderQueue = 3000;

        // Позиционирование
        Vector2 midPoint = (fromPos + toPos) / 2;
        Vector2 direction = (toPos - fromPos).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        Vector2 textPosition = midPoint + perpendicular * 0.3f;

        textObj.transform.position = textPosition;

        // Поворачиваем текст для лучшей читаемости
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        textObj.transform.rotation = Quaternion.Euler(0, 0, angle + 90);

        visualElements.Add(textObj);
    }


    private void CreateVertexLabel(Vertex vertex, GameObject vertexObj)
    {
        // Создаем объект для текста
        GameObject labelObj = new GameObject($"Label_{vertex.name}");
        labelObj.transform.SetParent(vertexObj.transform);
        labelObj.transform.localPosition = labelOffset;

        // Добавляем TextMesh
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        MeshRenderer meshRenderer = labelObj.GetComponent<MeshRenderer>();

        // Настройки текста
        textMesh.text = vertex.name;
        textMesh.fontSize = labelFontSize;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.UpperCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = labelColor;

        // Настройка шрифта
        if (labelFont != null)
        {
            textMesh.font = labelFont;
        }
        else
        {
            textMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Сохраняем ссылку на объект текста
        vertex.labelObject = labelObj;

        visualElements.Add(labelObj);
    }
}