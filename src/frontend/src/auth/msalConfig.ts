import { type Configuration, LogLevel } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_MSAL_CLIENT_ID ?? "",
    authority: import.meta.env.VITE_MSAL_AUTHORITY ?? "",
    redirectUri: import.meta.env.VITE_MSAL_REDIRECT_URI ?? window.location.origin,
    postLogoutRedirectUri: "/",
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
  system: {
    loggerOptions: {
      loggerCallback: (_level, message, containsPii) => {
        if (containsPii) return;
        if (import.meta.env.DEV) console.log(`[MSAL] ${message}`);
      },
      logLevel: LogLevel.Warning,
    },
  },
};

export const loginRequest = {
  scopes: [import.meta.env.VITE_API_SCOPE ?? "api://recipeboss.api/Recipes.ReadWrite"],
};

export const apiScopes = [import.meta.env.VITE_API_SCOPE ?? "api://recipeboss.api/Recipes.ReadWrite"];
