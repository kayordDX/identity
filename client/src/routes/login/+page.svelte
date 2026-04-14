<script lang="ts">
	import { page } from "$app/state";
	import { PUBLIC_OIDC_AUTHORITY } from "$env/static/public";
	import LogoButton from "$lib/components/LogoButton.svelte";
	import { GoogleIcon } from "$lib/svg/icons";
	import { Button, Card, Field, Input, Separator } from "@kayord/ui";
	import { onMount } from "svelte";

	const base = PUBLIC_OIDC_AUTHORITY;

	const returnUrl = $derived(page.url.searchParams.get("returnUrl") ?? "/");
	const error = $derived(page.url.searchParams.get("error"));
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

<div class="flex h-screen w-full flex-col items-center">
	<div class="flex h-full max-w-2xl flex-col items-center justify-center gap-6 p-2">
		<LogoButton />
		<Card.Root>
			<Card.Header>
				<Card.Title class="text-center">Welcome back</Card.Title>
				<Card.Description class="text-center">Sign in</Card.Description>
			</Card.Header>
			<Card.Content class="flex flex-col items-center">
				<form
					method="post"
					action={`${base}/account/login`}
					class="my-2 flex w-full flex-col gap-2"
				>
					<input type="hidden" name="returnUrl" value={returnUrl} />
					<Field.Field>
						<Field.Label for="username">Username</Field.Label>
						<Input id="username" type="text" name="username" required autocomplete="username" />
					</Field.Field>
					<Field.Field>
						<Field.Label for="password">Password</Field.Label>
						<Input
							id="password"
							type="password"
							name="password"
							required
							autocomplete="current-password"
						/>
					</Field.Field>
					<Button type="submit">Sign In</Button>
				</form>
				{#if googleEnabled}
					<div class="flex w-full flex-col gap-2">
						<Separator />
						<Button href={googleHref}>
							<GoogleIcon class="fill-primary-foreground" />
							Continue with Google
						</Button>
					</div>
				{/if}
			</Card.Content>
			<Card.Footer class="flex flex-col items-center gap-2">
				<p class="text-xs text-muted-foreground">
					We use Google to keep your account secure. No password needed.
				</p>
			</Card.Footer>
		</Card.Root>
		<p class="text-xs text-muted-foreground">
			By signing in, you agree to our Terms of Service and Privacy Policy
		</p>
	</div>
</div>
