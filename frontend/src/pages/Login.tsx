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
        <div>
            <h1>Login</h1>
           <form onSubmit={handleLogin}>
                <div>
                    <label>Email</label>
                    <input title='Email' type="email" value={email} onChange={e => setEmail(e.target.value)} required />
                </div>
                <div>
                    <label>Password</label>
                    <input title='Password' type='password' value={password} onChange={e => setPassword(e.target.value)} required />
                </div>
                {error && <p style={{color: 'red'}}>{error}</p>}
                <button title='submit' type='submit' disabled={loading}>
                    {loading ? 'Logging in...' : 'login'}
                </button>
            </form> 
        </div>
    )
}

export default Login