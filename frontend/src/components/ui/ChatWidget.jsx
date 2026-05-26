import { useState, useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import { MessageSquare, X, Send } from 'lucide-react'
import useAuthStore from '../../store/authStore'

export default function ChatWidget() {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState([])
  const [input, setInput] = useState('')
  const { user } = useAuthStore()
  const messagesEndRef = useRef(null)
  const connectionRef = useRef(null)

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5077/notificationHub")
      .withAutomaticReconnect()
      .build()

    connectionRef.current = newConnection
    newConnection.on("ReceiveMessage", (sender, message) => {
      setMessages(prev => [...prev, { sender, message, time: new Date() }])
    })
    newConnection.start().catch(e => console.error("Chat Connection Failed: ", e))

    return () => {
      newConnection.off("ReceiveMessage")
      newConnection.stop().catch(e => console.error("Chat Disconnect Failed: ", e))
      connectionRef.current = null
    }
  }, [])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, isOpen])

  const sendMessage = async (e) => {
    e.preventDefault()
    const connection = connectionRef.current
    if (!input.trim() || !connection) return

    try {
      await connection.invoke("SendMessage", user?.firstName || "Klient", input)
      setInput('')
    } catch (e) {
      console.error(e)
    }
  }

  return (
    <>
      {/* Floating Button */}
      <button
        onClick={() => setIsOpen(true)}
        className={`btn-primary fixed bottom-6 right-6 !p-4 z-40 ${
          isOpen ? 'scale-0 opacity-0' : 'scale-100 opacity-100'
        }`}
        aria-label="Open chat"
      >
        <MessageSquare className="w-6 h-6" />
      </button>

      {/* Chat Window */}
      <div
        className={`fixed bottom-6 right-6 w-80 sm:w-96 card flex flex-col transition-all origin-bottom-right z-50 ${
          isOpen ? 'scale-100 opacity-100' : 'scale-0 opacity-0 pointer-events-none'
        }`}
      >
        {/* Header */}
        <div
          className="flex items-center justify-between p-4 text-white rounded-t-[22px] relative overflow-hidden"
          style={{
            background: 'var(--glow-primary)',
            boxShadow: 'inset 0 1px 0 rgba(255,255,255,0.45), inset 0 -8px 18px rgba(0,0,0,0.2)',
          }}
        >
          <div className="flex flex-col relative z-10">
            <span className="font-bold">Mbështetja Live</span>
            <span className="text-xs opacity-90 flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-emerald-300 animate-pulse"></span> Online
            </span>
          </div>
          <button onClick={() => setIsOpen(false)} className="p-1 hover:bg-white/20 rounded-lg transition-colors relative z-10">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Messages */}
        <div className="h-80 overflow-y-auto p-4 flex flex-col gap-3">
          {messages.length === 0 ? (
            <div className="m-auto text-center text-health-secondary text-sm">
              <MessageSquare className="w-8 h-8 mx-auto mb-2 opacity-20" />
              Na shkruani për çdo pyetje!
            </div>
          ) : (
            messages.map((msg, i) => {
              const isMe = msg.sender === (user?.firstName || "Klient")
              return (
                <div key={i} className={`flex flex-col ${isMe ? 'items-end' : 'items-start'}`}>
                  <span className="text-[10px] text-health-secondary mb-1 ml-1">{msg.sender}</span>
                  <div
                    className={`px-4 py-2 max-w-[85%] text-sm rounded-2xl ${isMe ? 'text-white rounded-br-none' : 'text-health-primary rounded-bl-none'}`}
                    style={
                      isMe
                        ? { background: 'var(--glow-primary)', boxShadow: '0 6px 16px -6px color-mix(in srgb, var(--health-brand) 60%, transparent), inset 0 1px 0 rgba(255,255,255,0.4)' }
                        : { background: 'var(--glass-tint-strong)', border: '1px solid var(--glass-border)', backdropFilter: 'blur(12px)' }
                    }
                  >
                    {msg.message}
                  </div>
                </div>
              )
            })
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Input */}
        <form onSubmit={sendMessage} className="p-3 border-t border-health-border">
          <div className="relative flex items-center">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Shkruaj mesazhin..."
              className="input !pr-12"
            />
            <button
              type="submit"
              disabled={!input.trim()}
              className="absolute right-1.5 p-2 text-white rounded-full disabled:opacity-40 disabled:hidden transition-all"
              style={{
                background: 'var(--glow-primary)',
                boxShadow: '0 6px 16px -6px color-mix(in srgb, var(--health-brand) 65%, transparent), inset 0 1px 0 rgba(255,255,255,0.45)',
              }}
            >
              <Send className="w-4 h-4 ml-0.5" />
            </button>
          </div>
        </form>
      </div>
    </>
  )
}
