# Result Entry: {tool} / {client}-{model}

This folder contains the benchmark result for a single tool + model combination.

## Folder Structure

```
{client}-{model}/
├── README.md           ← This file
├── review.md           ← Filled scoring sheet (required)
├── details.md          ← Observations and context (optional)
├── chat-history.md     ← Full chat log export (optional)
├── backend/            ← Generated backend code
└── frontend/           ← Generated frontend code
```

## How to Fill This Out

1. **Run the benchmark**
   - Open a fresh session (no prior context)
   - Paste [`PROMPT.md`](../../PROMPT.md) verbatim
   - Wait for full completion — do not interrupt
   - Run [`PROMPT-2.md`](../../PROMPT-2.md) iteratively until success conditions are met

2. **Save the generated code**
   - Place all generated files directly in this folder (backend/ and frontend/ at the root)

3. **Verify success conditions**
   - `docker-compose up --build` starts successfully
   - Backend compiles: `dotnet build`
   - Backend tests pass: `dotnet test`
   - Frontend compiles: `npm run build`
   - Frontend tests pass: `npm run test`

4. **Fill out `review.md`**
   - Score each category honestly based on [`SCORING.md`](../../SCORING.md)
   - Add brief notes for each score to justify the rating

5. **Optional extras**
   - Export the conversation and save it as `chat-history.md`
   - Add a `details.md` with observations, quirks, or model settings used

6. **Update the summary table** in the root [`README.md`](../../README.md)
