/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_USER_SERVICE_URL: string
  readonly VITE_CHAT_SERVICE_URL: string
  readonly VITE_TASK_SERVICE_URL: string
  readonly VITE_SIGNALR_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
