# IDE/Editor Integration for YAML Schema Validation

> **Note:** This document has 0 references from other documentation and may be a candidate for archival or consolidation. Last reviewed: 2026-02-12.
>
> **Related:** This complements [community-tools.md](./community-tools.md) by providing IDE-specific setup for developers.

This document provides setup instructions for integrating YAML schema validation in various IDEs and editors.

## Overview

The project uses JSON Schema for YAML dialogue configuration validation. Schema files are located in `src/tools/schemas/` and should be associated with corresponding YAML files during editing for real-time validation and IntelliSense support.

**Primary Schema**: `src/tools/schemas/com.schema.json` - COM dialogue schema for files in `Game/data/**/*.yaml`

## VS Code Setup

### Automatic Setup (Workspace Configuration)

The repository includes `.vscode/settings.json` and `.vscode/extensions.json` for automatic configuration.

**Prerequisites**:
1. Install the recommended YAML Language Support extension:
   - Open VS Code in the workspace
   - Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (macOS)
   - Type "Extensions: Show Recommended Extensions"
   - Install "YAML Language Support by Red Hat" (`redhat.vscode-yaml`)

**Configuration** (already included in `.vscode/settings.json`):
```json
{
    "yaml.schemas": {
        "src/tools/schemas/com.schema.json": "Game/data/**/*.yaml"
    }
}
```

**Verification**:
1. Open any YAML file in `Game/data/` (e.g., `Game/data/coms/training/touch/caress.yaml`)
2. Hover over a property name (e.g., `id`, `trigger`)
3. IntelliSense tooltip should show schema documentation and type information
4. Add invalid syntax (e.g., `invalid: true`) to verify red underline appears

### Manual Setup

If workspace configuration is not available:

1. Install YAML Language Support extension: `redhat.vscode-yaml`
2. Open VS Code Settings (`Ctrl+,` or `Cmd+,`)
3. Search for "yaml.schemas"
4. Click "Edit in settings.json"
5. Add the schema mapping:
```json
{
    "yaml.schemas": {
        "/absolute/path/to/src/tools/schemas/com.schema.json": "Game/data/**/*.yaml"
    }
}
```

**Note**: Use absolute paths if workspace-relative paths don't work in your environment.

## IntelliJ IDEA Setup

### Plugin Installation

1. Open Settings/Preferences (`Ctrl+Alt+S` on Windows/Linux, `Cmd+,` on macOS)
2. Navigate to **Plugins**
3. Search for and install "YAML/Ansible support" (bundled plugin, usually pre-installed)

### Schema Association

1. Open Settings/Preferences
2. Navigate to **Languages & Frameworks** > **Schemas and DTDs** > **JSON Schema Mappings**
3. Click the `+` button to add a new schema mapping
4. Configure:
   - **Name**: COM Dialogue Schema
   - **Schema file or URL**: Browse to `src/tools/schemas/com.schema.json`
   - **Schema version**: JSON Schema version 7
5. In the **File path pattern** section, click `+` and add:
   - Pattern: `Game/data/**/*.yaml`
   - Pattern dialect: Glob
6. Click **OK** to save

### Verification

1. Open any YAML file in `Game/data/`
2. Hover over a property to see schema documentation
3. Use `Ctrl+Space` for auto-completion suggestions
4. Invalid syntax should show inline error highlights

## Troubleshooting

### VS Code: Schema not loading

**Symptoms**: No IntelliSense, no validation errors shown

**Solutions**:
1. **Check extension installed**: Verify "YAML Language Support by Red Hat" is installed and enabled
2. **Reload window**: Press `Ctrl+Shift+P` → "Developer: Reload Window"
3. **Check schema file path**: Ensure `src/tools/schemas/com.schema.json` exists
4. **Check YAML syntax**: Ensure the YAML file itself has valid basic syntax (invalid YAML prevents schema validation)
5. **Check output panel**: Open Output panel (`Ctrl+Shift+U`), select "YAML Support" from dropdown to see error messages
6. **Verify file pattern**: Ensure the YAML file path matches the glob pattern `Game/data/**/*.yaml`

### VS Code: Validation errors on valid YAML

**Symptoms**: Red underlines on correct YAML syntax

**Solutions**:
1. **Schema mismatch**: Verify the YAML file should use the COM schema
2. **Schema outdated**: Run `dotnet run --project tools/YamlSchemaGen` to regenerate schema
3. **Cache issue**: Delete `.vscode/.yaml-cache` if it exists and reload window
4. **Check schema version**: Ensure schema uses JSON Schema Draft 7 or compatible version

