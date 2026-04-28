# 📊 Scoring Rubric

All categories are scored during manual review. The reviewer fills out `review.md` inside each result entry.

---

## Categories

### 1. Compiles Without Errors — 15 pts

| Score | Criteria |
|---|---|
| 15 | Backend builds, frontend builds, no errors |
| 10 | One side compiles, the other has minor fixable errors |
| 5 | Significant compile errors but structure is sound |
| 0 | Does not compile at all |

### 2. `docker-compose up` Works End-to-End — 20 pts

| Score | Criteria |
|---|---|
| 20 | App starts, all containers healthy, reachable at `localhost:3000` |
| 15 | Starts but requires one manual fix (e.g. env var, port) |
| 10 | Partially starts (e.g. DB + API but no frontend) |
| 5 | Containers defined but none start successfully |
| 0 | No `docker-compose.yml` or completely broken |

### 3. Backend Unit Tests Pass — 15 pts

| Score | Criteria |
|---|---|
| 15 | All tests pass, good coverage of happy path + edge cases |
| 10 | Most tests pass, some missing cases |
| 5 | Tests exist but several fail |
| 0 | No tests or none run |

### 4. Frontend Tests Pass — 10 pts

| Score | Criteria |
|---|---|
| 10 | All tests pass, covers required components |
| 7 | Most pass, minor issues |
| 3 | Tests exist but fail |
| 0 | No tests |

### 5. Architecture Consistency — 10 pts

Does the model follow a clean layered architecture (Domain → Application → Infrastructure → Api)?

| Score | Criteria |
|---|---|
| 10 | Perfect separation, repository pattern, service layer, correct DI wiring |
| 7 | Mostly correct, minor violations (e.g. EF in controller) |
| 4 | Some structure but inconsistent or mixed layers |
| 0 | No discernible architecture |

### 6. Business Logic Correctness — 10 pts

Covers: ownership checks, soft deletes, `EditedAt` updates, `UpdatedAt` on task modification.

| Score | Criteria |
|---|---|
| 10 | All business rules implemented correctly |
| 7 | Most rules correct, 1-2 missing |
| 4 | Several rules missing or wrong |
| 0 | Business logic essentially absent |

### 7. TypeScript Type Safety — 5 pts

| Score | Criteria |
|---|---|
| 5 | No `any`, correct generics throughout |
| 3 | Minimal `any` usage (1-2 instances) |
| 1 | Frequent `any` or implicit `any` |
| 0 | Barely typed |

### 8. HTTP Status Codes — 5 pts

| Score | Criteria |
|---|---|
| 5 | All endpoints return correct codes (200, 201, 204, 400, 401, 403, 404) |
| 3 | Mostly correct, 1-2 wrong |
| 1 | Generic 200/500 everywhere |
| 0 | No attention to status codes |

### 9. Documentation Quality — 5 pts

Covers: file-level comments, inline comments on non-obvious logic, README.

| Score | Criteria |
|---|---|
| 5 | README present and complete, file-level comments on all files, inline comments where appropriate |
| 3 | Partial documentation |
| 1 | Minimal or boilerplate only |
| 0 | No documentation |

### 10. Assumptions Section Quality — 5 pts

| Score | Criteria |
|---|---|
| 5 | Comprehensive, covers all ambiguous decisions with clear reasoning |
| 3 | Exists but incomplete |
| 1 | Barely present |
| 0 | Missing |

---

## Total: 100 pts

| Range | Rating |
|---|---|
| 90–100 | ⭐⭐⭐⭐⭐ Excellent |
| 75–89 | ⭐⭐⭐⭐ Good |
| 55–74 | ⭐⭐⭐ Acceptable |
| 35–54 | ⭐⭐ Poor |
| 0–34 | ⭐ Failing |