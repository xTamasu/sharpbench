// LoginPage.tsx
// Email + password form. On success, stores the JWT via AuthContext and
// navigates to the originally requested URL (or /tasks).
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { login } from '../api/auth';
import { useAuth } from '../auth/AuthContext';

const schema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(1, 'Password is required'),
});
type FormValues = z.infer<typeof schema>;

interface LocationState { from?: { pathname: string }; }

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { setSession } = useAuth();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (auth) => {
      setSession(auth);
      const from = (location.state as LocationState | null)?.from?.pathname ?? '/tasks';
      navigate(from, { replace: true });
    },
  });

  const onSubmit = (values: FormValues) => mutation.mutate(values);

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-sm bg-white border border-slate-200 rounded-lg p-6 shadow-sm">
        <h1 className="text-xl font-semibold mb-4">Sign in</h1>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-3" noValidate>
          <div>
            <label htmlFor="email" className="block text-sm font-medium mb-1">Email</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              className="w-full border border-slate-300 rounded px-2 py-1.5"
              {...register('email')}
            />
            {errors.email && <p role="alert" className="text-sm text-red-600 mt-1">{errors.email.message}</p>}
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium mb-1">Password</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              className="w-full border border-slate-300 rounded px-2 py-1.5"
              {...register('password')}
            />
            {errors.password && <p role="alert" className="text-sm text-red-600 mt-1">{errors.password.message}</p>}
          </div>

          {mutation.isError && (
            <p role="alert" className="text-sm text-red-600">{(mutation.error as Error).message}</p>
          )}

          <button
            type="submit"
            disabled={mutation.isPending}
            className="w-full bg-slate-900 text-white rounded py-2 disabled:opacity-60"
          >
            {mutation.isPending ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        <p className="text-sm text-slate-600 mt-4">
          No account? <Link to="/register" className="text-blue-600 underline">Register</Link>
        </p>
      </div>
    </div>
  );
}
