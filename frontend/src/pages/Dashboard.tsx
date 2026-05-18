import { useState, useEffect } from "react";
import { Session } from "@supabase/supabase-js";
import { useNavigate } from "react-router-dom";
import { supabase } from "../supabaseClient";
import axios from "axios";
import CreateTodoForm from "../components/CreateTodoForm";
import TodoList from "../components/TodoList";
import Navbar from "../components/Navbar"
import type { Todo } from "../types/todo";

interface Props {
  session: Session;
}

function Dashboard({ session }: Props) {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const authHeaders = {
    headers: { Authorization: `Bearer ${session.access_token}` },
  };

  const fetchTodos = async () => {
    try {
      const { data } = await axios.get<Todo[]>("/api/todo", authHeaders);
      setTodos(data);
    } catch {
      setError("Failed to load todos");
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (task: string) => {
    try {
      const { data } = await axios.post<Todo>(
        "/api/todo",
        { task },
        authHeaders,
      );
      setTodos((prev) => [...prev, data]);
    } catch {
      setError("Failed to create todo");
    }
  };

  const handleComplete = async (id: number) => {
    try {
      await axios.put(`/api/todo/${id}/complete`, {}, authHeaders);
      setTodos((prev) =>
        prev.map((todo) =>
          todo.id === id ? { ...todo, isComplete: true } : todo,
        ),
      );
    } catch {
      setError("Failed to complete todo");
    }
  };

  // Navigate to the edit page — the todo data goes along as route state
  // so EditTodo can pre-populate the form without an extra API call
  const handleEdit = (todo: Todo) => {
    navigate(`/edit/${todo.id}`, { state: { todo } });
  };

  const handleDelete = async (id: number) => {
    try {
      await axios.delete(`/api/todo/${id}`, authHeaders);
      setTodos((prev) => prev.filter((todo) => todo.id !== id));
    } catch {
      setError("Failed to delete todo");
    }
  };

  const handleLogout = async () => {
    await supabase.auth.signOut();
  };

  useEffect(() => {
    fetchTodos();
  }, []);

  // Get the user's email from the session to show in the welcome message
  const userEmail = session.user?.email ?? "user";

  return (
    <div>
      <Navbar onLogout={handleLogout} />

      <div className="app">
        <div className="dash">
          {/* Welcome message shown at the top of the dashboard */}
          <p className="welcome-msg">welcome, {userEmail}</p>

          <div className="dash-header">
            <h1>Dashboard</h1>
          </div>

          <div className="stats">
            <div className="stat">
              <span>{todos.length}</span>total
            </div>
            <div className="stat">
              <span>{todos.filter((t) => t.isComplete).length}</span>done
            </div>
            <div className="stat">
              <span>{todos.filter((t) => !t.isComplete).length}</span>remaining
            </div>
          </div>

          {error && <p className="err-msg">{error}</p>}

          <CreateTodoForm onCreate={handleCreate} />
          <div className="divider" />

          {loading ? (
            <p className="loading-line">loading todos...</p>
          ) : (
            <TodoList
              todos={todos}
              onComplete={handleComplete}
              onEdit={handleEdit}
              onDelete={handleDelete}
            />
          )}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
