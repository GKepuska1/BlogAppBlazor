import { useState, useEffect } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { createBlog, checkSubscriptionLimit } from '../api'

function CreateBlog() {
  const navigate = useNavigate()
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [tags, setTags] = useState('')
  const [loading, setLoading] = useState(false)
  const [checkingLimit, setCheckingLimit] = useState(true)
  const [limitReached, setLimitReached] = useState(false)
  const [subscriptionActive, setSubscriptionActive] = useState(false)

  useEffect(() => {
    checkLimit()
  }, [])

  const checkLimit = async () => {
    try {
      const data = await checkSubscriptionLimit()
      setLimitReached(data.limitReached)
      setSubscriptionActive(data.subscriptionActive)
    } catch (error) {
      console.error('Failed to check limit:', error)
    } finally {
      setCheckingLimit(false)
    }
  }

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

      // Check if it's a daily limit error
      if (error.message.includes('limit')) {
        setLimitReached(true)
        alert('You have reached your daily post limit. Upgrade to premium for unlimited posting!')
      } else {
        alert('Failed to create blog post. Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  if (checkingLimit) {
    return <div className="loading">Checking subscription status...</div>
  }

  if (limitReached) {
    return (
      <div className="page-container">
        <div className="paywall-container">
          <h1 className="heading">Daily Limit Reached</h1>

          <div className="paywall-message">
            <p className="paywall-text">
              You've reached your daily limit of <strong>1 blog post per day</strong> for free users.
            </p>

            <div className="paywall-options">
              <h2>Upgrade to Premium</h2>
              <p>Get unlimited blog posts with a one-time Bitcoin payment!</p>

              <div className="premium-benefits">
                <ul>
                  <li>✅ Unlimited blog posts</li>
                  <li>✅ No daily restrictions</li>
                  <li>✅ Lifetime access</li>
                  <li>✅ Only 0.001 BTC</li>
                </ul>
              </div>

              <div className="paywall-actions">
                <Link to="/upgrade" className="button button-large">
                  Upgrade Now with Bitcoin
                </Link>
                <button
                  onClick={() => navigate('/')}
                  className="button button-secondary"
                >
                  Back to Home
                </button>
              </div>
            </div>

            <div className="wait-option">
              <p>Or wait until tomorrow to post again for free!</p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="page-container">
      <div className="form-container">
        <h1 className="heading">Create New Blog Post</h1>

        {subscriptionActive && (
          <div className="premium-badge">
            ⭐ Premium User - Unlimited Posts
          </div>
        )}

        {!subscriptionActive && (
          <div className="info-banner">
            ℹ️ Free users can post 1 blog per day. <Link to="/upgrade">Upgrade for unlimited posting</Link>
          </div>
        )}

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
