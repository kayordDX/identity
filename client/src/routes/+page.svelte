<script lang="ts">
	import { onMount } from 'svelte';
	import { UserManager } from 'oidc-client-ts';
	import { PUBLIC_OIDC_AUTHORITY } from '$env/static/public';
	import AuthCard from '../lib/AuthCard.svelte';
	import ApiCard from '../lib/ApiCard.svelte';
	import RegisterCard from '../lib/RegisterCard.svelte';
	import GoogleCard from '../lib/GoogleCard.svelte';

	let user: any = $state(null);
	let isLoading = $state(true);

	// In dev (vite dev) PUBLIC_OIDC_AUTHORITY is set in .env.development to
	// point at the backend.  In production the SPA is served by the backend
	// itself, so both share the same origin and we fall back to
	// window.location.origin.
	const authority = PUBLIC_OIDC_AUTHORITY || window.location.origin;

	onMount(async () => {
		try {
			const mgr = new UserManager({
				authority,
				client_id: 'web_client',
				redirect_uri: window.location.origin + '/',
				post_logout_redirect_uri: window.location.origin + '/',
				scope: 'openid profile email offline_access',
				response_type: 'code'
			});

			// Handle callback
			const params = new URLSearchParams(window.location.search);
			if (params.has('code') || params.has('error')) {
				await mgr.signinRedirectCallback();
				window.history.replaceState({}, document.title, '/');
			}

			user = await mgr.getUser();
		} catch (err) {
			console.error('Auth init error:', err);
		} finally {
			isLoading = false;
		}
	});
</script>

<div class="header">
	<h1>🔐 Identity Service</h1>
	<p class="subtitle">OpenID Connect test client · Authorization Code + PKCE</p>
</div>

<div class="grid">
	<AuthCard bind:user bind:isLoading />
	<ApiCard {user} />
	<RegisterCard />
	<GoogleCard {user} />
</div>

<style>
	:global(*),
	:global(*::before),
	:global(*::after) {
		box-sizing: border-box;
	}

	:global(body) {
		font-family:
			system-ui,
			-apple-system,
			sans-serif;
		background: #f3f4f6;
		margin: 0;
		padding: 2rem 1rem;
		color: #1f2937;
	}

	.header {
		max-width: 900px;
		margin: 0 auto 1.5rem;
	}

	.header h1 {
		margin: 0 0 0.25rem;
		font-size: 1.75rem;
	}

	.subtitle {
		margin: 0;
		color: #6b7280;
		font-size: 0.95rem;
	}

	.grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
		gap: 1.25rem;
		max-width: 900px;
		margin: 0 auto;
	}
</style>
