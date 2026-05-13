---
alwaysApply: true
---

# Git Workflow Rule

For every task or feature request, you must follow this sequence:

1. **Branching**: Before making any code changes, create a new local branch with a descriptive name (e.g., `feat/task-name` or `fix/issue-description`).
2. **Implementation**: Perform the requested changes only after the branch is created.
3. **Verification**: Run tests or build the project to ensure the changes work.
4. **Merging**: Once the task is complete and verified, attempt to merge the current branch back into the main development branch.
5. **Cleanup**: Delete the feature branch after a successful merge.
