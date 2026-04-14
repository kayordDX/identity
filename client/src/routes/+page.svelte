<script lang="ts">
	import { userManager, type User } from "$lib/auth";

	import AuthCard from "../lib/AuthCard.svelte";
	import { onMount } from "svelte";
	import ApiCard from "$lib/ApiCard.svelte";

	let user: User | null = $state(null);
	let isLoading = $state(true);

	onMount(async () => {
		try {
			// Handle callback
			const params = new URLSearchParams(window.location.search);
			if (params.has("code") || params.has("error")) {
				await userManager.signinRedirectCallback();
				window.history.replaceState({}, document.title, "/");
			}

			user = await userManager.getUser();
		} catch (err) {
			console.error("Auth init error:", err);
		} finally {
			isLoading = false;
		}
	});
</script>

<div class="p-4">
	<h1 class="text-2xl">Identity Service</h1>
	<p class="text-muted-foreground">OpenID Connect test client · Authorization Code + PKCE</p>
</div>

<div class="grid gap-2 p-2">
	<AuthCard bind:user bind:isLoading />
	<ApiCard {user} />
</div>
