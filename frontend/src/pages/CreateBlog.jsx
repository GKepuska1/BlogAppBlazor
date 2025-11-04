import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createBlog } from '../api'

function CreateBlog() {
  const navigate = useNavigate()
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [tags, setTags] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()

    if (!title.trim() || !content.trim()) {
      alert('Title and content are required!')
      return
    }

    setLoading(true)
    try {
      const tagList = tags
        .split(',')
        .map((t) => t.trim())
        .filter((t) => t)
        .map((name) => ({ name }))

      const blog = await createBlog({
        name: title,
        content,
        tags: tagList,
      })

      alert('Blog post created successfully!')
      navigate(`/blog/${blog.id}`)
    } catch (error) {
      console.error('Failed to create blog:', error)
      alert('Failed to create blog post. You may have reached your daily limit.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page-container">
      <div className="form-container">
        <h1 className="heading">Create New Blog Post</h1>

        <form onSubmit={handleSubmit} className="blog-form">
          <div className="form-group">
            <label htmlFor="title">Title</label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Enter blog title..."
              className="input"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="content">Content</label>
            <textarea
              id="content"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="Write your blog content here..."
              className="textarea"
              rows="12"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="tags">Tags (comma separated)</label>
            <input
              id="tags"
              type="text"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="e.g., Technology, Travel, Food"
              className="input"
            />
          </div>

          <div className="form-actions">
            <button type="submit" className="button" disabled={loading}>
              {loading ? 'Creating...' : 'Create Blog Post'}
            </button>
            <button
              type="button"
              onClick={() => navigate('/')}
              className="button button-secondary"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

export default CreateBlog
