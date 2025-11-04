import { Link, useNavigate } from 'react-router-dom'
import { logout, getCurrentUsername } from '../api'

function Header() {
  const navigate = useNavigate()
  const username = getCurrentUsername()

  const handleLogout = () => {
    logout()
    navigate('/login')
    window.location.reload()
  }

  return (
    <header className="header">
      <div className="header-content">
        <Link to="/" className="logo">BlogApp</Link>
        <nav className="nav">
          <Link to="/" className="nav-link">Home</Link>
          <Link to="/create" className="nav-link">Create Post</Link>
          <span className="username">Hello, {username}!</span>
          <button onClick={handleLogout} className="button button-small">
            Logout
          </button>
        </nav>
      </div>
    </header>
  )
}

export default Header
