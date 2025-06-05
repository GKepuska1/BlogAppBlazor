import { useEffect, useState } from 'react'
import { fetchBlogs, createBlog } from './api'
import './App.css'

function App() {
  const [blogs, setBlogs] = useState([])
  const [title, setTitle] = useState('')
  const [tags, setTags] = useState('')

  useEffect(() => {
    fetchBlogs().then(setBlogs).catch(console.error)
  }, [])

  const handleSubmit = async (e) => {
    e.preventDefault()
    const blog = await createBlog({ title, tags: tags.split(',').map(t => t.trim()) })
    setBlogs([...blogs, blog])
    setTitle('')
    setTags('')
  }

  return (
    <div className="p-4 max-w-xl mx-auto">
      <h1 className="text-2xl font-bold mb-4">Blogs</h1>
      <form onSubmit={handleSubmit} className="space-y-2 mb-4">
        <input className="border px-2 py-1 w-full" placeholder="Title" value={title} onChange={e => setTitle(e.target.value)} />
        <input className="border px-2 py-1 w-full" placeholder="Tags comma separated" value={tags} onChange={e => setTags(e.target.value)} />
        <button className="bg-blue-500 text-white px-4 py-1 rounded" type="submit">Create</button>
      </form>
      <ul className="space-y-2">
        {blogs.map(b => (
          <li key={b.id} className="border p-2 rounded">
            <h2 className="font-semibold">{b.title}</h2>
            {b.tags && <p className="text-sm text-gray-500">Tags: {b.tags.join(', ')}</p>}
          </li>
        ))}
      </ul>
    </div>
  )
}

export default App
