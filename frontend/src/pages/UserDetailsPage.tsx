import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export function UserDetailsPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  if (!user) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="page-container">
      <div className="card">
        <h1>My Profile</h1>
        <div className="user-details">
          <div className="detail-row">
            <span className="detail-label">First Name</span>
            <span className="detail-value">{user.firstName}</span>
          </div>
          <div className="detail-row">
            <span className="detail-label">Last Name</span>
            <span className="detail-value">{user.lastName}</span>
          </div>
          <div className="detail-row">
            <span className="detail-label">Email</span>
            <span className="detail-value">{user.email}</span>
          </div>
        </div>
        <button
          type="button"
          className="btn-primary btn-logout"
          onClick={handleLogout}
        >
          Logout
        </button>
      </div>
    </div>
  );
}
