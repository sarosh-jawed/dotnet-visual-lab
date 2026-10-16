# DotNetScope lab documentation

Each lab has a dedicated guide in `docs/labNN/README.md`. The repository root README provides the project overview and visual previews.

| Lab | Topic | Guide | Screenshots |
| --- | --- | --- | --- |
| 01 | Dependency Injection Lifetimes | [Lab 01](lab01/README.md) | [Overview](images/lab01-overview.png) |
| 02 | Strategy Pattern: Document Export Router | [Lab 02](lab02/README.md) | [Overview](images/lab02-overview.png), [Full page](images/lab02-full.png) |
| 03 | Middleware Flight Recorder | [Lab 03](lab03/README.md) | [Seven captures](lab03/screenshots/) |

## Asset organization

The original Lab 01 and Lab 02 screenshots remain in `docs/images/` to preserve links already published in previous README versions. They are not copied or moved. Lab 03, the first lab with a full image walkthrough, stores screenshots alongside its guide in `docs/lab03/screenshots/`.

Future labs should use `docs/labNN/README.md` with any new screenshots in `docs/labNN/screenshots/`. This avoids disrupting historical links while keeping future documentation consistent.

## Principles

- Explain the learning goal and the real runtime behavior.
- Separate verified behavior from unimplemented or illustrative examples.
- Keep a concise project-level preview and a focused guide for each lab.
- Preserve existing screenshot URLs unless a migration plan protects external references.
- Run the Release build and tests before merging documentation changes.
