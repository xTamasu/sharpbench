// TaskListPage.tsx — wraps TaskList with the create form.
import CreateTaskForm from '../components/CreateTaskForm';
import TaskList from '../components/TaskList';

export default function TaskListPage() {
  return (
    <div>
      <h1 className="text-xl font-semibold mb-4">Tasks</h1>
      <CreateTaskForm />
      <TaskList />
    </div>
  );
}
