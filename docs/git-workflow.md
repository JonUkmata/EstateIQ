# Git Workflow for EstateIQ

This document describes the Git branching strategy and workflow for the EstateIQ project.

## Branches

- **main**: Production-ready, stable code only. Deployments are made from this branch.
- **dev**: Integration branch for development. All features are merged here before going to main.
- **initials/***: Each new feature or fix should be developed in its own branch, named as `INICIALET/ticket-title` (p.sh. `AB/property-listing`).

## Workflow

1. **Create a feature branch**
   ```sh
   git checkout dev
   git pull
   git checkout -b INICIALET/ticket-title
   ```

2. **Work and commit**
   - Make changes and commit regularly with clear messages:
     ```sh
     git add .
     git commit -m "Short, clear description of the change"
     ```

3. **Push your branch**
   ```sh
   git push -u origin INICIALET/ticket-title
   ```

4. **Open a Pull Request (PR) to dev**
   - Go to GitHub and open a PR from your `INICIALET/ticket-title` branch into `dev`.
   - Request review and address feedback.

5. **Merge dev to main**
   - When `dev` is stable and ready for release, open a PR from `dev` to `main` and merge after review.

## Example Commands

- Create a new feature branch:
  ```sh
  git checkout dev
  git pull
   git checkout -b AB/property-listing
  ```
- Commit changes:
  ```sh
  git add .
  git commit -m "Add property listing page"
  ```
- Push branch:
  ```sh
   git push -u origin AB/property-listing
  ```
- Open a PR on GitHub from `feature/property-listing` to `dev`.
   # Open a PR on GitHub from `AB/property-listing` to `dev`.

## Notes
- Always keep your branches up to date with `dev`.
- Use descriptive branch names.
- Do not commit directly to `main` or `dev`.
- All code must be reviewed before merging.

---

*This workflow ensures a clean, collaborative, and stable development process for EstateIQ.*
