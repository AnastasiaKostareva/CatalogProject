import { defineConfig } from 'vite';

export default defineConfig({
    root: '.',
    server: {
        port: 5173,
        open: true,
        proxy: {
            '/api': {
                target: 'http://localhost:5151',
                changeOrigin: true
            }
        }
    },
    build: {
        outDir: 'dist',
        emptyOutDir: true
    }
});