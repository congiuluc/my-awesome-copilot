---
description: "Use when building VS Code extensions. Covers extension anatomy, commands, tree views, webview panels, language features, CSP security, testing, bundling, and publishing."
applyTo: "src/vscode-extension/**,**/extension.ts,**/.vscodeignore"
---
# VS Code Extension Guidelines

## Extension Entry Point

- Export `activate(context)` and `deactivate()` from `src/extension.ts`.
- Push **all disposables** to `context.subscriptions` — no manual cleanup leaks.
- Keep `activate()` lean — lazy-initialize heavy resources on first use.

## Command Registration

- Prefix all commands: `extensionName.commandName`.
- Declare commands in both `package.json` contributes and register at runtime.
- Use `when` clauses for conditional command availability.
- Handle errors in every command — show user-friendly messages.

```typescript
context.subscriptions.push(
    vscode.commands.registerCommand('myExt.doWork', async () => {
        try {
            await performWork();
        } catch (err) {
            vscode.window.showErrorMessage(`Failed: ${(err as Error).message}`);
        }
    })
);
```

## Webview Security

- **Always** set `Content-Security-Policy` with nonce on webview HTML.
- Use `localResourceRoots` to restrict file access to extension dist folder.
- Communicate via `postMessage` / `onDidReceiveMessage` only.
- **Never** use `eval()` or inline scripts in webviews.
- Store secrets with `context.secrets` (SecretStorage API), not configuration.

## Tree Views

- Implement `TreeDataProvider<T>` with `getTreeItem()` and `getChildren()`.
- Use `EventEmitter` for `onDidChangeTreeData` to trigger refreshes.
- Declare views in `package.json` under `contributes.views`.

## Testing

- **Integration tests**: Use `@vscode/test-electron` — run in Extension Host.
- **Unit tests**: Use Vitest for pure logic — no VS Code API dependency.
- Test command registration, execution, and side effects.
- Mock `vscode` namespace only in unit tests.

## Bundling

- Use **esbuild** — mark `vscode` as external.
- Output: single `dist/extension.js` file (CommonJS, Node target).
- Minify for production, include sourcemaps for development.
- `.vscodeignore`: exclude `src/`, `test/`, `node_modules/`, `*.map`.

## Publishing

1. Bump version in `package.json`
2. Update `CHANGELOG.md`
3. Run full test suite
4. `vsce package` → test VSIX locally → `vsce publish`
