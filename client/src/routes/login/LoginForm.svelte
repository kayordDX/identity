<script lang="ts">
	import { page } from "$app/state";
	import { PUBLIC_OIDC_AUTHORITY } from "$env/static/public";
	import { Button, Field, Input } from "@kayord/ui";
	import { z } from "zod";
	import { createForm } from "@tanstack/svelte-form";
	import FieldError from "$lib/components/FieldError.svelte";
	import type { AnyFieldApi } from "@tanstack/svelte-form";

	let bypassValidation = false;

	const returnUrl = $derived(page.url.searchParams.get("returnUrl") ?? "/");

	const schema = z.object({
		returnUrl: z.string(),
		username: z.string().min(3, "Username must be at least 3 characters"),
		password: z.string().min(1),
	});

	const isInvalid = (field: AnyFieldApi) => {
		return field.state.meta.isTouched && !field.state.meta.isValid;
	};

	const form = createForm(() => ({
		defaultValues: {
			returnUrl,
			username: "",
			password: "",
		},
		validators: {
			onChange: schema,
		},
	}));
</script>

<form
	method="post"
	id={form.formId}
	action={`${PUBLIC_OIDC_AUTHORITY}/account/login`}
	onsubmit={async (e) => {
		if (bypassValidation) return;

		e.preventDefault();
		e.stopPropagation();
		await form.handleSubmit();

		bypassValidation = true;
		e.currentTarget.submit();
	}}
	class="my-2 flex w-full flex-col gap-4"
>
	<input type="hidden" name="returnUrl" value={returnUrl} />
	<form.Field name="username">
		{#snippet children(field)}
			<Field.Field>
				<Field.Label for="username">Username</Field.Label>
				<Input
					id={field.name}
					value={field.state.value}
					aria-invalid={isInvalid(field)}
					onblur={() => field.handleBlur()}
					oninput={(e) => field.handleChange((e.target as HTMLInputElement).value)}
					type="username"
					name="username"
					autocomplete="email"
				/>
				<FieldError {field} />
			</Field.Field>
		{/snippet}
	</form.Field>

	<form.Field name="password">
		{#snippet children(field)}
			<Field.Field>
				<Field.Label for="password">Password</Field.Label>
				<Input
					id={field.name}
					value={field.state.value}
					aria-invalid={isInvalid(field)}
					onblur={() => field.handleBlur()}
					oninput={(e) => field.handleChange((e.target as HTMLInputElement).value)}
					type="password"
					name="password"
					autocomplete="current-password"
				/>
				<FieldError {field} />
			</Field.Field>
		{/snippet}
	</form.Field>

	<Button type="submit" class="mt-2">Sign In</Button>
</form>
