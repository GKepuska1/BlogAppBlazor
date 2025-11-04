import { useState } from 'react'
import './App.css'

function Login({ onLogin }) {
  const [loading, setLoading] = useState(false)

  const handleGuestLogin = async () => {
    setLoading(true)
    try {
      await onLogin()
    } catch (error) {
      console.error('Login failed:', error)
      alert('Failed to login as guest. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container">
      <div className="welcome-container">
        <h1 className="heading">Welcome to BlogApp</h1>
        <p className="welcome-text">
          A simple blogging platform where you can share your thoughts and read what others have to say.
        </p>
        <button
          className="button button-large"
          onClick={handleGuestLogin}
          disabled={loading}
        >
          {loading ? 'Logging in...' : 'Login as Guest'}
        </button>
        <p className="info-text">
          Click the button above to get started with a randomly generated username!
        </p>
      </div>
    </div>
  )
}

export default Login
