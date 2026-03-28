---
name: vscode-extension
description: >-
  Build VS Code extensions with TypeScript. Covers extension anatomy, activation events,
  commands, tree views, webview panels, language features, testing, and publishing.
  Use when: creating a new VS Code extension, adding commands/views/providers, building
  webview UIs, implementing language server features, testing extensions, or packaging
  for the marketplace.
argument-hint: 'Describe the VS Code extension feature or capability to implement.'
---

# VS Code Extension Development

## When to Use

- Creating a new VS Code extension from scratch
- Adding commands, keybindings, or menu contributions
- Building tree views, sidebar panels, or webview UIs
- Implementing language features (completion, diagnostics, hover, CodeLens)
- Writing extension tests with `@vscode/test-electron`
- Packaging and publishing to the VS Code Marketplace

## Official Documentation

- [VS Code Extension API](https://code.visualstudio.com/api)
- [Extension Guides](https://code.visualstudio.com/api/extension-guides/overview)
- [VS Code API Reference](https://code.visualstudio.com/api/references/vscode-api)
- [Extension Manifest (package.json)](https://code.visualstudio.com/api/references/extension-manifest)
- [Activation Events](https://code.visualstudio.com/api/references/activation-events)
- [Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)

## Procedure

### Step 1 — Scaffold & Project Structure

Use `yo code` or manual setup. Follow this structure:

```
my-extension/
├── .vscode/
│   ├── launch.json          # Extension Host debug config
│   └── tasks.json           # Build tasks
├── src/
│   ├── extension.ts         # Entry point: activate() and deactivate()
│   ├── commands/             # Command handler implementations
│   ├── providers/            # TreeDataProvider, CompletionProvider, etc.
│   ├── views/                # Webview panel creation and messaging
│   ├── services/             # Business logic, API clients
│   └── utils/                # Helpers, constants
├── webview-ui/               # React/Svelte webview source (if applicable)
│   ├── src/
│   └── vite.config.ts
├── test/
│   ├── suite/                # Integration tests (Extension Host)
│   └── unit/                 # Pure unit tests (no VS Code API)
├── package.json              # Extension manifest + contributions
├── tsconfig.json
├── esbuild.config.mjs        # Bundler config
├── .vscodeignore              # Files to exclude from VSIX
├── CHANGELOG.md
└── README.md
```

**Key Rules:**
- `src/extension.ts` exports `activate(context)` and `deactivate()`.
- Register all disposables via `context.subscriptions.push(...)`.
- Keep `activate()` lean — lazy-initialize heavy resources.

### Step 2 — Extension Manifest (package.json)

```json
{
  "name": "my-extension",
  "displayName": "My Extension",
  "description": "A VS Code extension that does X",
  "version": "0.1.0",
  "publisher": "your-publisher-id",
  "engines": { "vscode": "^1.96.0" },
  "categories": ["Other"],
  "activationEvents": [],
  "main": "./dist/extension.js",
  "contributes": {
    "commands": [
      {
        "command": "myExtension.helloWorld",
        "title": "Hello World",
        "category": "My Extension"
      }
    ],
    "keybindings": [
      {
        "command": "myExtension.helloWorld",
        "key": "ctrl+shift+h",
        "mac": "cmd+shift+h"
      }
    ],
    "menus": {
      "editor/context": [
        { "command": "myExtension.helloWorld", "group": "navigation" }
      ]
    },
    "configuration": {
      "title": "My Extension",
      "properties": {
        "myExtension.enableFeature": {
          "type": "boolean",
          "default": true,
          "description": "Enable the main feature"
        }
      }
    }
  }
}
```

**Key Rules:**
- Use `activationEvents: []` for lazy activation (VS Code 1.74+).
- Prefix all commands with `extensionName.commandName`.
- Declare every command in both `commands` and `menus` as needed.
- Use `when` clauses for conditional command availability.

### Step 3 — Commands & Activation

```typescript
import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext): void {
    // Register commands
    context.subscriptions.push(
        vscode.commands.registerCommand('myExtension.helloWorld', async () => {
            const name = await vscode.window.showInputBox({
                prompt: 'Enter your name',
                placeHolder: 'World'
            });
            vscode.window.showInformationMessage(`Hello, ${name ?? 'World'}!`);
        })
    );

    // Register providers
    const treeProvider = new MyTreeDataProvider();
    context.subscriptions.push(
        vscode.window.registerTreeDataProvider('myExtension.treeView', treeProvider)
    );
}

export function deactivate(): void {
    // Cleanup resources if needed
}
```

**Key Rules:**
- Always push disposables to `context.subscriptions`.
- Use `async` commands with proper error handling.
- Show progress for long-running operations via `vscode.window.withProgress()`.
- Read configuration via `vscode.workspace.getConfiguration('myExtension')`.

### Step 4 — Tree Views

```typescript
import * as vscode from 'vscode';

export class MyTreeDataProvider implements vscode.TreeDataProvider<TreeItem> {
    private _onDidChangeTreeData = new vscode.EventEmitter<TreeItem | undefined>();
    readonly onDidChangeTreeData = this._onDidChangeTreeData.event;

    refresh(): void {
        this._onDidChangeTreeData.fire(undefined);
    }

    getTreeItem(element: TreeItem): vscode.TreeItem {
        return element;
    }

    async getChildren(element?: TreeItem): Promise<TreeItem[]> {
        if (!element) {
            // Root items
            return this.getRootItems();
        }
        // Child items
        return this.getChildItems(element);
    }

    private async getRootItems(): Promise<TreeItem[]> {
        return [
            new TreeItem('Item 1', vscode.TreeItemCollapsibleState.Collapsed),
            new TreeItem('Item 2', vscode.TreeItemCollapsibleState.None)
        ];
    }

    private async getChildItems(parent: TreeItem): Promise<TreeItem[]> {
        return [];
    }
}

class TreeItem extends vscode.TreeItem {
    constructor(
        public readonly label: string,
        public readonly collapsibleState: vscode.TreeItemCollapsibleState
    ) {
        super(label, collapsibleState);
    }
}
```

Declare in `package.json`:
```json
"contributes": {
    "views": {
        "explorer": [
            { "id": "myExtension.treeView", "name": "My Items" }
        ]
    },
    "viewsContainers": {
        "activitybar": [
            {
                "id": "myExtension-sidebar",
                "title": "My Extension",
                "icon": "resources/icon.svg"
            }
        ]
    }
}
```

### Step 5 — Webview Panels

```typescript
export class MyWebviewPanel {
    private panel: vscode.WebviewPanel | undefined;

    constructor(private readonly extensionUri: vscode.Uri) {}

    show(): void {
        if (this.panel) {
            this.panel.reveal();
            return;
        }

        this.panel = vscode.window.createWebviewPanel(
            'myExtension.panel',
            'My Panel',
            vscode.ViewColumn.One,
            {
                enableScripts: true,
                retainContextWhenHidden: true,
                localResourceRoots: [
                    vscode.Uri.joinPath(this.extensionUri, 'dist', 'webview')
                ]
            }
        );

        this.panel.webview.html = this.getHtml(this.panel.webview);

        // Handle messages from webview
        this.panel.webview.onDidReceiveMessage(
            (message: { command: string; data?: unknown }) => {
                switch (message.command) {
                    case 'save':
                        this.handleSave(message.data);
                        break;
                }
            },
            undefined,
            []
        );

        this.panel.onDidDispose(() => {
            this.panel = undefined;
        });
    }

    private getHtml(webview: vscode.Webview): string {
        const scriptUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.extensionUri, 'dist', 'webview', 'main.js')
        );
        const nonce = getNonce();

        return `<!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta http-equiv="Content-Security-Policy"
                  content="default-src 'none'; script-src 'nonce-${nonce}';">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body>
            <div id="root"></div>
            <script nonce="${nonce}" src="${scriptUri}"></script>
        </body>
        </html>`;
    }

    private handleSave(data: unknown): void {
        // Process save action
    }
}

function getNonce(): string {
    let text = '';
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    for (let i = 0; i < 32; i++) {
        text += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return text;
}
```

**Key Rules:**
- Always set `Content-Security-Policy` with nonce — never allow inline scripts.
- Use `localResourceRoots` to restrict file access.
- Communicate via `postMessage` / `onDidReceiveMessage` — no direct DOM access.
- Use `retainContextWhenHidden` sparingly — it increases memory usage.

### Step 6 — Language Features

```typescript
// Completion Provider
class MyCompletionProvider implements vscode.CompletionItemProvider {
    provideCompletionItems(
        document: vscode.TextDocument,
        position: vscode.Position
    ): vscode.CompletionItem[] {
        const item = new vscode.CompletionItem('mySnippet', vscode.CompletionItemKind.Snippet);
        item.insertText = new vscode.SnippetString('console.log($1);');
        item.documentation = new vscode.MarkdownString('Inserts a console.log statement');
        return [item];
    }
}

// Diagnostics
const diagnosticCollection = vscode.languages.createDiagnosticCollection('myExtension');

function updateDiagnostics(document: vscode.TextDocument): void {
    const diagnostics: vscode.Diagnostic[] = [];
    // Analyze document and add diagnostics
    const range = new vscode.Range(0, 0, 0, 10);
    diagnostics.push(new vscode.Diagnostic(range, 'Issue found', vscode.DiagnosticSeverity.Warning));
    diagnosticCollection.set(document.uri, diagnostics);
}

// Register
context.subscriptions.push(
    vscode.languages.registerCompletionItemProvider('typescript', new MyCompletionProvider()),
    diagnosticCollection
);
```

### Step 7 — Testing

**Integration Tests** (run in Extension Host):
```typescript
import * as assert from 'assert';
import * as vscode from 'vscode';

suite('Extension Test Suite', () => {
    vscode.window.showInformationMessage('Start all tests.');

    test('Command is registered', async () => {
        const commands = await vscode.commands.getCommands(true);
        assert.ok(commands.includes('myExtension.helloWorld'));
    });

    test('Command executes successfully', async () => {
        await vscode.commands.executeCommand('myExtension.helloWorld');
        // Verify side effects
    });
});
```

**Unit Tests** (no VS Code dependency):
```typescript
import { describe, it, expect } from 'vitest';
import { parseInput } from '../src/utils/parser';

describe('Parser', () => {
    it('should parse valid input', () => {
        const result = parseInput('test input');
        expect(result).toBeDefined();
    });
});
```

**Key Rules:**
- Use `@vscode/test-electron` for integration tests that need the Extension Host.
- Use Vitest for pure logic unit tests (no VS Code API dependency).
- Mock `vscode` namespace in unit tests when needed.
- Test both command registration and execution.

### Step 8 — Bundling & Publishing

Use **esbuild** for fast bundling:

```javascript
// esbuild.config.mjs
import * as esbuild from 'esbuild';

const production = process.argv.includes('--production');

await esbuild.build({
    entryPoints: ['src/extension.ts'],
    bundle: true,
    outfile: 'dist/extension.js',
    external: ['vscode'],
    format: 'cjs',
    platform: 'node',
    target: 'node20',
    sourcemap: !production,
    minify: production,
});
```

**Publishing Checklist:**
1. Update `version` in `package.json`
2. Update `CHANGELOG.md`
3. Run tests: `npm test`
4. Package: `vsce package`
5. Test VSIX locally: `code --install-extension my-extension-0.1.0.vsix`
6. Publish: `vsce publish`

**.vscodeignore:**
```
.vscode/**
src/**
test/**
webview-ui/src/**
node_modules/**
.gitignore
tsconfig.json
esbuild.config.mjs
**/*.map
```

## Quick Reference

| Concept | API |
|---------|-----|
| Show message | `vscode.window.showInformationMessage()` |
| Input box | `vscode.window.showInputBox()` |
| Quick pick | `vscode.window.showQuickPick()` |
| Progress | `vscode.window.withProgress()` |
| File picker | `vscode.window.showOpenDialog()` |
| Status bar | `vscode.window.createStatusBarItem()` |
| Output channel | `vscode.window.createOutputChannel()` |
| Read config | `vscode.workspace.getConfiguration()` |
| File system | `vscode.workspace.fs.readFile()` |
| Watch files | `vscode.workspace.createFileSystemWatcher()` |
| Decorations | `vscode.window.createTextEditorDecorationType()` |
| CodeLens | `vscode.languages.registerCodeLensProvider()` |
| Hover | `vscode.languages.registerHoverProvider()` |
| Definition | `vscode.languages.registerDefinitionProvider()` |

## Anti-Patterns

- **Never** block the extension host with synchronous operations — always use `async`.
- **Never** use `eval()` or `Function()` in webviews — use nonce-based CSP.
- **Never** store secrets in configuration — use `context.secrets` (SecretStorage API).
- **Never** bundle `node_modules` in the VSIX — use esbuild to bundle into a single file.
- **Never** use global state for communication between components — use event emitters.
