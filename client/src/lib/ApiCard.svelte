<script lang="ts">
	type Props = {
		user: any;
	};

	let { user }: Props = $props();

	let apiOutput = '';
	let apiError = '';
	let isLoading = false;

	async function callApi() {
		isLoading = true;
		apiOutput = '';
		apiError = '';

		if (!user || user.expired) {
			apiError = '⚠ Not authenticated. Please sign in first.';
			isLoading = false;
			return;
		}

		try {
			const res = await fetch('/api/me', {
				headers: { Authorization: `Bearer ${user.access_token}` }
			});

			const body = await res.json();

			if (res.ok) {
				apiOutput = JSON.stringify(body, null, 2);
			} else {
				apiError = `HTTP ${res.status}: ${JSON.stringify(body)}`;
			}
		} catch (err) {
			apiError = `Network error: ${err instanceof Error ? err.message : 'Unknown error'}`;
		} finally {
			isLoading = false;
		}
	}
</script>

<div class="card">
	<h2>Protected API Response</h2>
	<p style="font-size: 0.875rem; color: #6b7280; margin: 0 0 0.5rem">
		Calls <code>GET /api/me</code> with the access token as a Bearer header.
	</p>
	{#if !apiOutput && !apiError}
		<div style="font-size: 0.875rem; color: #9ca3af">
			Sign in, then click <em>Call /api/me</em>.
		</div>
	{/if}
	{#if apiOutput}
		<pre>{apiOutput}</pre>
	{/if}
	{#if apiError}
		<div class="alert alert-error">{apiError}</div>
	{/if}
	<div class="btn-row" style="margin-top: 1rem">
		<button class="btn btn-success" disabled={isLoading} on:click={callApi}>
			{isLoading ? 'Loading...' : 'Call /api/me'}
		</button>
	</div>
</div>

<style>
	.card {
		background: #fff;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		padding: 1.5rem;
	}
</style>
