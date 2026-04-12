<script lang="ts">
	let username = '';
	let email = '';
	let password = '';
	let feedback = '';
	let feedbackType = 'error';
	let isLoading = false;

	async function register() {
		feedback = '';

		if (!username || !password) {
			feedback = '⚠ Username and password are required.';
			feedbackType = 'error';
			return;
		}

		isLoading = true;

		try {
			const res = await fetch('/api/account/register', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ username, email, password })
			});

			const body = await res.json();

			if (res.ok) {
				feedback = `✅ ${body.message} You can now sign in.`;
				feedbackType = 'success';
				username = '';
				email = '';
				password = '';
			} else {
				feedback = `❌ ${body.error}`;
				feedbackType = 'error';
			}
		} catch (err) {
			feedback = `❌ Network error: ${err instanceof Error ? err.message : 'Unknown error'}`;
			feedbackType = 'error';
		} finally {
			isLoading = false;
		}
	}
</script>

<div class="card full">
	<h2>Register New User</h2>
	<p style="font-size: 0.875rem; color: #6b7280; margin: 0 0 1rem">
		Creates an account via <code>POST /api/account/register</code>. After registering, use
		<em>Sign In</em> above with the new credentials.
	</p>

	<div
		style="
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
          "
	>
		<div>
			<label for="reg-username">Username</label>
			<input
				id="reg-username"
				type="text"
				placeholder="johndoe"
				bind:value={username}
				autocomplete="off"
			/>
		</div>
		<div>
			<label for="reg-email">Email</label>
			<input
				id="reg-email"
				type="email"
				placeholder="john@example.com"
				bind:value={email}
				autocomplete="off"
			/>
		</div>
		<div>
			<label for="reg-password"
				>Password
				<small style="color: #9ca3af">(min 6 chars)</small></label
			>
			<input
				id="reg-password"
				type="password"
				placeholder="••••••"
				bind:value={password}
				autocomplete="new-password"
			/>
		</div>
	</div>

	<div class="btn-row" style="margin-top: 1rem">
		<button class="btn btn-primary" disabled={isLoading} on:click={register}>
			{isLoading ? 'Creating…' : 'Create Account'}
		</button>
	</div>

	{#if feedback}
		<div class="alert alert-{feedbackType}">{feedback}</div>
	{/if}

	<details style="margin-top: 1.5rem; font-size: 0.8rem; color: #6b7280">
		<summary style="cursor: pointer">Default seeded account</summary>
		<p style="margin: 0.5rem 0 0">
			Username: <code>admin</code> &nbsp;·&nbsp; Password: <code>password</code>
		</p>
	</details>
</div>

<style>
	.card {
		background: #fff;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		padding: 1.5rem;
	}

	.full {
		grid-column: 1 / -1;
	}
</style>
