import { useNavigate } from "react-router-dom";

interface Props {
  onLogout: () => void;
}

function Navbar({ onLogout }: Props) {
  const navigate = useNavigate();

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        {/* Left side — brand/app name */}
        <span className="navbar-brand">Taskly</span>

        {/* Right side — navigation links */}
        <div className="navbar-links">
          <button className="nav-link" onClick={() => navigate("/")}>
            home
          </button>
          <button className="nav-link nav-link-logout" onClick={onLogout}>
            logout
          </button>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
