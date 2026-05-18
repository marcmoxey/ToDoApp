import { useEffect, useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Session } from "@supabase/supabase-js";
import { supabase } from "./supabaseClient";
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import EditTodo from "./pages/EditTodo";
import "./App.css";

function App() {
  const [session, setSession] = useState<Session | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session);
      setLoading(false);
    });

    const {
      data: { subscription },
    } = supabase.auth.onAuthStateChange((_event, session) => {
      setSession(session);
    });

    return () => subscription.unsubscribe();
  }, []);

  if (loading)
    return (
      <div
        className="loading-line"
        style={{ padding: 40, textAlign: "center" }}
      >
        loading...
      </div>
    );

  return (
    <BrowserRouter>
      <Routes>
        {/* Public route — redirect to dashboard if already logged in */}
        <Route
          path="/login"
          element={session ? <Navigate to="/" replace /> : <Login />}
        />

        {/* Dashboard — protected */}
        <Route
          path="/"
          element={
            session ? (
              <Dashboard session={session} />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />

        {/* Edit todo page — /edit/5 — protected */}
        <Route
          path="/edit/:id"
          element={
            session ? (
              <EditTodo session={session} />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
