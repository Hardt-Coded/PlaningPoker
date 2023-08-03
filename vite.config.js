import { defineConfig } from 'vite'
import commonjs from 'vite-plugin-commonjs'
import react from '@vitejs/plugin-react'

/** @type {import('vite').UserConfig} */
export default defineConfig({
    plugins: [
        commonjs(),
        react()
    ],
    root: "./src/Client",
    server: {
        port: 8080,
        proxy: {
            '/api': 'http://localhost:7071',
        }
    },
    build: {
        outDir: "../../publish/app-fe"
    },
    define: {
        global: {}
    },
    optimizeDeps: {

    }

})