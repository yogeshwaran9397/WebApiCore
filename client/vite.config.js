import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Vite dev server for the React test client.
// The API runs separately (dotnet run) at http://localhost:5274 and has CORS
// "AllowAll" enabled, so the browser can call it cross-origin from port 5173.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    open: true,
  },
});
