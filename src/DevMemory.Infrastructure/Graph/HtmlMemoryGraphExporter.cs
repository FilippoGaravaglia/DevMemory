using System.Text;
using System.Text.Json;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Graph;

namespace DevMemory.Infrastructure.Graph;

public sealed class HtmlMemoryGraphExporter : IMemoryGraphHtmlExporter
{
    private readonly DevMemoryStorageOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HtmlMemoryGraphExporter()
        : this(new DevMemoryStorageOptions())
    {
    }

    public HtmlMemoryGraphExporter(DevMemoryStorageOptions options)
    {
        _options = options;
        Directory.CreateDirectory(_options.GraphDirectoryPath);
    }

    public string Export(MemoryGraph graph, string? outputPath = null)
    {
        var filePath = string.IsNullOrWhiteSpace(outputPath)
            ? _options.DefaultGraphHtmlFilePath
            : ExpandHomeDirectory(outputPath.Trim());

        var directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var html = BuildHtml(graph);

        File.WriteAllText(filePath, html, Encoding.UTF8);

        return filePath;
    }

    private static string BuildHtml(MemoryGraph graph)
    {
        var graphJson = JsonSerializer.Serialize(graph, JsonOptions);

        return $$"""
                    <!doctype html>
                    <html lang="en">
                    <head>
                        <meta charset="utf-8">
                        <title>DevMemory Graph</title>
                        <meta name="viewport" content="width=device-width, initial-scale=1">
                        <style>
                            :root {
                                --bg: #0f172a;
                                --panel: #111827;
                                --panel-2: #1f2937;
                                --text: #e5e7eb;
                                --muted: #9ca3af;
                                --line: #475569;
                                --memory: #60a5fa;
                                --project: #34d399;
                                --area: #fbbf24;
                                --tag: #f472b6;
                                --file: #a78bfa;
                            }

                            * {
                                box-sizing: border-box;
                            }

                            body {
                                margin: 0;
                                font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                                background: var(--bg);
                                color: var(--text);
                            }

                            header {
                                padding: 18px 24px;
                                border-bottom: 1px solid #1f2937;
                                background: #020617;
                            }

                            header h1 {
                                margin: 0;
                                font-size: 20px;
                            }

                            header p {
                                margin: 6px 0 0;
                                color: var(--muted);
                                font-size: 13px;
                            }

                            main {
                                display: grid;
                                grid-template-columns: minmax(0, 1fr) 360px;
                                height: calc(100vh - 76px);
                            }

                            #graph-wrapper {
                                position: relative;
                                overflow: hidden;
                            }

                            svg {
                                width: 100%;
                                height: 100%;
                                display: block;
                            }

                            aside {
                                border-left: 1px solid #1f2937;
                                background: var(--panel);
                                padding: 18px;
                                overflow: auto;
                            }

                            .card {
                                background: var(--panel-2);
                                border: 1px solid #374151;
                                border-radius: 14px;
                                padding: 14px;
                                margin-bottom: 14px;
                            }

                            .label {
                                color: var(--muted);
                                font-size: 12px;
                                text-transform: uppercase;
                                letter-spacing: .08em;
                                margin-bottom: 4px;
                            }

                            .value {
                                font-size: 14px;
                                word-break: break-word;
                            }

                            .legend {
                                display: grid;
                                grid-template-columns: 1fr 1fr;
                                gap: 8px;
                            }

                            .legend-item {
                                display: flex;
                                align-items: center;
                                gap: 8px;
                                color: var(--muted);
                                font-size: 13px;
                            }

                            .dot {
                                width: 10px;
                                height: 10px;
                                border-radius: 999px;
                                display: inline-block;
                            }

                            .node {
                                cursor: pointer;
                            }

                            .node circle {
                                stroke: #020617;
                                stroke-width: 2;
                            }

                            .node text {
                                fill: var(--text);
                                font-size: 11px;
                                paint-order: stroke;
                                stroke: #020617;
                                stroke-width: 3px;
                                stroke-linecap: round;
                                stroke-linejoin: round;
                            }

                            .edge {
                                stroke: var(--line);
                                stroke-width: 1.1;
                                opacity: .55;
                            }

                            .edge-label {
                                fill: var(--muted);
                                font-size: 9px;
                                opacity: .7;
                            }

                            .empty {
                                color: var(--muted);
                                font-size: 14px;
                            }
                        </style>
                    </head>
                    <body>
                    <header>
                        <h1>DevMemory Knowledge Graph</h1>
                        <p>Local graph generated from task memories. Click a node to inspect its details.</p>
                    </header>

                    <main>
                        <section id="graph-wrapper">
                            <svg id="graph" viewBox="-720 -520 1440 1040" role="img" aria-label="DevMemory knowledge graph"></svg>
                        </section>

                        <aside>
                            <div class="card">
                                <div class="label">Graph summary</div>
                                <div class="value" id="summary"></div>
                            </div>

                            <div class="card">
                                <div class="label">Legend</div>
                                <div class="legend">
                                    <div class="legend-item"><span class="dot" style="background: var(--memory)"></span>Memory</div>
                                    <div class="legend-item"><span class="dot" style="background: var(--project)"></span>Project</div>
                                    <div class="legend-item"><span class="dot" style="background: var(--area)"></span>Area</div>
                                    <div class="legend-item"><span class="dot" style="background: var(--tag)"></span>Tag</div>
                                    <div class="legend-item"><span class="dot" style="background: var(--file)"></span>File</div>
                                </div>
                            </div>

                            <div class="card">
                                <div class="label">Selected node</div>
                                <div class="value" id="details">No node selected.</div>
                            </div>

                            <div class="card">
                                <div class="label">Relations</div>
                                <div class="value" id="relations">No node selected.</div>
                            </div>
                        </aside>
                    </main>

                    <script>
                    const rawGraph = {{graphJson}};
                    
                    const graph = {
                        nodes: rawGraph.nodes ?? rawGraph.Nodes ?? [],
                        edges: rawGraph.edges ?? rawGraph.Edges ?? []
                    };

                    const svg = document.getElementById('graph');
                    const summary = document.getElementById('summary');
                    const details = document.getElementById('details');
                    const relations = document.getElementById('relations');

                    const colors = {
                        memory: '#60a5fa',
                        project: '#34d399',
                        area: '#fbbf24',
                        tag: '#f472b6',
                        file: '#a78bfa'
                    };

                    const radii = {
                        memory: 130,
                        project: 260,
                        area: 350,
                        tag: 450,
                        file: 560
                    };

                    summary.innerHTML = `${graph.nodes.length} nodes<br>${graph.edges.length} edges`;

                    const nodesById = new Map(graph.nodes.map(node => [node.id, node]));
                    const positionedNodes = layoutNodes(graph.nodes);

                    drawEdges(graph.edges, positionedNodes);
                    drawNodes(positionedNodes);

                    function layoutNodes(nodes) {
                        const grouped = groupBy(nodes, node => node.type);
                        const result = new Map();

                        const typeOrder = ['memory', 'project', 'area', 'tag', 'file'];

                        for (const type of typeOrder) {
                            const items = grouped.get(type) || [];
                            const radius = radii[type] || 600;

                            items.forEach((node, index) => {
                                const angle = (2 * Math.PI * index) / Math.max(items.length, 1);
                                const offset = type === 'memory' ? Math.PI / 8 : 0;

                                result.set(node.id, {
                                    ...node,
                                    x: Math.cos(angle + offset) * radius,
                                    y: Math.sin(angle + offset) * radius
                                });
                            });
                        }

                        return result;
                    }

                    function drawEdges(edges, nodes) {
                        for (const edge of edges) {
                            const source = nodes.get(edge.sourceId);
                            const target = nodes.get(edge.targetId);

                            if (!source || !target) {
                                continue;
                            }

                            const line = createSvg('line');
                            line.setAttribute('x1', source.x);
                            line.setAttribute('y1', source.y);
                            line.setAttribute('x2', target.x);
                            line.setAttribute('y2', target.y);
                            line.setAttribute('class', 'edge');
                            svg.appendChild(line);
                        }
                    }

                    function drawNodes(nodes) {
                        for (const node of nodes.values()) {
                            const group = createSvg('g');
                            group.setAttribute('class', 'node');
                            group.setAttribute('transform', `translate(${node.x}, ${node.y})`);

                            const circle = createSvg('circle');
                            circle.setAttribute('r', getNodeRadius(node.type));
                            circle.setAttribute('fill', colors[node.type] || '#94a3b8');

                            const text = createSvg('text');
                            text.setAttribute('text-anchor', 'middle');
                            text.setAttribute('y', getNodeRadius(node.type) + 15);
                            text.textContent = truncate(node.label, 28);

                            group.appendChild(circle);
                            group.appendChild(text);

                            group.addEventListener('click', () => selectNode(node));

                            svg.appendChild(group);
                        }
                    }

                    function selectNode(node) {
                        details.innerHTML = `
                            <strong>${escapeHtml(node.label)}</strong><br>
                            <span style="color: var(--muted)">Type:</span> ${escapeHtml(node.type)}<br>
                            <span style="color: var(--muted)">Id:</span> ${escapeHtml(node.id)}
                        `;

                        const relatedEdges = graph.edges.filter(edge =>
                            edge.sourceId === node.id || edge.targetId === node.id);

                        if (relatedEdges.length === 0) {
                            relations.innerHTML = '<span class="empty">No relations.</span>';
                            return;
                        }

                        relations.innerHTML = relatedEdges
                            .map(edge => {
                                const direction = edge.sourceId === node.id ? '→' : '←';
                                const otherId = edge.sourceId === node.id ? edge.targetId : edge.sourceId;
                                const otherNode = nodesById.get(otherId);

                                return `${direction} <strong>${escapeHtml(edge.type)}</strong><br>${escapeHtml(otherNode?.label ?? otherId)}`;
                            })
                            .join('<hr style="border:0;border-top:1px solid #374151;margin:10px 0">');
                    }

                    function getNodeRadius(type) {
                        if (type === 'memory') {
                            return 17;
                        }

                        if (type === 'file') {
                            return 9;
                        }

                        return 12;
                    }

                    function groupBy(items, keySelector) {
                        const map = new Map();

                        for (const item of items) {
                            const key = keySelector(item);

                            if (!map.has(key)) {
                                map.set(key, []);
                            }

                            map.get(key).push(item);
                        }

                        return map;
                    }

                    function createSvg(tagName) {
                        return document.createElementNS('http://www.w3.org/2000/svg', tagName);
                    }

                    function truncate(value, maxLength) {
                        if (!value || value.length <= maxLength) {
                            return value;
                        }

                        return `${value.substring(0, maxLength - 1)}…`;
                    }

                    function escapeHtml(value) {
                        return String(value)
                            .replaceAll('&', '&amp;')
                            .replaceAll('<', '&lt;')
                            .replaceAll('>', '&gt;')
                            .replaceAll('"', '&quot;')
                            .replaceAll("'", '&#039;');
                    }
                    </script>
                    </body>
                    </html>
                """;
    }

    private static string ExpandHomeDirectory(string path)
    {
        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[2..]);
        }

        return path;
    }
}
