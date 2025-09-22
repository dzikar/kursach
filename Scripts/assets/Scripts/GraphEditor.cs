using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GraphEditor : MonoBehaviour, IPointerClickHandler
{
    public GraphController graphController;
    public InputField vertexNameInput;
    public InputField edgeWeightInput;
    public Toggle directedToggle;
    public Button addVertexButton;
    public Button addEdgeButton;
    public Button deleteModeButton;
    public Text modeText;
    public Button clearButton;

    private enum EditorMode { AddVertex, AddEdge, Delete }
    private EditorMode currentMode = EditorMode.AddVertex;
    private Vertex firstVertexForEdge = null;

    void Start()
    {
        // Инициализация кнопок
        addVertexButton.onClick.AddListener(() => SetMode(EditorMode.AddVertex));
        addEdgeButton.onClick.AddListener(() => SetMode(EditorMode.AddEdge));
        deleteModeButton.onClick.AddListener(() => SetMode(EditorMode.Delete));
        clearButton.onClick.AddListener(ClearGraph);

        // Установка начального режима
        SetMode(EditorMode.AddVertex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);

        switch (currentMode)
        {
            case EditorMode.AddVertex:
                AddVertexAtPosition(worldPos);
                break;

            case EditorMode.AddEdge:
                HandleEdgeCreation(worldPos);
                break;

            case EditorMode.Delete:
                DeleteElementAtPosition(worldPos);
                break;
        }
    }

    private void AddVertexAtPosition(Vector2 position)
    {
        string name = string.IsNullOrEmpty(vertexNameInput.text) ?
                     $"V{graphController.graph.vertices.Count}" :
                     vertexNameInput.text;

        int newId = graphController.graph.vertices.Count;
        Vertex newVertex = new Vertex(newId, name, position);
        graphController.graph.vertices.Add(newVertex);

        graphController.DrawGraph(); // или graphController.RefreshGraph();
        vertexNameInput.text = "";
    }

    public void RenameVertex(int vertexId, string newName)
    {
        Vertex vertex = graphController.graph.vertices.Find(v => v.id == vertexId);
        if (vertex != null)
        {
            vertex.name = newName;
            UpdateVertexLabel(vertex);
        }
    }

    private void HandleEdgeCreation(Vector2 position)
    {
        Vertex clickedVertex = FindVertexAtPosition(position);

        if (clickedVertex == null) return;

        if (firstVertexForEdge == null)
        {
            firstVertexForEdge = clickedVertex;
            modeText.text = $"Режим: Выбрана вершина {firstVertexForEdge.name}, выберите вторую";
        }
        else
        {
            if (firstVertexForEdge.id != clickedVertex.id)
            {
                CreateEdgeBetweenVertices(firstVertexForEdge, clickedVertex);
            }
            firstVertexForEdge = null;
            SetMode(EditorMode.AddEdge); // Возвращаем обычный текст режима
        }
    }

    private void CreateEdgeBetweenVertices(Vertex from, Vertex to)
    {
        float weight = 1f;
        if (!string.IsNullOrEmpty(edgeWeightInput.text) && float.TryParse(edgeWeightInput.text, out weight))
        {
            // Проверяем, нет ли уже такого ребра
            bool edgeExists = graphController.graph.edges.Exists(e =>
                e.from == from.id && e.to == to.id);

            if (!edgeExists)
            {
                graphController.graph.edges.Add(new Edge(from.id, to.id, weight));

                // Если не ориентированное - добавляем обратное ребро
                if (!directedToggle.isOn)
                {
                    graphController.graph.edges.Add(new Edge(to.id, from.id, weight));
                }

                graphController.DrawGraph(); // или graphController.RefreshGraph();
            }
            else
            {
                Debug.Log("Такое ребро уже существует!");
            }

            edgeWeightInput.text = "";
        }
        else
        {
            Debug.Log("Введите корректный вес ребра!");
        }
    }

    private void DeleteElementAtPosition(Vector2 position)
    {
        // Проверяем вершины
        Vertex vertexToDelete = FindVertexAtPosition(position);
        if (vertexToDelete != null)
        {
            // Удаляем все связанные рёбра
            graphController.graph.edges.RemoveAll(e =>
                e.from == vertexToDelete.id || e.to == vertexToDelete.id);

            // Удаляем вершину
            graphController.graph.vertices.Remove(vertexToDelete);

            // Перенумеровываем оставшиеся вершины
            RenumberVertices();

            graphController.DrawGraph(); // или graphController.RefreshGraph();
            return;
        }

        // Проверяем рёбра
        Edge edgeToDelete = FindEdgeNearPosition(position);
        if (edgeToDelete != null)
        {
            graphController.graph.edges.Remove(edgeToDelete);
            graphController.DrawGraph(); // или graphController.RefreshGraph();
        }
    }

    private Vertex FindVertexAtPosition(Vector2 position, float radius = 0.5f)
    {
        foreach (var vertex in graphController.graph.vertices)
        {
            if (Vector2.Distance(position, vertex.position) <= radius)
                return vertex;
        }
        return null;
    }

    private Edge FindEdgeNearPosition(Vector2 position, float maxDistance = 0.3f)
    {
        foreach (var edge in graphController.graph.edges)
        {
            Vertex from = graphController.graph.GetVertex(edge.from);
            Vertex to = graphController.graph.GetVertex(edge.to);

            if (from != null && to != null &&
                IsPointNearLine(position, from.position, to.position, maxDistance))
                return edge;
        }
        return null;
    }

    private bool IsPointNearLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float maxDistance)
    {
        if (lineStart == lineEnd) return false;

        Vector2 lineDir = (lineEnd - lineStart).normalized;
        Vector2 pointDir = point - lineStart;

        float dot = Vector2.Dot(pointDir, lineDir);
        dot = Mathf.Clamp(dot, 0, Vector2.Distance(lineStart, lineEnd));

        Vector2 closestPoint = lineStart + lineDir * dot;
        return Vector2.Distance(point, closestPoint) <= maxDistance;
    }

    private void RenumberVertices()
    {
        for (int i = 0; i < graphController.graph.vertices.Count; i++)
        {
            graphController.graph.vertices[i].id = i;
            // Обновляем только автоматические имена
            if (graphController.graph.vertices[i].name.StartsWith("V"))
            {
                graphController.graph.vertices[i].name = $"V{i}";
            }
        }

        // Также обновляем все ссылки в рёбрах
        foreach (var edge in graphController.graph.edges)
        {
            // Корректировка не нужна, так как id вершин не меняются при удалении
            // благодаря перенумерации
        }
    }

    private void SetMode(EditorMode mode)
    {
        currentMode = mode;
        firstVertexForEdge = null;

        switch (mode)
        {
            case EditorMode.AddVertex:
                modeText.text = "Режим: Добавление вершин";
                break;
            case EditorMode.AddEdge:
                modeText.text = "Режим: Добавление рёбер (выберите первую вершину)";
                break;
            case EditorMode.Delete:
                modeText.text = "Режим: Удаление";
                break;
        }
    }

    private void UpdateVertexLabel(Vertex vertex)
    {
        if (vertex.labelObject != null)
        {
            TextMesh textMesh = vertex.labelObject.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = vertex.name;
            }
        }
        else
        {
            // Если labelObject null, перерисовываем весь граф
            graphController.DrawGraph();
        }
    }

    public void ClearGraph()
    {
        // Подтверждение действия (опционально)
        if (graphController.graph.vertices.Count > 0 || graphController.graph.edges.Count > 0)
        {
            // Можно добавить диалоговое окно подтверждения
            Debug.Log("Очищаем граф...");

            // Очищаем вершины и рёбра
            graphController.graph.vertices.Clear();
            graphController.graph.edges.Clear();

            // Сбрасываем выбранные вершины
            graphController.ResetSelection();

            // Перерисовываем пустой граф
            graphController.DrawGraph();

            Debug.Log("Граф очищен");
        }
        else
        {
            Debug.Log("Граф уже пустой");
        }
    }
}