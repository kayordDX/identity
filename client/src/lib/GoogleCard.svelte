<script lang="ts">
	import { UserManager } from 'oidc-client-ts';

	type Props = {
		user: any;
	};

	let { user }: Props = $props();

	let googleClientId = '';
	let isGoogleConfigured = false;

	let mgr: UserManager;

	async function initGoogle() {
		try {
			const res = await fetch('/api/config/google-client-id');
			const data = await res.json();
			googleClientId = data.clientId || '';
			isGoogleConfigured = !!googleClientId;

			if (isGoogleConfigured && window.google) {
				window.google.accounts.id.initialize({
					client_id: googleClientId,
					callback: handleGoogleSignIn
				});
			}
		} catch (err) {
			console.warn('Google config not available:', err);
		}
	}

	async function handleGoogleSignIn(response: any) {
		const btn = document.getElementById('google-btn') as HTMLButtonElement;
		if (!btn) return;

		btn.disabled = true;

		try {
			const exchangeRes = await fetch('/account/login/google/exchange', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ googleIdToken: response.credential })
			});

			const data = await exchangeRes.json();

			if (!exchangeRes.ok || !data.success) {
				alert('Google sign-in failed: ' + (data.message || 'Unknown error'));
				btn.disabled = false;
				return;
			}

			if (!mgr) {
				mgr = new UserManager({
					authority: window.location.origin,
					client_id: 'web_client',
					redirect_uri: window.location.origin + '/',
					post_logout_redirect_uri: window.location.origin + '/',
					scope: 'openid profile email offline_access',
					response_type: 'code'
				});
			}

			await mgr.signinRedirect();
		} catch (err) {
			alert('Google sign-in error: ' + (err instanceof Error ? err.message : 'Unknown error'));
			btn.disabled = false;
		}
	}

	function loginWithGoogle() {
		if (!window.google) {
			alert(
				'Google Sign-In is not available. Make sure Google OAuth credentials are configured in appsettings.json'
			);
			return;
		}
		window.google.accounts.id.prompt();
	}

	// Initialize when component mounts
	$effect(() => {
		if (typeof window !== 'undefined') {
			initGoogle();
		}
	});
</script>

<div class="card">
	<h2>Google Sign-In</h2>
	<p style="font-size: 0.875rem; color: #6b7280; margin: 0 0 1rem">
		Authenticates via Google OAuth 2.0. The identity server validates your Google credential and
		creates a local account on first use.
	</p>

	{#if user && !user.expired}
		<span class="badge online">● Signed in via Google</span>
		<table class="info-table">
			<tbody>
				<tr>
					<td>Name</td>
					<td><strong>{user.profile.name || '—'}</strong></td>
				</tr>
				<tr>
					<td>Email</td>
					<td><strong>{user.profile.email || '—'}</strong></td>
				</tr>
			</tbody>
		</table>
	{:else if isGoogleConfigured}
		<button class="btn-google" id="google-btn" on:click={loginWithGoogle}>
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
			Sign In with Google
		</button>
	{:else}
		<p style="font-size: 0.875rem; color: #9ca3af">
			Google OAuth is not configured on the server. Set credentials in appsettings.json to enable.
		</p>
	{/if}

	<details style="margin-top: 1.5rem; font-size: 0.8rem; color: #6b7280">
		<summary style="cursor: pointer">Setup instructions</summary>
		<ol class="setup-steps">
			<li>
				Create OAuth 2.0 credentials in the
				<a href="https://console.cloud.google.com/apis/credentials" target="_blank" rel="noopener"
					>Google Cloud Console</a
				>
				(Application type: <em>Web application</em>)
			</li>
			<li>
				Add <code>http://localhost:PORT/signin-google</code> as an <em>Authorized redirect URI</em>
			</li>
			<li>
				Set <code>Authentication:Google:ClientId</code> and
				<code>Authentication:Google:ClientSecret</code>
				in <code>appsettings.json</code>
			</li>
		</ol>
	</details>
</div>

<style>
	.card {
		background: #fff;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		padding: 1.5rem;
	}
</style>
