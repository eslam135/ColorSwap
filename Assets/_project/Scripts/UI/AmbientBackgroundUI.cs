using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmbientBackgroundUI : MonoBehaviour
{
    private class AmbientNode
    {
        public RectTransform rectTransform;
        public Image image;
        public Vector2 velocity;
        public float baseAlpha;
        public float pulseOffset;
    }

    private class AmbientLink
    {
        public int nodeA;
        public int nodeB;
        public RectTransform rectTransform;
        public Image image;
    }

    [Header("References")]
    [SerializeField] private RectTransform _root;
    [SerializeField] private Image _dotPrefab;
    [SerializeField] private Image _linePrefab;

    [Header("Node Settings")]
    [SerializeField] private int _nodeCount = 18;
    [SerializeField] private Vector2 _nodeSizeRange = new Vector2(5f, 11f);
    [SerializeField] private Vector2 _speedRange = new Vector2(4f, 12f);

    [Header("Link Settings")]
    [SerializeField] private int _maxLinksPerNode = 2;
    [SerializeField] private float _maxLinkDistance = 240f;
    [SerializeField] private float _lineThickness = 2f;

    [Header("Visuals")]
    [SerializeField] private Color _dotColor = new Color(0.75f, 0.78f, 0.82f, 0.20f);
    [SerializeField] private Color _accentDotColor = new Color(0.82f, 0.88f, 1f, 0.28f);
    [SerializeField] private Color _lineColor = new Color(0.20f, 0.22f, 0.25f, 0.08f);

    [Header("Animation")]
    [SerializeField] private float _pulseSpeed = 1.4f;
    [SerializeField] private float _pulseAmount = 0.25f;

    private readonly List<AmbientNode> _nodes = new List<AmbientNode>();
    private readonly List<AmbientLink> _links = new List<AmbientLink>();

    private void Start()
    {
        if (_root == null)
        {
            _root = GetComponent<RectTransform>();
        }

        BuildBackground();
    }

    [ContextMenu("Rebuild Background")]
    public void BuildBackground()
    {
        ClearBackground();
        SpawnNodes();
        SpawnLinks();
        UpdateLinksImmediate();
    }

    private void Update()
    {
        if (_nodes.Count == 0 || _root == null)
        {
            return;
        }

        Rect bounds = _root.rect;
        float dt = Time.deltaTime;

        for (int i = 0; i < _nodes.Count; i++)
        {
            AmbientNode node = _nodes[i];

            Vector2 position = node.rectTransform.anchoredPosition;
            position += node.velocity * dt;

            float halfSize = node.rectTransform.sizeDelta.x * 0.5f;

            if (position.x < bounds.xMin + halfSize)
            {
                position.x = bounds.xMin + halfSize;
                node.velocity.x *= -1f;
            }
            else if (position.x > bounds.xMax - halfSize)
            {
                position.x = bounds.xMax - halfSize;
                node.velocity.x *= -1f;
            }

            if (position.y < bounds.yMin + halfSize)
            {
                position.y = bounds.yMin + halfSize;
                node.velocity.y *= -1f;
            }
            else if (position.y > bounds.yMax - halfSize)
            {
                position.y = bounds.yMax - halfSize;
                node.velocity.y *= -1f;
            }

            node.rectTransform.anchoredPosition = position;

            Color color = node.image.color;
            float pulse = 1f - _pulseAmount + Mathf.Sin(Time.time * _pulseSpeed + node.pulseOffset) * 0.5f * _pulseAmount;
            color.a = node.baseAlpha * pulse;
            node.image.color = color;
        }

        UpdateLinksImmediate();
    }

    private void ClearBackground()
    {
        for (int i = _root.childCount - 1; i >= 0; i--)
        {
            Destroy(_root.GetChild(i).gameObject);
        }

        _nodes.Clear();
        _links.Clear();
    }

    private void SpawnNodes()
    {
        Rect bounds = _root.rect;

        for (int i = 0; i < _nodeCount; i++)
        {
            Image dot = Instantiate(_dotPrefab, _root);
            dot.raycastTarget = false;

            RectTransform rectTransform = dot.rectTransform;

            float size = Random.Range(_nodeSizeRange.x, _nodeSizeRange.y);
            rectTransform.sizeDelta = new Vector2(size, size);
            rectTransform.anchoredPosition = GetRandomPoint(bounds);

            Color color = Random.value < 0.2f ? _accentDotColor : _dotColor;
            color.a *= Random.Range(0.85f, 1.15f);
            dot.color = color;

            Vector2 velocity = Random.insideUnitCircle.normalized * Random.Range(_speedRange.x, _speedRange.y);

            _nodes.Add(new AmbientNode
            {
                rectTransform = rectTransform,
                image = dot,
                velocity = velocity,
                baseAlpha = color.a,
                pulseOffset = Random.Range(0f, Mathf.PI * 2f)
            });
        }
    }

    private void SpawnLinks()
    {
        if (_linePrefab == null)
        {
            return;
        }

        int[] linkCounts = new int[_nodes.Count];

        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                if (linkCounts[i] >= _maxLinksPerNode || linkCounts[j] >= _maxLinksPerNode)
                {
                    continue;
                }

                float distance = Vector2.Distance(
                    _nodes[i].rectTransform.anchoredPosition,
                    _nodes[j].rectTransform.anchoredPosition
                );

                if (distance > _maxLinkDistance)
                {
                    continue;
                }

                float chance = 0.14f;
                if (Random.value > chance)
                {
                    continue;
                }

                Image line = Instantiate(_linePrefab, _root);
                line.raycastTarget = false;
                line.transform.SetAsFirstSibling();

                RectTransform rectTransform = line.rectTransform;
                rectTransform.pivot = new Vector2(0f, 0.5f);

                _links.Add(new AmbientLink
                {
                    nodeA = i,
                    nodeB = j,
                    rectTransform = rectTransform,
                    image = line
                });

                linkCounts[i]++;
                linkCounts[j]++;
            }
        }
    }

    private void UpdateLinksImmediate()
    {
        for (int i = 0; i < _links.Count; i++)
        {
            AmbientLink link = _links[i];

            Vector2 start = _nodes[link.nodeA].rectTransform.anchoredPosition;
            Vector2 end = _nodes[link.nodeB].rectTransform.anchoredPosition;

            Vector2 direction = end - start;
            float distance = direction.magnitude;

            link.rectTransform.anchoredPosition = start;
            link.rectTransform.sizeDelta = new Vector2(distance, _lineThickness);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            link.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

            if (link.image != null)
            {
                link.image.color = _lineColor;
            }
        }
    }

    private Vector2 GetRandomPoint(Rect bounds)
    {
        float x = Random.Range(bounds.xMin, bounds.xMax);
        float y = Random.Range(bounds.yMin, bounds.yMax);
        return new Vector2(x, y);
    }
}