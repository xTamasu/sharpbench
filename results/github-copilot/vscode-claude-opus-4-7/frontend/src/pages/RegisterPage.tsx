// RegisterPage.tsx — registration form mirroring backend validation rules.
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Link, useNavigate } from 'react-router-dom';
import { register as registerApi } from '../api/auth';
import { useAuth } from '../auth/AuthContext';

// Constraints kept in sync with the backend FluentValidation rules.
const schema = z.object({
  email: z.string().email().max(256),
  password: z.string().min(8, 'At least 8 characters').max(128),
  displayName: z.string().min(1).max(100),
});
type FormValues = z.infer<typeof schema>;

export default function RegisterPage() {
  const navigate = useNavigate();
  const { setSession } = useAuth();

  const {
    register: rhfRegister,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const mutation = useMutation({
    mutationFn: registerApi,
    onSuccess: (auth) => {
      setSession(auth);
      navigate('/tasks', { replace: true });
    },
  });

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-sm bg-white border border-slate-200 rounded-lg p-6 shadow-sm">
        <h1 className="text-xl font-semibold mb-4">Create an account</h1>
        <form onSubmit={handleSubmit((v) => mutation.mutate(v))} className="space-y-3" noValidate>
          <div>
            <label htmlFor="email" className="block text-sm font-medium mb-1">Email</label>
            <input id="email" type="email" className="w-full border border-slate-300 rounded px-2 py-1.5" {...rhfRegister('email')} />
            {errors.email && <p role="alert" className="text-sm text-red-600 mt-1">{errors.email.message}</p>}
          </div>
          <div>
            <label htmlFor="displayName" className="block text-sm font-medium mb-1">Display name</label>
            <input id="displayName" type="text" className="w-full border border-slate-300 rounded px-2 py-1.5" {...rhfRegister('displayName')} />
            {errors.displayName && <p role="alert" className="text-sm text-red-600 mt-1">{errors.displayName.message}</p>}
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium mb-1">Password</label>
            <input id="password" type="password" className="w-full border border-slate-300 rounded px-2 py-1.5" {...rhfRegister('password')} />
            {errors.password && <p role="alert" className="text-sm text-red-600 mt-1">{errors.password.message}</p>}
          </div>

          {mutation.isError && (
            <p role="alert" className="text-sm text-red-600">{(mutation.error as Error).message}</p>
          )}

          <button type="submit" disabled={mutation.isPending} className="w-full bg-slate-900 text-white rounded py-2 disabled:opacity-60">
            {mutation.isPending ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        <p className="text-sm text-slate-600 mt-4">
          Already have an account? <Link to="/login" className="text-blue-600 underline">Sign in</Link>
        </p>
      </div>
    </div>
  );
}
