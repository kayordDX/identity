<script lang="ts">
	import { userManager, type User } from "$lib/auth";
	import { Button, Card, StatusDot } from "@kayord/ui";

	type Props = {
		user: User | null;
		isLoading: boolean;
	};

	let { user = $bindable(), isLoading = $bindable() }: Props = $props();

	async function login() {
		await userManager.signinRedirect();
	}

	async function logout() {
		await userManager.signoutRedirect({ id_token_hint: user?.id_token });
	}
</script>

<Card.Root class="p-4">
	<h2>Authentication</h2>

	{#if isLoading}
		<span class="badge offline">⏳ Initialising…</span>
	{:else if user && !user.expired}
		<span class="badge online flex items-center gap-2"> <StatusDot.Root /> Authenticated</span>
		<table class="info-table">
			<tbody>
				<tr>
					<td>Subject</td>
					<td><strong>{user.profile.sub}</strong></td>
				</tr>
				<tr>
					<td>Name</td>
					<td><strong>{user.profile.name || "—"}</strong></td>
				</tr>
				<tr>
					<td>Email</td>
					<td><strong>{user.profile.email || "—"}</strong></td>
				</tr>
				<tr>
					<td>Expires</td>
					<td><strong>{new Date((user.expires_at ?? 0) * 1000).toLocaleTimeString()}</strong></td>
				</tr>
			</tbody>
		</table>
		<div style="margin-top: 0.75rem">
			<small style="color: #6b7280">Access Token</small>
			<div class="token-box truncate">{user.access_token}</div>
		</div>
		<div class="btn-row">
			<Button variant="destructive" onclick={logout}>Sign Out</Button>
		</div>
	{:else}
		<span class="badge offline">● Not signed in</span>
		<p style="font-size: 0.875rem; color: #6b7280; margin: 0.5rem 0 1rem">
			Clicking <em>Sign In</em> will redirect you to the identity server's login page and return here
			after authentication.
		</p>
		<div class="btn-row">
			<Button onclick={login}>Sign In</Button>
		</div>
	{/if}
</Card.Root>
