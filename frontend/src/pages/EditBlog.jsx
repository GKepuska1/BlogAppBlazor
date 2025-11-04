import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getBlogById, updateBlog } from '../api'

function EditBlog() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [tags, setTags] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    loadBlog()
  }, [id])

  const loadBlog = async () => {
    try {
      const blog = await getBlogById(id)
      setTitle(blog.name)
      setContent(blog.content || '')
      setTags(blog.tags ? blog.tags.map((t) => t.name).join(', ') : '')
    } catch (error) {
      console.error('Failed to load blog:', error)
      alert('Failed to load blog or you do not have permission to edit it.')
      navigate('/')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()

    if (!title.trim() || !content.trim()) {
      alert('Title and content are required!')
      return
    }

    setSaving(true)
    try {
      const tagList = tags
        .split(',')
        .map((t) => t.trim())
        .filter((t) => t)
        .map((name) => ({ name }))

      await updateBlog(id, {
        name: title,
        content,
        tags: tagList,
      })

      alert('Blog post updated successfully!')
      navigate(`/blog/${id}`)
    } catch (error) {
      console.error('Failed to update blog:', error)
      alert('Failed to update blog post. You may not have permission.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return <div className="loading">Loading...</div>
  }

  return (
    <div className="page-container">
      <div className="form-container">
        <h1 className="heading">Edit Blog Post</h1>

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
            <button type="submit" className="button" disabled={saving}>
              {saving ? 'Saving...' : 'Save Changes'}
            </button>
            <button
              type="button"
              onClick={() => navigate(`/blog/${id}`)}
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

export default EditBlog