### IntelliJ IDEA: Schema not applied

**Symptoms**: No auto-completion or validation

**Solutions**:
1. **Check plugin enabled**: Verify YAML/Ansible support plugin is active
2. **Reindex project**: File > Invalidate Caches / Restart
3. **Check file pattern**: Ensure the glob pattern in schema mapping is correct
4. **Absolute vs relative paths**: Try using absolute path to schema file if relative path fails
5. **Schema format**: Ensure `com.schema.json` is valid JSON Schema (validate with online tools)

### Performance Issues

**Symptoms**: Editor lag when opening YAML files

**Solutions**:
1. **Large schema files**: Consider splitting schema into smaller, referenced schemas
2. **Too many files matching pattern**: Narrow the glob pattern if possible
3. **VS Code**: Increase memory limit in settings: `"yaml.maxItemsComputed": 5000` (default)
4. **IntelliJ IDEA**: Increase heap size in Help > Change Memory Settings

### Schema Changes Not Reflected

**Symptoms**: Old validation rules still applied after schema update

**Solutions**:
1. **VS Code**: Reload window (`Ctrl+Shift+P` → "Developer: Reload Window")
2. **IntelliJ IDEA**: Invalidate caches (File > Invalidate Caches / Restart)
3. **Verify schema file timestamp**: Ensure the schema file was actually updated
4. **Check for multiple schema files**: Ensure no conflicting schema files in the workspace

## Vim

### Plugin Installation

Vim supports YAML schema validation through Language Server Protocol (LSP) clients. Two main options are available:

**Option 1: coc.nvim (Recommended)**

