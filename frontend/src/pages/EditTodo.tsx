import { useState } from "react";
import { useNavigate, useLocation, useParams } from "react-router-dom";
import { Session } from "@supabase/supabase-js";
import axios from "axios";
import Navbar from "../components/Navbar";
import { supabase } from "../supabaseClient";
import type { Todo } from "../types/todo";

interface Props {
  session: Session;
}

function EditTodo({ session }: Props) {
  const navigate = useNavigate();
  const location = useLocation();
  const { id } = useParams();

  // The todo was passed as route state from the dashboard when Edit was clicked.
  // This means the form is pre-populated immediately — no extra API call needed.
  const todo = location.state?.todo as Todo | undefined;

  // Pre-populate the task field with the existing task value
  const [task, setTask] = useState(todo?.task ?? "");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const authHeaders = {
    headers: { Authorization: `Bearer ${session.access_token}` },
  };

  const handleLogout = async () => {
    await supabase.auth.signOut();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!task.trim()) return;

    setLoading(true);
    setError(null);

    try {
      await axios.put(`/api/todo/${id}`, { task }, authHeaders);
      // Go back to the dashboard after a successful update
      navigate("/");
    } catch {
      setError("Failed to update todo");
      setLoading(false);
    }
  };

  return (
    <div>
      <Navbar onLogout={handleLogout} />

      <div className="app">
        <div className="edit-wrap">
          <div className="edit-header">
            <h1>Edit Todo</h1>
            {/* Back button takes the user back to the dashboard without saving */}
            <button className="btn-ghost" onClick={() => navigate("/")}>
              ← back
            </button>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="field">
              <label>Task</label>
              {/* Input starts with the existing task text already filled in */}
              <input
                type="text"
                value={task}
                onChange={(e) => setTask(e.target.value)}
                required
              />
            </div>

            {error && <p className="err-msg">{error}</p>}

            <button className="btn-primary" type="submit" disabled={loading}>
              {loading ? (
                <>
                  <span className="spinner" /> saving...
                </>
              ) : (
                "save changes"
              )}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

export default EditTodo;
