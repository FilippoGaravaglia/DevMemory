# DevMemory local demo

DevMemory includes an isolated local demo script that lets you try the CLI without touching your real local data.

The demo is designed for reviewers, recruiters, contributors, and anyone who wants to quickly understand what DevMemory does.

## Why an isolated demo?

DevMemory is a local-first CLI tool.

By default, real user data is stored under:

```text
~/.devmemory
```

The demo does **not** use that directory.

Instead, it creates a temporary `DEVMEMORY_HOME` under the system temporary folder, seeds it with sample memories, runs several commands, and deletes the temporary data automatically at the end.

This means you can safely run the demo without modifying your real DevMemory storage.

## Run the demo

From the repository root:

```bash
./scripts/demo-local.sh
```

The script will:

1. create a temporary isolated `DEVMEMORY_HOME`;
2. seed demo memories into local JSON storage;
3. run general diagnostics with `devmemory doctor`;
4. list saved memories;
5. run keyword search;
6. show a memory in detail;
7. print a chronological timeline;
8. edit a memory;
9. export the knowledge graph as JSON;
10. generate the local HTML graph view;
11. optionally run local AI commands if Ollama and Qdrant are available.

## Keep demo data for inspection

By default, demo data is deleted automatically when the script exits.

To keep the generated temporary data:

```bash
DEVMEMORY_KEEP_DEMO_HOME=true ./scripts/demo-local.sh
```

At the end of the execution, the script prints the temporary demo directory path.

You can inspect:

```text
devmemory.json
markdown/
graph/
```

This is useful when you want to inspect the generated Markdown exports or the graph output.

## What the demo shows

### General diagnostics

The demo starts with:

```bash
devmemory doctor
```

This command checks the general health of the local DevMemory environment:

```text
Storage
Markdown directory
Persistent configuration
AI runtime configuration
Git availability
```

The result may be:

```text
ready
attention required
failed
```

`attention required` is not necessarily an error. For example, the demo may show attention if AI providers are not configured.

### Listing memories

The demo runs:

```bash
devmemory list
```

This shows the seeded memories sorted by creation date.

Each memory contains structured technical context:

```text
Title
Project
Area
Branch
Tags
Problem
Solution
Decisions
Files touched
Tests
Lessons learned
Created date
```

### Keyword search

The demo runs:

```bash
devmemory search "qdrant"
```

This tests the local keyword/ranked search over the JSON source of truth.

This part does not require AI, embeddings, Ollama, or Qdrant.

### Show memory details

The demo runs:

```bash
devmemory show 7340ac82-4ed6-41b1-b790-e15edfaf39b4
```

This prints the full structured memory.

This is useful when you want to recover the context of a past task.

### Timeline

The demo runs:

```bash
devmemory timeline --project DevMemory --limit 10
```

This shows saved memories as a chronological timeline.

The timeline is useful to understand project evolution over time.

It is also one of the best commands to use in screenshots because it makes the value of DevMemory immediately visible.

### Edit memory

The demo runs:

```bash
devmemory edit bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb --add-tag demo --solution "Updated during the isolated local demo."
```

This demonstrates that memories are not immutable notes.

They can be updated as knowledge evolves.

After the edit, the demo runs:

```bash
devmemory show bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb
```

to verify that the memory was updated.

### Graph export

The demo runs:

```bash
devmemory graph-export
```

This generates a graph representation of the stored memories.

The graph contains nodes and edges derived from memory metadata such as projects, areas, tags, files, and relationships.

### Graph view

The demo runs:

```bash
devmemory graph-view
```

This generates a local HTML graph view.

The graph view is useful to visually inspect the structure of developer knowledge captured by DevMemory.

## Optional local AI section

The final part of the demo tries to run AI-related commands.

It uses:

```bash
DEVMEMORY_CHAT_PROVIDER=ollama
DEVMEMORY_EMBEDDING_PROVIDER=ollama
DEVMEMORY_VECTOR_STORE=qdrant
```

and then attempts to run:

```bash
devmemory index --limit 4
devmemory semantic-search "local AI runtime qdrant" --limit 3
devmemory related 7340ac82-4ed6-41b1-b790-e15edfaf39b4 --limit 3
devmemory ask --rag "How did we validate the local AI runtime?" --limit 3
```

These commands require:

```text
Ollama running locally
Qdrant running locally
Required Ollama models pulled
```

If Qdrant or Ollama are not running, the demo skips the AI section gracefully.

This is expected and does not mean the demo failed.

## Start the local AI runtime

To run the full AI section, first start the local AI runtime:

```bash
./scripts/start-ai-local.sh
```

Then pull the required models if needed:

```bash
./scripts/pull-ollama-models-local.sh
```

Then rerun the demo:

```bash
./scripts/demo-local.sh
```

## What this demo proves

The local demo proves that DevMemory can:

```text
Capture structured developer knowledge
Store memories locally
Search saved memories
Edit saved memories
Display timeline history
Export Markdown artifacts
Generate graph data
Generate an HTML graph view
Optionally index memories into Qdrant
Optionally run semantic search and RAG locally
```

It also proves that DevMemory can be demonstrated safely without touching real user data.

## Recommended demo flow for screenshots

For a GitHub README or LinkedIn post, the most useful command to capture is:

```bash
./scripts/demo-local.sh
```

You can also run individual commands:

```bash
devmemory doctor
devmemory timeline --project DevMemory --limit 10
devmemory search "qdrant"
devmemory show <memory-id>
devmemory graph-view
```

With Ollama and Qdrant running:

```bash
devmemory semantic-search "local AI runtime qdrant" --limit 3
devmemory related <memory-id> --limit 3
devmemory ask --rag "How did we validate the local AI runtime?" --limit 3
```

Remember to replace `<memory-id>` with a real memory id returned by:

```bash
devmemory list
```

## Troubleshooting

### The AI section is skipped

This usually means Qdrant or Ollama are not running.

Start the local AI runtime:

```bash
./scripts/start-ai-local.sh
```

Then rerun:

```bash
./scripts/demo-local.sh
```

### Qdrant connection refused

If you see an error like:

```text
Connection refused (localhost:6333)
```

Qdrant is not running.

Start it with:

```bash
./scripts/start-ai-local.sh
```

### Ollama model missing

If Ollama is running but a model is missing, pull the local models:

```bash
./scripts/pull-ollama-models-local.sh
```

### I want to inspect generated files

Run:

```bash
DEVMEMORY_KEEP_DEMO_HOME=true ./scripts/demo-local.sh
```

Then inspect the printed temporary directory.

## Notes for contributors

The demo intentionally uses an isolated `DEVMEMORY_HOME`.

This makes the script safe to run locally and suitable for documentation, onboarding, and portfolio demonstrations.

When adding new CLI features, consider whether they should be included in the demo flow.

A good demo command should be:

```text
safe
repeatable
easy to understand
useful without private user data
```
