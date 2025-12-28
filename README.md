# MulderConfig

MulderConfig is a small Windows launcher/configurator driven by a JSON file.
It can:
- apply file tweaks (copy/rename/delete/replace text)
- change game executable and/or add arguments
- provide a simple WinForms UI to pick options and save them per **Title**

## Quick start

1. Put `MulderConfig.json` next to the executable.
2. Run `MulderConfig.exe` for the UI.
3. Optional headless modes (requires existing save)
   - `MulderConfig.exe -apply`
   - `MulderConfig.exe -launch`

The app writes user selections to `MulderConfig.save.json`.

## Steam addons

This is mainly for Steam games that expose "official addons/DLC" through Steam launch options (for example: *Duke Nukem 3D: Megaton Edition*).
In that setup, when you start the game from Steam, Steam shows a launch options dialog and then starts the game with an extra argument like `-addon X` (where `X` is an integer).

With MulderConfig, you can define an `addons` list in `MulderConfig.json` and write different rules depending on which addon is selected.
If no addon matches (or if `-addon` is not provided), MulderConfig falls back to the base game title.

## Why exe replacement exists (Steam / launchers)

If your config contains at least one `actions.launch` rule, MulderConfig will perform an “exe replacement” step.
In practice, it moves the original game executable aside (renaming it to `*_o.exe`) and copies `MulderConfig.exe` in its place.

The goal is to keep launching the game from Steam (or another launcher that expects the original executable) while still applying your configuration.
This help to keep Steam features such as playtime tracking, overlay, and of course Steam achievements.

If your config has no `launch` rules, MulderConfig will not replace the executable with **Apply**.

## Writing MulderConfig.json

Top-level structure:

```json
{
  "game": { "title": "...", "originalExe": "..." },
  "addons": [ { "title": "...", "steamId": 123 } ],
  "optionGroups": [ ... ],
  "actions": {
    "operations": [ ... ],
    "launch": [ ... ]
  }
}
```

Validation rules (important):
- `addons` is optional.
- `actions.operations` and `actions.launch` are optional individually.
- But you must have **at least one action**: `launch` OR `operations` must contain at least one item.

### Minimal example

```json
{
  "game": { "title": "Fallout", "originalExe": "Fallout.exe" },
  "optionGroups": [
    {
      "name": "Renderer",
      "type": "radioGroup",
      "radios": [ { "value": "DX9" }, { "value": "DX11" } ]
    }
  ],
  "actions": {
    "launch": [
      {
        "when": [ { "Renderer": "DX11" } ],
        "exec": { "name": "FalloutDX11.exe", "workDir": ".\\" },
        "args": [ "-novsync" ]
      }
    ]
  }
}
```

## `when` / `disabledWhen`

`when` is used in `actions.launch` and `actions.operations`.
`disabledWhen` is used in `optionGroups` to disable a radio/checkbox.

Logic:
- list of groups = **OR**
- inside a group = **AND**
- missing/empty list = “always apply”

### Operators

The operator is encoded in the key prefix:
- `Key: "Value"` → equals (case-insensitive)
- `!Key: "Value"` → notEquals
- `*Key: "Value"` → contains (substring)
- `!*Key: "Value"` → notContains

`Title` is always available to `when`.

Example:

```json
{ "when": [ { "Renderer": "DX11" }, { "!Title": "Vanilla" } ] }
```

### Checkbox lists

Checkbox groups are stored as a list of strings:
- `Key` / `!Key` behaves like “list contains value” / “list does not contain value”
- `*Key` / `!*Key` behaves like “any item contains substring” / “no item contains substring”

Special case: `Key: ""` matches when the selection is empty (null/empty list).

Missing keys:
- `Key` / `*Key` cannot match
- `!Key` / `!*Key` is treated as true

## actions.operations

Operations run in JSON order (optional `when`). Supported operations:
- `rename` / `move` (requires `source`, `target`)
- `copy` (requires `source`, `target`)
- `delete` (requires `source`)
- `setReadOnly` (requires `files`)
- `removeReadOnly` (requires `files`)
- `replaceLine` (requires `files`, `pattern`, `replacement`)
- `removeLine` (requires `files`, `pattern`)
- `replaceText` (requires `files`, `search`, `replacement`)

Paths are relative to the game directory (app startup directory). Windows environment variables like `%USERPROFILE%` are expanded.

## actions.launch

Launch rules are evaluated in JSON order:
- `args` are cumulative (appended)
- `exec` is atomic and last-match-wins (exe + workDir together)

Defaults if no exec matches:
- exe: `game.originalExe`
- workDir: app startup directory

