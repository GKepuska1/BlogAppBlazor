import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { getBitcoinPaymentInfo, verifyBitcoinPayment, checkSubscriptionLimit } from '../api'

function Upgrade() {
  const navigate = useNavigate()
  const [paymentInfo, setPaymentInfo] = useState(null)
  const [transactionId, setTransactionId] = useState('')
  const [loading, setLoading] = useState(true)
  const [verifying, setVerifying] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)
  const [subscriptionActive, setSubscriptionActive] = useState(false)
  const [copied, setCopied] = useState(false)

  useEffect(() => {
    loadPaymentInfo()
    checkSubscription()
  }, [])

  const checkSubscription = async () => {
    try {
      const data = await checkSubscriptionLimit()
      setSubscriptionActive(data.subscriptionActive)
    } catch (error) {
      console.error('Failed to check subscription:', error)
    }
  }

  const loadPaymentInfo = async () => {
    try {
      const data = await getBitcoinPaymentInfo()
      setPaymentInfo(data)
    } catch (error) {
      console.error('Failed to load payment info:', error)
      setError('Failed to load payment information')
    } finally {
      setLoading(false)
    }
  }

  const handleCopyAddress = () => {
    if (paymentInfo?.address) {
      navigator.clipboard.writeText(paymentInfo.address)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  const handleVerifyPayment = async (e) => {
    e.preventDefault()

    if (!transactionId.trim()) {
      setError('Please enter a transaction ID')
      return
    }

    setVerifying(true)
    setError('')

    try {
      const result = await verifyBitcoinPayment(transactionId)
      setSuccess(true)
      setSubscriptionActive(true)

      setTimeout(() => {
        navigate('/')
      }, 3000)
    } catch (error) {
      setError(error.message || 'Payment verification failed')
    } finally {
      setVerifying(false)
    }
  }

  if (loading) {
    return <div className="loading">Loading payment information...</div>
  }

  if (subscriptionActive && !success) {
    return (
      <div className="page-container">
        <div className="upgrade-container">
          <div className="success-message">
            <h1>✅ You're Already Premium!</h1>
            <p>You have unlimited posting privileges.</p>
            <button onClick={() => navigate('/')} className="button">
              Go to Home
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="page-container">
      <div className="upgrade-container">
        {success ? (
          <div className="success-message">
            <h1>🎉 Payment Verified!</h1>
            <p>Your subscription is now active. You have unlimited posting!</p>
            <p className="redirect-message">Redirecting to home...</p>
          </div>
        ) : (
          <>
            <h1 className="heading">Upgrade to Premium</h1>

            <div className="features-box">
              <h2>Premium Benefits</h2>
              <ul className="features-list">
                <li>✅ Unlimited blog posts per day</li>
                <li>✅ No restrictions</li>
                <li>✅ Lifetime access</li>
                <li>✅ Support the platform</li>
              </ul>
            </div>

            <div className="payment-section">
              <h2>Pay with Bitcoin</h2>

              <div className="payment-info">
                <div className="info-item">
                  <label>Amount:</label>
                  <div className="amount-display">
                    {paymentInfo?.amount} {paymentInfo?.currency}
                  </div>
                </div>

                <div className="info-item">
                  <label>Bitcoin Address:</label>
                  <div className="address-container">
                    <code className="bitcoin-address">{paymentInfo?.address}</code>
                    <button
                      onClick={handleCopyAddress}
                      className="button button-small"
                      type="button"
                    >
                      {copied ? '✓ Copied!' : 'Copy'}
                    </button>
                  </div>
                </div>
              </div>

              <div className="instructions">
                <h3>Payment Instructions:</h3>
                <ol>
                  <li>Send exactly <strong>{paymentInfo?.amount} BTC</strong> to the address above</li>
                  <li>Wait for the transaction to be broadcast to the network</li>
                  <li>Copy your transaction ID from your wallet</li>
                  <li>Paste it below and click "Verify Payment"</li>
                </ol>
              </div>

              <form onSubmit={handleVerifyPayment} className="verification-form">
                <div className="form-group">
                  <label htmlFor="txId">Transaction ID</label>
                  <input
                    id="txId"
                    type="text"
                    value={transactionId}
                    onChange={(e) => setTransactionId(e.target.value)}
                    placeholder="Enter your Bitcoin transaction ID..."
                    className="input"
                    required
                  />
                  <small className="help-text">
                    The transaction ID (txid) can be found in your Bitcoin wallet after sending the payment
                  </small>
                </div>

                {error && <div className="error-message">{error}</div>}

                <div className="form-actions">
                  <button type="submit" className="button" disabled={verifying}>
                    {verifying ? 'Verifying...' : 'Verify Payment'}
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

              <div className="demo-note">
                <p><strong>Demo Mode:</strong> For testing, you can enter any transaction ID with at least 10 characters.</p>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default Upgrade
