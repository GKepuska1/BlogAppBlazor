import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import {
  getBlogById,
  deleteBlog,
  getComments,
  createComment,
  updateComment,
  deleteComment,
  getCurrentUsername,
} from '../api'

function BlogPost() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [blog, setBlog] = useState(null)
  const [comments, setComments] = useState([])
  const [newComment, setNewComment] = useState('')
  const [editingCommentId, setEditingCommentId] = useState(null)
  const [editCommentContent, setEditCommentContent] = useState('')
  const [loading, setLoading] = useState(true)
  const currentUsername = getCurrentUsername()

  useEffect(() => {
    loadBlog()
    loadComments()
  }, [id])

  const loadBlog = async () => {
    try {
      const data = await getBlogById(id)
      setBlog(data)
    } catch (error) {
      console.error('Failed to load blog:', error)
      alert('Blog not found')
      navigate('/')
    } finally {
      setLoading(false)
    }
  }

  const loadComments = async () => {
    try {
      const data = await getComments(id)
      setComments(data)
    } catch (error) {
      console.error('Failed to load comments:', error)
    }
  }

  const handleDeleteBlog = async () => {
    if (!window.confirm('Are you sure you want to delete this blog post?')) {
      return
    }

    try {
      await deleteBlog(id)
      alert('Blog deleted successfully!')
      navigate('/')
    } catch (error) {
      console.error('Failed to delete blog:', error)
      alert('Failed to delete blog. You may not have permission.')
    }
  }

  const handleAddComment = async (e) => {
    e.preventDefault()
    if (!newComment.trim()) return

    try {
      await createComment(id, newComment)
      setNewComment('')
      loadComments()
    } catch (error) {
      console.error('Failed to add comment:', error)
      alert('Failed to add comment')
    }
  }

  const handleEditComment = (comment) => {
    setEditingCommentId(comment.id)
    setEditCommentContent(comment.content)
  }

  const handleUpdateComment = async (commentId) => {
    if (!editCommentContent.trim()) return

    try {
      await updateComment(id, commentId, editCommentContent)
      setEditingCommentId(null)
      setEditCommentContent('')
      loadComments()
    } catch (error) {
      console.error('Failed to update comment:', error)
      alert('Failed to update comment. You may not have permission.')
    }
  }

  const handleDeleteComment = async (commentId) => {
    if (!window.confirm('Are you sure you want to delete this comment?')) {
      return
    }

    try {
      await deleteComment(id, commentId)
      loadComments()
    } catch (error) {
      console.error('Failed to delete comment:', error)
      alert('Failed to delete comment. You may not have permission.')
    }
  }

  if (loading) {
    return <div className="loading">Loading...</div>
  }

  if (!blog) {
    return <div className="error">Blog not found</div>
  }

  const isOwner = blog.user === currentUsername

  return (
    <div className="page-container">
      <div className="blog-detail">
        <div className="blog-header-section">
          <h1 className="blog-detail-title">{blog.name}</h1>
          <div className="blog-actions">
            {isOwner && (
              <>
                <Link to={`/edit/${blog.id}`} className="button button-small">
                  Edit
                </Link>
                <button
                  onClick={handleDeleteBlog}
                  className="button button-small button-danger"
                >
                  Delete
                </button>
              </>
            )}
          </div>
        </div>

        <div className="blog-meta">
          <span className="blog-author">by {blog.user}</span>
          <span className="blog-date">
            {new Date(blog.createdAt).toLocaleDateString()}
          </span>
        </div>

        {blog.tags && blog.tags.length > 0 && (
          <div className="tags">
            {blog.tags.map((tag, idx) => (
              <span key={idx} className="tag">
                {tag.name}
              </span>
            ))}
          </div>
        )}

        <div className="blog-content">{blog.content}</div>

        <div className="comments-section">
          <h2>Comments ({comments.length})</h2>

          <form onSubmit={handleAddComment} className="comment-form">
            <textarea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="Add a comment..."
              className="comment-textarea"
              rows="3"
            />
            <button type="submit" className="button">
              Post Comment
            </button>
          </form>

          <div className="comments-list">
            {comments.map((comment) => (
              <div key={comment.id} className="comment">
                <div className="comment-header">
                  <span className="comment-author">{comment.username}</span>
                  <span className="comment-date">
                    {new Date(comment.createdAt).toLocaleDateString()}
                    {comment.isEdited && ' (edited)'}
                  </span>
                </div>

                {editingCommentId === comment.id ? (
                  <div className="comment-edit">
                    <textarea
                      value={editCommentContent}
                      onChange={(e) => setEditCommentContent(e.target.value)}
                      className="comment-textarea"
                      rows="3"
                    />
                    <div className="comment-actions">
                      <button
                        onClick={() => handleUpdateComment(comment.id)}
                        className="button button-small"
                      >
                        Save
                      </button>
                      <button
                        onClick={() => setEditingCommentId(null)}
                        className="button button-small button-secondary"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : (
                  <>
                    <p className="comment-content">{comment.content}</p>
                    {comment.username === currentUsername && (
                      <div className="comment-actions">
                        <button
                          onClick={() => handleEditComment(comment)}
                          className="button button-small"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDeleteComment(comment.id)}
                          className="button button-small button-danger"
                        >
                          Delete
                        </button>
                      </div>
                    )}
                  </>
                )}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}

export default BlogPost
