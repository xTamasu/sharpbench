import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';
import { apiClient } from '../apiClient';
import { useNavigate, Link } from 'react-router-dom';

const registerSchema = z.object({
  email: z.string().email(),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  displayName: z.string().min(1, 'Display name is required').max(50),
});

type RegisterForm = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
  });

  const mutation = useMutation({
    mutationFn: (data: RegisterForm) => apiClient.post('/auth/register', data),
    onSuccess: (res) => {
      localStorage.setItem('token', res.data.token);
      localStorage.setItem('userId', res.data.userId);
      navigate('/tasks');
    },
  });

  return (
    <div className="flex h-screen items-center justify-center bg-gray-100">
      <div className="w-full max-w-md bg-white rounded-lg shadow-md p-8">
        <h2 className="text-2xl font-bold text-center mb-6">Register</h2>
        {mutation.isError && (
          <div className="bg-red-100 text-red-700 p-3 rounded mb-4">
            Registration failed. Email might already exist.
          </div>
        )}
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))}>
          <div className="mb-4">
            <label className="block text-gray-700 mb-2">Display Name</label>
            <input 
              {...register('displayName')} 
              type="text" 
              className="w-full border rounded px-3 py-2"
            />
            {errors.displayName && <p className="text-red-500 text-sm mt-1">{errors.displayName.message}</p>}
          </div>
          <div className="mb-4">
            <label className="block text-gray-700 mb-2">Email</label>
            <input 
              {...register('email')} 
              type="email" 
              className="w-full border rounded px-3 py-2"
            />
            {errors.email && <p className="text-red-500 text-sm mt-1">{errors.email.message}</p>}
          </div>
          <div className="mb-6">
            <label className="block text-gray-700 mb-2">Password</label>
            <input 
              {...register('password')} 
              type="password" 
              className="w-full border rounded px-3 py-2"
            />
            {errors.password && <p className="text-red-500 text-sm mt-1">{errors.password.message}</p>}
          </div>
          <button 
            type="submit" 
            disabled={mutation.isPending}
            className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600 disabled:opacity-50"
          >
            {mutation.isPending ? 'Registering...' : 'Register'}
          </button>
        </form>
        <div className="mt-4 text-center">
          <Link to="/login" className="text-blue-500 hover:underline">Already have an account? Login</Link>
        </div>
      </div>
    </div>
  );
}
