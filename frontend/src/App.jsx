import { useState, useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { guestLogin, setAuthToken, getCurrentUsername } from './api'
import Login from './Login'
import Header from './components/Header'
import Home from './pages/Home'
import BlogPost from './pages/BlogPost'
import CreateBlog from './pages/CreateBlog'
import EditBlog from './pages/EditBlog'
import './App.css'

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    // Check if user is already logged in
    const token = localStorage.getItem('token')
    const username = localStorage.getItem('username')

    if (token && username) {
      setAuthToken(token, username)
      setIsAuthenticated(true)
    }

    setLoading(false)
  }, [])

  const handleLogin = async () => {
    try {
      await guestLogin()
      setIsAuthenticated(true)
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  }

  if (loading) {
    return <div className="loading">Loading...</div>
  }

  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} />
  }

  return (
    <Router>
      <div className="app">
        <Header />
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/blog/:id" element={<BlogPost />} />
            <Route path="/create" element={<CreateBlog />} />
            <Route path="/edit/:id" element={<EditBlog />} />
            <Route path="/login" element={<Navigate to="/" replace />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
