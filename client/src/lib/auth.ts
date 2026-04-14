import { PUBLIC_URL, PUBLIC_OIDC_AUTHORITY } from "$env/static/public";
import { UserManager, type User } from "oidc-client-ts";

const authority = PUBLIC_OIDC_AUTHORITY || window.location.origin;

const settings = {
	authority,
	client_id: "web_client",
	redirect_uri: PUBLIC_URL + "/",
	post_logout_redirect_uri: PUBLIC_URL + "/",
	scope: "openid profile email offline_access",
	response_type: "code",
	automaticSilentRenew: true,
	// userStore: new WebStorageStateStore({ store: window.localStorage }),
};

const userManager = new UserManager(settings);

export { userManager, type User };
