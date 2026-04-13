import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	preprocess: vitePreprocess(),
	kit: {
		alias: {
			$lib: './src/lib'
		},
		adapter: adapter({
			// Output the static build directly into the backend's wwwroot so
			// ASP.NET Core can serve the Svelte SPA as static files.
			pages: '../api/src/identity/wwwroot',
			assets: '../api/src/identity/wwwroot',
			// SPA fallback: any URL the server doesn't recognise (including the
			// OIDC redirect-back URL) is served as index.html so the client-side
			// router can take over and handle the ?code= / ?error= parameters.
			fallback: 'index.html'
		})
	},
	vitePlugin: {
		inspector: {
			showToggleButton: 'never'
		}
	}
};

export default config;
