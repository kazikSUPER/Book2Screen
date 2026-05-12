# Contributing to Book2Screen

Welcome to the Book2Screen project! To maintain code quality and ensure smooth collaboration, please follow these guidelines.

## Git Workflow

1.  **Main Branches:**
    *   `main`: Production-ready code only. Direct commits are forbidden.
    *   `develop`: Integration branch for features. All Pull Requests should target this branch.
2.  **Feature Branches:** Create a new branch for every task or bug fix.
3.  **Pull Requests:**
    *   Target the `develop` branch.
    *   Must be reviewed and approved by at least one other team member.
    *   Ensure all tests pass before requesting a review.

## Branch Naming Convention

Follow the format: `type/short-description`

*   `feat/` — for new features (e.g., `feat/auth-service`)
*   `fix/` — for bug fixes (e.g., `fix/db-connection`)
*   `docs/` — for documentation changes (e.g., `docs/api-endpoints`)
*   `refactor/` — for code refactoring
*   `test/` — for adding or updating tests

## Commit Message Format

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

`<type>: <description>`

**Common types:**
*   `feat`: A new feature
*   `fix`: A bug fix
*   `docs`: Documentation only changes
*   `style`: Changes that do not affect the meaning of the code (white-space, formatting, etc.)
*   `refactor`: A code change that neither fixes a bug nor adds a feature
*   `test`: Adding missing tests or correcting existing tests
*   `chore`: Changes to the build process or auxiliary tools and libraries

**Example:**
`feat: implement JWT authentication logic`

## Development Standards

*   Follow the project's `.editorconfig` and `stylecop.json` settings.
*   Ensure every new feature includes relevant Unit Tests.
*   Document public APIs and complex logic using XML comments (for C#) or JSDoc (for TS).
