<script lang="ts">
	import { page } from "$app/state";
	import { PUBLIC_OIDC_AUTHORITY } from "$env/static/public";
	import LogoButton from "$lib/components/LogoButton.svelte";
	import { GoogleIcon } from "$lib/svg/icons";
	import { Alert, Button, Card, Field, Input, Spinner } from "@kayord/ui";
	import { onMount } from "svelte";

	interface GoogleClientIdResponse {
		googleClientId: string | null;
	}

	const returnUrl = $derived(page.url.searchParams.get("returnUrl") ?? "/");
	// const error = $derived(page.url.searchParams.get("error"));
	const googleHref = $derived(
		`${PUBLIC_OIDC_AUTHORITY}/account/login/external?provider=Google&returnUrl=${encodeURIComponent(returnUrl)}`
	);

	let isLoadingGoogle = $state(false);
	let googleEnabled = $state(false);
	let googleError = $state("");

	const getClientId = async () => {
		try {
			isLoadingGoogle = true;
			const res = await fetch(`${PUBLIC_OIDC_AUTHORITY}/config/auth/external`);
			if (res.ok) {
				const data = (await res.json()) as GoogleClientIdResponse;
				googleEnabled = !!data.googleClientId;
			}
		} catch {
			googleError = "Unable to load Google login. Please try again later.";
			// Google not configured or server unavailable — button stays hidden
		} finally {
			isLoadingGoogle = false;
		}
	};

	onMount(async () => {
		getClientId();
	});
</script>

<div class="flex min-h-svh flex-col items-center justify-center gap-6 bg-muted p-6 md:p-10">
	<div class="flex w-full max-w-sm flex-col gap-6">
		<div class="flex justify-center">
			<LogoButton />
		</div>
		<Card.Root>
			<Card.Header>
				<Card.Title class="text-center">Welcome back</Card.Title>
				<Card.Description class="text-center">Login with your Google account</Card.Description>
			</Card.Header>
			<Card.Content class="flex flex-col items-center">
				{#if googleError}
					<Alert.Root variant="destructive">
						<Alert.Title>Google error</Alert.Title>
						<Alert.Description>{googleError}</Alert.Description>
					</Alert.Root>
				{/if}
				{#if googleEnabled && !googleError}
					<div class="flex w-full flex-col gap-2">
						<Button href={googleHref} disabled={isLoadingGoogle}>
							{#if isLoadingGoogle}
								<Spinner />
							{:else}
								<GoogleIcon class="fill-primary-foreground" />
							{/if}
							Login with Google
						</Button>
					</div>
				{/if}

				<Field.Separator class="my-6 w-full *:data-[slot=field-separator-content]:bg-card">
					Or continue with
				</Field.Separator>

				<form
					method="post"
					action={`${PUBLIC_OIDC_AUTHORITY}/account/login`}
					class="my-2 flex w-full flex-col gap-4"
				>
					<input type="hidden" name="returnUrl" value={returnUrl} />
					<Field.Field>
						<Field.Label for="username">Email</Field.Label>
						<Input id="username" type="email" name="username" required autocomplete="email" />
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
					<Button type="submit" class="mt-2">Sign In</Button>
				</form>
			</Card.Content>
		</Card.Root>
		<p class="text-xs text-muted-foreground">
			By signing in, you agree to our Terms of Service and Privacy Policy
		</p>
	</div>
</div>
