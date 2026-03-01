// PM2 launcher for lsp-daemon.py
// Spawns: python -m uv run --with serena python tools/lsp-daemon.py
const { spawn } = require("child_process");
const path = require("path");

const PROJECT_ROOT = path.resolve(__dirname, "../../..");
const proc = spawn(
  "python",
  [
    "-m",
    "uv",
    "run",
    "--with",
    "git+https://github.com/oraios/serena",
    "python",
    path.join(PROJECT_ROOT, "src", "tools", "python", "lsp-daemon.py"),
  ],
  {
    cwd: PROJECT_ROOT,
    stdio: "inherit",
    env: { ...process.env, NO_PROXY: "127.0.0.1,localhost" },
    shell: true,
  }
);

proc.on("exit", (code) => process.exit(code ?? 1));
process.on("SIGINT", () => proc.kill("SIGINT"));
process.on("SIGTERM", () => proc.kill("SIGTERM"));
