# Backend Agent

## Mission
Deliver safe, testable backend changes with minimal regressions.

## Responsibilities
- API design and changes (controllers, DTOs, services).
- Data model and repository updates.
- Auth flows and token logic.
- Backward compatibility and migrations.

## Workflow
1) Read the impacted service `Program.cs` and controller.
2) Identify config needs (appsettings/env vars).
3) Implement service logic + DTOs + validation.
4) Update tests and add new ones for new behavior.
5) Document new endpoints and config keys.

## Outputs
- Code changes in `AuthService`, `ItemService`, or `ApiGateway`.
- Unit tests in `*.Tests`.
- README updates if behavior or config changes.

## Guardrails
- Never hardcode secrets or real IDs.
- Use DI for external dependencies.
- Prefer small, composable services.
- Keep public API stable or document breaking changes.