1. Install [coc.nvim](https://github.com/neoclide/coc.nvim) following the plugin's installation instructions
2. Install coc-yaml extension:
   ```vim
   :CocInstall coc-yaml
   ```

**Option 2: ALE (Asynchronous Lint Engine)**

1. Install [ALE](https://github.com/dense-analysis/ale) using your plugin manager
2. Install yaml-language-server globally:
   ```bash
   npm install -g yaml-language-server
   ```

### Schema Association (coc.nvim)

1. Open Vim configuration: `:CocConfig`
2. Add schema mapping to the configuration:
```json
{
  "yaml.schemas": {
    "file:///absolute/path/to/src/tools/schemas/com.schema.json": "Game/data/**/*.yaml"
  }
}
```

**Note**: Replace `/absolute/path/to/` with the full path to your repository root. Vim requires absolute file URLs in the format `file:///path/to/schema.json`.

### Schema Association (ALE)

Add to your `.vimrc` or `init.vim`:
```vim
let g:ale_yaml_yamllint_options = '-d "{extends: default, rules: {line-length: disable}}"'
```

Create `.vim/coc-settings.json` in the project root:
```json
{
  "yaml.schemas": {
    "file:///absolute/path/to/src/tools/schemas/com.schema.json": "Game/data/**/*.yaml"
  }
}
```

### Verification

1. Open any YAML file in `Game/data/` (e.g., `Game/data/coms/training/touch/caress.yaml`)
2. Move cursor over a property name (e.g., `id`, `trigger`)
3. Use `:CocCommand document.showDocumentation` or press `K` (if mapped) to see schema documentation
4. Add invalid syntax (e.g., `invalid: true`)
5. Error should appear in the sign column and status line

## Emacs

### Plugin Installation

Emacs supports YAML schema validation through yaml-mode and flycheck with LSP support.

**Prerequisites**:
1. Install [yaml-mode](https://github.com/yoshiki/yaml-mode)
2. Install [flycheck](https://www.flycheck.org/)
3. Install [lsp-mode](https://emacs-lsp.github.io/lsp-mode/)
4. Install yaml-language-server globally:
   ```bash
   npm install -g yaml-language-server
   ```

**Using package.el**:
```elisp
M-x package-install RET yaml-mode RET
M-x package-install RET flycheck RET
M-x package-install RET lsp-mode RET
```

### Configuration

Add to your Emacs configuration file (`~/.emacs.d/init.el` or `~/.emacs`):

```elisp
;; Enable yaml-mode for YAML files
(require 'yaml-mode)
(add-to-list 'auto-mode-alist '("\\.yaml\\'" . yaml-mode))
(add-to-list 'auto-mode-alist '("\\.yml\\'" . yaml-mode))

;; Enable flycheck
(require 'flycheck)
(add-hook 'yaml-mode-hook 'flycheck-mode)

;; Enable lsp-mode for YAML
(require 'lsp-mode)
(add-hook 'yaml-mode-hook #'lsp)

;; Configure yaml-language-server schema association
(setq lsp-yaml-schemas
      '(:file:///absolute/path/to/src/tools/schemas/com.schema.json ["Game/data/**/*.yaml"]))
```

**Note**: Replace `/absolute/path/to/` with the full path to your repository root.

**Alternative: Project-local configuration**

Create `.dir-locals.el` in the repository root:
```elisp
((yaml-mode . ((lsp-yaml-schemas . ((:file:///absolute/path/to/src/tools/schemas/com.schema.json ["Game/data/**/*.yaml"]))))))
```

### Verification

1. Open any YAML file in `Game/data/`
2. Wait for LSP to connect (check mode line for `LSP` indicator)
3. Move cursor over a property name
4. Use `M-x lsp-describe-thing-at-point` to see schema documentation
5. Add invalid syntax (e.g., `invalid: true`)
6. Flycheck should highlight the error (red underline or fringe indicator)
7. Use `M-x flycheck-list-errors` to view all validation errors

## Sublime Text

### Package Installation

Sublime Text supports YAML schema validation through the LSP package and LSP-yaml.

**Prerequisites**:
1. Install [Package Control](https://packagecontrol.io/installation) if not already installed
2. Install yaml-language-server globally:
   ```bash
   npm install -g yaml-language-server
   ```

**Installing LSP packages**:
1. Open Command Palette (`Ctrl+Shift+P` on Windows/Linux, `Cmd+Shift+P` on macOS)
2. Type "Package Control: Install Package"
3. Search for and install:
   - **LSP** (Language Server Protocol support)
   - **LSP-yaml** (YAML language server)

### Schema Association

**Method 1: Project-specific configuration (Recommended)**

1. Open your project in Sublime Text
2. Go to **Project** > **Edit Project**
3. Add LSP settings to the project configuration:

```json
{
  "folders": [
    {
      "path": "."
    }
  ],
  "settings": {
    "LSP": {
      "LSP-yaml": {
        "settings": {
          "yaml.schemas": {
            "file:///C:/Era/erakoumakanNTR/src/tools/schemas/com.schema.json": "Game/data/**/*.yaml"
          }
        }
      }
    }
  }
}
```

**Note**: On Windows, use forward slashes in the file URI. Replace the path with your actual repository path.

**Method 2: Global configuration**

1. Open Command Palette
2. Type "Preferences: LSP-yaml Settings"
3. Add schema mapping:

```json
{
  "settings": {
    "yaml.schemas": {
      "file:///absolute/path/to/src/tools/schemas/com.schema.json": "Game/data/**/*.yaml"
    }
  }
}
```

### Verification

1. Open any YAML file in `Game/data/` (e.g., `Game/data/coms/training/touch/caress.yaml`)
2. Wait for LSP-yaml to initialize (check status bar for "LSP-yaml" indicator)
3. Hover over a property name to see schema documentation popup
4. Add invalid syntax (e.g., `invalid: true`)
5. Error should appear as red underline with diagnostic message in the status bar
6. Open **Tools** > **LSP** > **Toggle Diagnostics Panel** to view all validation errors

## Additional Editors

For other editors not covered in this document, most modern editors support YAML schema validation through:
- Language Server Protocol (LSP) with yaml-language-server
- Direct integration with YAML libraries that support JSON Schema

Refer to your editor's documentation for LSP setup instructions.

## References

- [YAML Language Support for VS Code](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml)
- [JSON Schema Specification](https://json-schema.org/)
- [IntelliJ IDEA YAML Support](https://www.jetbrains.com/help/idea/yaml.html)
- [coc.nvim](https://github.com/neoclide/coc.nvim)
- [ALE (Asynchronous Lint Engine)](https://github.com/dense-analysis/ale)
- [Emacs yaml-mode](https://github.com/yoshiki/yaml-mode)
- [Emacs lsp-mode](https://emacs-lsp.github.io/lsp-mode/)
- [Sublime LSP](https://lsp.sublimetext.io/)
- [yaml-language-server](https://github.com/redhat-developer/yaml-language-server)
