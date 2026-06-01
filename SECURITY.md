# Security Policy

## Supported versions

DevMemory is currently a personal open-source project in early development.

The latest GitHub Release is the only supported public release.

| Version | Supported |
| ------- | --------- |
| latest  | Yes       |
| older   | No        |

---

## Reporting a vulnerability

If you find a security issue, please do not open a public issue with sensitive details.

Instead, contact the maintainer privately through GitHub or LinkedIn.

Please include:

- affected version or commit;
- operating system;
- command or workflow involved;
- impact description;
- reproduction steps if possible;
- whether local files, environment variables, AI runtime or external services are involved.

---

## Security model

DevMemory is local-first.

By default:

- primary memory data is stored locally;
- core memory commands do not require cloud services;
- AI/RAG features are optional;
- local AI can run through Ollama;
- vector data can be stored locally through Qdrant.

Default local storage:

```text
~/.devmemory/devmemory.json
```

If `DEVMEMORY_HOME` is set, DevMemory uses that directory instead.

---

## Sensitive data guidance

DevMemory can store technical context written by the user.

Avoid storing:

- secrets;
- passwords;
- API keys;
- private customer data;
- confidential source snippets;
- personal data not needed for technical memory.

Before sharing logs, issues or screenshots, review them for sensitive information.

---

## External services

Core local memory features do not require external services.

Optional AI/RAG features may interact with configured local or external providers depending on environment variables and configuration.

Configuration precedence:

```text
Environment variables > ~/.devmemory/config.json > default values
```

Review your configuration before enabling non-local providers.

---

## Known limitations

Current limitations:

- primary storage is JSON-based;
- no encryption-at-rest is implemented by DevMemory itself;
- access control is delegated to local operating-system permissions;
- local AI/RAG quality and privacy depend on selected providers and configuration.

Use appropriate filesystem permissions and avoid storing secrets in DevMemory memories.