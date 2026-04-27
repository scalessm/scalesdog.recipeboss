import { useMsal, useIsAuthenticated } from "@azure/msal-react";
import { loginRequest } from "../auth/msalConfig";

export function LoginButton() {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  if (isAuthenticated) {
    return (
      <div className="flex items-center gap-3">
        <span className="text-sm" style={{ color: "var(--color-text-secondary)" }}>
          {accounts[0]?.name ?? accounts[0]?.username}
        </span>
        <button
          onClick={() => instance.logoutRedirect({ postLogoutRedirectUri: "/" })}
          className="text-sm transition-colors"
          style={{ color: "var(--color-primary)" }}
          onMouseOver={(e) => (e.currentTarget.style.color = "var(--color-primary-dark)")}
          onMouseOut={(e) => (e.currentTarget.style.color = "var(--color-primary)")}
        >
          Sign out
        </button>
      </div>
    );
  }

  return (
    <button
      onClick={() => instance.loginRedirect(loginRequest)}
      className="px-4 py-2 rounded-md text-sm font-medium text-white transition-colors"
      style={{ backgroundColor: "var(--color-primary)" }}
      onMouseOver={(e) => (e.currentTarget.style.backgroundColor = "var(--color-primary-dark)")}
      onMouseOut={(e) => (e.currentTarget.style.backgroundColor = "var(--color-primary)")}
    >
      Sign in
    </button>
  );
}
