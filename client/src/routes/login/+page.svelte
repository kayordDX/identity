<script module>
	// This page depends entirely on URL params (?returnUrl, ?error) that are
	// only known at runtime, so skip static prerendering and let the SPA
	// fallback (index.html) serve it client-side.
	export const prerender = false;
</script>

<script lang="ts">
	import { page } from '$app/state';
	import { PUBLIC_OIDC_AUTHORITY } from '$env/static/public';
	import { onMount } from 'svelte';

	// In dev:  PUBLIC_OIDC_AUTHORITY = 'http://localhost:5214'  → absolute URLs
	// In prod: PUBLIC_OIDC_AUTHORITY = ''                       → relative URLs
	const base = PUBLIC_OIDC_AUTHORITY;

	// Reactively derived so they update correctly after client-side hydration.
	const returnUrl = $derived(page.url.searchParams.get('returnUrl') ?? '/');
	const error = $derived(page.url.searchParams.get('error'));
	const googleHref = $derived(
		`${base}/account/login/external?provider=Google&returnUrl=${encodeURIComponent(returnUrl)}`
	);

	let googleEnabled = $state(false);

	onMount(async () => {
		try {
			const res = await fetch(`${base}/api/config/google-client-id`);
			if (res.ok) {
				const data = await res.json();
				googleEnabled = !!data.clientId;
			}
		} catch {
			// Google not configured or server unavailable — button stays hidden
		}
	});
</script>

<div class="page">
	<div class="card login-card">
		<h1>🔐 Sign In</h1>
		<p class="subtitle">Identity Server</p>

		{#if error}
			<div class="alert alert-error">{error}</div>
		{/if}

		<form method="post" action={`${base}/account/login`}>
			<!-- returnUrl carries the OIDC authorization endpoint back to the server -->
			<input type="hidden" name="returnUrl" value={returnUrl} />

			<label for="username">Username</label>
			<input id="username" type="text" name="username" required autofocus autocomplete="username" />

			<label for="password">Password</label>
			<input
				id="password"
				type="password"
				name="password"
				required
				autocomplete="current-password"
			/>

			<button type="submit" class="btn btn-primary submit-btn">Sign In</button>
		</form>

		{#if googleEnabled}
			<div class="divider"><span>or</span></div>
			<a href={googleHref} class="btn-google" data-sveltekit-reload>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					viewBox="0 0 48 48"
					width="20"
					height="20"
					aria-hidden="true"
				>
					<path
						fill="#EA4335"
						d="M24 9.5c3.14 0 5.95 1.08 8.17 2.85l6.1-6.1C34.46 3.04 29.5 1 24 1 14.82 1 7.07 6.48 3.64 14.22l7.1 5.52C12.46 13.65 17.77 9.5 24 9.5z"
					/>
					<path
						fill="#4285F4"
						d="M46.52 24.5c0-1.64-.15-3.22-.42-4.75H24v9h12.68c-.55 2.96-2.2 5.47-4.68 7.15l7.18 5.57C43.25 37.32 46.52 31.36 46.52 24.5z"
					/>
					<path
						fill="#FBBC05"
						d="M10.74 28.26A14.57 14.57 0 0 1 9.5 24c0-1.48.25-2.91.74-4.26l-7.1-5.52A23.93 23.93 0 0 0 0 24c0 3.87.93 7.53 2.56 10.75l7.18-5.57z"
					/>
					<path
						fill="#34A853"
						d="M24 47c5.5 0 10.12-1.82 13.49-4.94l-7.18-5.57C28.6 38.3 26.41 39 24 39c-6.23 0-11.54-4.15-13.44-9.74l-7.18 5.57C7.07 42.52 14.82 47 24 47z"
					/>
				</svg>
				Continue with Google
			</a>
		{/if}
	</div>
</div>

<style>
	/* Full-viewport centering for the standalone login experience */
	.page {
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 100vh;
		padding: 1rem;
		background: #f3f4f6;
	}

	.login-card {
		width: 100%;
		max-width: 380px;
	}

	/* Override the global h1 size — the login card wants a slightly smaller heading */
	.login-card :global(h1),
	h1 {
		font-size: 1.5rem;
		text-align: center;
		margin-bottom: 0.125rem;
	}

	.subtitle {
		text-align: center;
		margin-top: 0;
		margin-bottom: 1.25rem;
	}

	/* Submit button fills the full card width */
	.submit-btn {
		display: block;
		width: 100%;
		margin-top: 1.25rem;
		padding: 0.65rem;
		font-size: 1rem;
	}
</style>
