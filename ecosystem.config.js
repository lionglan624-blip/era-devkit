module.exports = {
  apps: [
    {
      name: "lsp-daemon",
      script: "src/tools/node/lsp-daemon-launcher.cjs",
      cwd: process.env.DEVKIT_ROOT || "C:\\Era\\erakoumakanNTR",
      autorestart: true,
      max_restarts: 3,
      restart_delay: 5000,
      kill_timeout: 5000,
      treekill: true,
      watch: false,
      error_file: "_out/tmp/lsp-daemon-error.log",
      out_file: "_out/tmp/lsp-daemon-out.log",
      merge_logs: true,
      env: {
        NO_PROXY: "127.0.0.1,localhost",
      },
    },
  ],
};
