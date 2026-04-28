# 🤖 LLM Coding Benchmark

A structured benchmark comparing how different LLMs handle a greenfield full-stack coding task. Each model receives the **exact same prompts** and produces a complete, runnable system.

## 📋 The Prompt

The benchmark prompt asks models to build a **Task Management System** from scratch:

- **Backend:** C# .NET 8, ASP.NET Core, Entity Framework Core, PostgreSQL
- **Frontend:** React 18, TypeScript, Vite, Tailwind CSS, React Query
- **Infrastructure:** Docker, Docker Compose (`docker-compose up` → working app)
- **Tests:** xUnit + Moq (backend), Vitest + React Testing Library (frontend)

The full prompt is in [`PROMPT.md`](./PROMPT.md) and the iterative debugging prompt is in [`PROMPT-2.md`](./PROMPT-2.md).

---

## 🗂️ Repository Structure

```
llm-benchmark/
│
├── README.md               ← You are here
├── PROMPT.md               ← The exact prompt given to every model
├── PROMPT-2.md             ← The iterative debugging prompt used after initial run
├── SCORING.md              ← Scoring rubric for manual review
│
└── results/
    ├── github-copilot/
    │   ├── vscode-gpt-4-1/
    │   │   └── 2026-04-28/
    │   ├── vscode-claude-sonnet-4-5/
    │   │   └── 2026-04-28/
    │   └── vscode-o3/
    │       └── 2026-04-28/
    ├── claude/
    │   ├── web-claude-opus-4-5/
    │   │   └── 2026-04-28/
    │   └── web-claude-sonnet-4-5/
    │       └── 2026-04-28/
    ├── opencode-zen/
    │   └── cli-claude-opus-4-5/
    │       └── 2026-04-28/
    ├── cursor/
    │   └── ide-gpt-4-1/
    │       └── 2026-04-28/
    └── _template/
        ├── README.md           ← How to fill out a result entry
        ├── review.md           ← Blank scoring sheet
        ├── details.md          ← Additional context/notes (optional)
        └── chat-history.md     ← Full chat log (if available)
```

Each result entry lives at: `results/{tool}/{client}-{model}/{date}/`

---

## 🧪 Models Tested

| Tool | Client | Model | Status |
|---|---|---|---|
| GitHub Copilot | VS Code | gpt-4.1 | ⏳ Pending |
| GitHub Copilot | VS Code | claude-sonnet-4.5 | ⏳ Pending |
| GitHub Copilot | VS Code | o3 | ⏳ Pending |
| Claude | Web | claude-opus-4.5 | ⏳ Pending |
| Claude | Web | claude-sonnet-4.5 | ⏳ Pending |
| OpenCode Zen | CLI | claude-opus-4.5 | ⏳ Pending |
| Cursor | IDE | gpt-4.1 | ⏳ Pending |

---

## 📊 Results Summary

> Updated manually after each review.

| Tool | Model | Compiles | docker-compose up | Tests Pass | Score /100 |
|---|---|---|---|---|---|
| — | — | — | — | — | — |

---

## 🔍 How to Add a Result

1. Copy `results/_template/` into the correct `results/{tool}/{model}/{date}/` path
2. Run the prompt, collect the output
3. Run the second prompt for debugging until all success conditions are met (compiles, `docker-compose up`, tests)
4. Place all generated files inside a `src/` subfolder
5. Fill out `review.md` based on the scoring rubric in `SCORING.md`
6. If the tool allows exporting the conversation, save it as `chat-history.md` _(optional but encouraged)_
7. Optionally add a `details.md` with any additional context (e.g. model settings, observations, quirks)
8. Update the summary table in this README
9. Open a PR or push directly

See [`results/_template/README.md`](./results/_template/README.md) for detailed instructions.

---

## ⚖️ Scoring Rubric (Summary)

Full rubric in [`SCORING.md`](./SCORING.md).

| Category | Points |
|---|---|
| Compiles without errors | 15 |
| `docker-compose up` works end-to-end | 20 |
| Backend unit tests pass | 15 |
| Frontend tests pass | 10 |
| Architecture consistency | 10 |
| Business logic correctness | 10 |
| TypeScript type safety (no `any`) | 5 |
| HTTP status codes correct | 5 |
| Documentation quality | 5 |
| Assumptions section quality | 5 |
| **Total** | **100** |

---

## 📌 Ground Rules

- The prompt is **never modified** between runs
- No follow-up messages — given prompts only
- Models are tested in their default configuration (no system prompt changes)
- Results reflect the output **as-is** — no manual fixing before review
- All generated code is committed verbatim

---

## 📅 Methodology

1. Open a **fresh session** (no prior context)
2. Paste [`PROMPT.md`](./PROMPT.md) verbatim
3. Wait for full completion (no interruption)
4. Run the second prompt from [`PROMPT-2.md`](./PROMPT-2.md) iteratively until all success conditions are met
4. Save all output files into `results/{tool}/{model}/src/`
5. Attempt `docker-compose up` and record outcome
6. Run backend and frontend tests separately
7. Fill out `review.md`

---

## 🤝 Contributing

Want to add a model or tool? Follow the steps in [How to Add a Result](#-how-to-add-a-result) and open a PR. Please do not edit another reviewer's `review.md`.