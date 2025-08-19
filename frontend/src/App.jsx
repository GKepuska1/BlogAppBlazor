import { useEffect, useState } from 'react'
import { fetchBlogs, createBlog, login, setAuthToken } from './api'
import Login from './Login'
import './App.css'

function App() {
  const [blogs, setBlogs] = useState([])
  const [title, setTitle] = useState('')
  const [tags, setTags] = useState('')
  const [token, setToken] = useState(localStorage.getItem('token') || '')

  useEffect(() => {
    if (token) {
      setAuthToken(token)
      fetchBlogs().then(setBlogs).catch(console.error)
    }
  }, [token])

  const handleSubmit = async (e) => {
    e.preventDefault()
    const blog = await createBlog({
      title,
      tags: tags.split(',').map((t) => t.trim()),
    })
    setBlogs([...blogs, blog])
    setTitle('')
    setTags('')
  }

  const handleLogin = async (credentials) => {
    const t = await login(credentials)
    setToken(t)
  }

  if (!token) {
    return <Login onLogin={handleLogin} />
  }

  return (
    <div className="container">
      <h1 className="heading">Blogs</h1>
      <form onSubmit={handleSubmit} className="form">
        <input
          placeholder="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <input
          placeholder="Tags comma separated"
          value={tags}
          onChange={(e) => setTags(e.target.value)}
        />
        <button className="button" type="submit">
          Create
        </button>
      </form>
      <ul className="list">
        {blogs.map((b) => (
          <li key={b.id} className="list-item">
            <h2>{b.title}</h2>
            {b.tags && <p className="tags">Tags: {b.tags.join(', ')}</p>}
          </li>
        ))}
      </ul>
    </div>
  )
}

export default App
