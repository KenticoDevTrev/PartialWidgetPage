import {UserConfig, defineConfig} from "vite";
import babel from 'vite-plugin-babel';

export default defineConfig(async () => {
    const config : UserConfig = {
        appType: "custom",
        root: "./src",
        build: {
            assetsDir: "",
            emptyOutDir: true,
            outDir: "../wwwroot/",
            sourcemap: true,
            rollupOptions: {
                input: [
                    "src/js/pwp.js",
                ],
                output: {
                    chunkFileNames: `[name].js`,
                    assetFileNames: `[name].[ext]`,
                    entryFileNames: `[name].js`,
                    preserveModules: false,
                },
                preserveEntrySignatures: "strict"
            },
        }
    };

    return config;
});