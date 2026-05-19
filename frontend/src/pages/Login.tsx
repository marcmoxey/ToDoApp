import { useState } from "react";
import { supabase } from "../supabaseClient";

function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [isSignUp, setIsSignUp] = useState(false); // toggles between login and sign up

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setMessage(null);
    setLoading(true);

    if (isSignUp) {
      // Sign up flow
      const { error } = await supabase.auth.signUp({ email, password });
      if (error) {
        setError(error.message);
      } else {
        // Supabase sends a confirmation email by default
        setMessage(
          "Account created! Check your email to confirm your account.",
        );
      }
    } else {
      // Login flow
      const { error } = await supabase.auth.signInWithPassword({
        email,
        password,
      });
      if (error) {
        setError(error.message);
      }
    }

    setLoading(false);
  };

  return (
    <div className="app">
      <div className="login-wrap">
        <h1>Todo App</h1>
        <p className="sub">
          {isSignUp ? "create an account" : "sign in to continue"}
        </p>

        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label>Password</label>
            <input 
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          {error && <p className="err-msg">{error}</p>}
          {message && <p className="success-msg">{message}</p>}

          <button className="btn-primary" type="submit" disabled={loading}>
            {loading ? (
              <>
                <span className="spinner" />{" "}
                {isSignUp ? "creating account..." : "logging in..."}
              </>
            ) : isSignUp ? (
              "create account"
            ) : (
              "login"
            )}
          </button>
        </form>

        {/* Toggle between login and sign up */}
        <p className="toggle-auth">
          {isSignUp ? "already have an account?" : "don't have an account?"}
          <button
            className="btn-link"
            onClick={() => {
              setIsSignUp(!isSignUp);
              setError(null);
              setMessage(null);
            }}
          >
            {isSignUp ? "login" : "sign up"}
          </button>
        </p>
      </div>
    </div>
  );
}

export default Login;
