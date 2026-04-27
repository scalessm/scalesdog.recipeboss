import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';

export function AuthCallbackPage() {
  const { inProgress } = useMsal();
  const navigate = useNavigate();

  // Read hash synchronously on first render — before MSAL's async init clears it.
  const [authError] = useState<string | null>(() => {
    const params = new URLSearchParams(window.location.hash.slice(1));
    const error = params.get('error');
    return error ? (params.get('error_description') ?? error) : null;
  });

  useEffect(() => {
    if (authError) return; // Stay on this page to show the error.
    if (inProgress === 'none') {
      navigate('/', { replace: true });
    }
  }, [inProgress, authError, navigate]);

  if (authError) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen gap-4 px-6">
        <p className="text-red-500 text-sm font-semibold">Sign-in failed</p>
        <p className="text-gray-400 text-xs max-w-md text-center">{authError}</p>
        <button
          onClick={() => navigate('/', { replace: true })}
          className="text-sm transition-colors"
          style={{ color: "var(--color-primary)" }}
        >
          Return home
        </button>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-screen">
      <p className="text-gray-500 text-sm">Signing in…</p>
    </div>
  );
}
