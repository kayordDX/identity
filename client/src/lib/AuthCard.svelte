<script lang="ts">
	import { UserManager } from 'oidc-client-ts';
	import { PUBLIC_OIDC_AUTHORITY } from '$env/static/public';

	// In dev (vite dev) the app runs on a different origin than the OIDC server,
	// so PUBLIC_OIDC_AUTHORITY is set in .env.development to point at the backend.
	// In production the SPA is served by the backend itself, so both share the
	// same origin and we fall back to window.location.origin.
	const authority = PUBLIC_OIDC_AUTHORITY || window.location.origin;

	type Props = {
		user: any;
		isLoading: boolean;
	};

	let { user = $bindable(), isLoading = $bindable() }: Props = $props();

	let mgr: UserManager;

	async function login() {
		if (!mgr) {
			mgr = new UserManager({
				authority,
				client_id: 'web_client',
				redirect_uri: window.location.origin + '/',
				post_logout_redirect_uri: window.location.origin + '/',
				scope: 'openid profile email offline_access',
				response_type: 'code'
			});
		}
		await mgr.signinRedirect();
	}

	async function logout() {
		if (!mgr) {
			mgr = new UserManager({
				authority,
				client_id: 'web_client',
				redirect_uri: window.location.origin + '/',
				post_logout_redirect_uri: window.location.origin + '/',
				scope: 'openid profile email offline_access',
				response_type: 'code'
			});
		}
		await mgr.signoutRedirect({ id_token_hint: user?.id_token });
	}
</script>

<div class="card">
	<h2>Authentication</h2>

	{#if isLoading}
		<span class="badge offline">⏳ Initialising…</span>
	{:else if user && !user.expired}
		<span class="badge online">● Authenticated</span>
		<table class="info-table">
			<tbody>
				<tr>
					<td>Subject</td>
					<td><strong>{user.profile.sub}</strong></td>
				</tr>
				<tr>
					<td>Name</td>
					<td><strong>{user.profile.name || '—'}</strong></td>
				</tr>
				<tr>
					<td>Email</td>
					<td><strong>{user.profile.email || '—'}</strong></td>
				</tr>
				<tr>
					<td>Expires</td>
					<td><strong>{new Date(user.expires_at * 1000).toLocaleTimeString()}</strong></td>
				</tr>
			</tbody>
		</table>
		<div style="margin-top: 0.75rem">
			<small style="color: #6b7280">Access Token</small>
			<div class="token-box">{user.access_token}</div>
		</div>
		<div class="btn-row">
			<button class="btn btn-danger" on:click={logout}>Sign Out</button>
		</div>
	{:else}
		<span class="badge offline">● Not signed in</span>
		<p style="font-size: 0.875rem; color: #6b7280; margin: 0.5rem 0 1rem">
			Clicking <em>Sign In</em> will redirect you to the identity server's login page and return here
			after authentication.
		</p>
		<div class="btn-row">
			<button class="btn btn-primary" on:click={login}>Sign In</button>
		</div>
	{/if}
</div>

<style>
	.card {
		background: #fff;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		padding: 1.5rem;
	}
</style>
