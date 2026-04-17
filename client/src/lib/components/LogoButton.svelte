<script lang="ts">
	import { goto } from "$app/navigation";
	import { resolve } from "$app/paths";
</script>

<button class="flex items-center gap-3" onclick={() => goto(resolve("/"))}>
	<img src="/favicon.svg" alt="logo" class="h-10" />
	<div class="flex flex-col items-baseline justify-center">
		<span class="hidden text-lg font-bold md:block"> AviateSoft </span>
		<span class="hidden text-xs font-bold text-muted-foreground md:block">Identity</span>
	</div>
</button>
