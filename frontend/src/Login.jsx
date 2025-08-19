import { useState } from 'react'
import './App.css'

function Login({ onLogin }) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    await onLogin({ username, password })
    setUsername('')
    setPassword('')
  }

  return (
    <div className="container">
      <h1 className="heading">Login</h1>
      <form onSubmit={handleSubmit} className="form">
        <input
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <button className="button" type="submit">
          Login
        </button>
      </form>
    </div>
  )
}

export default Login

