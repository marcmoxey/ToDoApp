import { useState } from 'react';
import  { supabase } from "../supabaseClient"; 

function Login() {
    const [email, setEmail] = useState(''); 
    const [password, setPassword] = useState(''); 
    const [error, setError] = useState<string | null>(null); 
    const [loading, setLoading] = useState(false); 

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault(); 
        setError(null); 
        setLoading(true);

        const { error } = await supabase.auth.signInWithPassword({ email, password }); 

        if (error) {
            setError(error.message);
        }

        setLoading(false);
    }

return (
  <div className="app">
    <div className="login-wrap">
      <h1>Todo App</h1>
      <p className="sub">sign in to continue</p>

      <form onSubmit={handleLogin}>
        <div className="field">
          <label>Email</label>
          <input title='email'
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="field">
          <label>Password</label>
          <input title='password'
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        {error && <p className="err-msg">{error}</p>}

        <button className="btn-primary" type="submit" disabled={loading}>
          {loading ? (
            <>
              <span className="spinner" /> logging in...
            </>
          ) : (
            "login"
          )}
        </button>
      </form>
    </div>
  </div>
);
}

export default Login