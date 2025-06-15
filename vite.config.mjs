import path from "node:path";
import { fileURLToPath } from "node:url";
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fable from "vite-plugin-fable";
import postcssplugin from '@tailwindcss/postcss'

const currentDir = path.dirname(fileURLToPath(import.meta.url));
const fsproj = path.join(currentDir, "src/PlaningPoker.Client/PlaningPoker.Client.fsproj");
console.log(`Using fsproj: ${fsproj}`);


/** @type {import('vite').UserConfig} */
export default defineConfig({
    plugins: [
        fable({ fsproj }),
        react({ include: /\.fs$/ }),
    ],
    root: "./src/PlaningPoker.Client",
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
    }


})