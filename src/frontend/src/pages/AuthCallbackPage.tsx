import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';

export function AuthCallbackPage() {
  const { inProgress } = useMsal();
  const navigate = useNavigate();

  useEffect(() => {
    // MSAL handles the auth code automatically on init.
    // Once inProgress is 'none', the auth flow is complete.
    if (inProgress === 'none') {
      navigate('/', { replace: true });
    }
  }, [inProgress, navigate]);

  return (
    <div className="flex items-center justify-center min-h-screen">
      <p className="text-gray-500 text-sm">Signing in...</p>
    </div>
  );
}
