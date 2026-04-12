import { useState, useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import { MessageSquare, X, Send } from 'lucide-react'
import useAuthStore from '../../store/authStore'

export default function ChatWidget() {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState([])
  const [input, setInput] = useState('')
  const [connection, setConnection] = useState(null)
  const { user } = useAuthStore()
  const messagesEndRef = useRef(null)

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5077/notificationHub")
      .withAutomaticReconnect()
      .build()

    setConnection(newConnection)
  }, [])

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          connection.on("ReceiveMessage", (sender, message) => {
            setMessages(prev => [...prev, { sender, message, time: new Date() }])
          })
        })
        .catch(e => console.error("Chat Connection Failed: ", e))
    }
  }, [connection])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, isOpen])

  const sendMessage = async (e) => {
    e.preventDefault()
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
        className={`fixed bottom-6 right-6 p-4 rounded-full shadow-2xl transition-all hover:scale-105 z-40 ${
          isOpen ? 'scale-0 opacity-0' : 'scale-100 opacity-100 bg-health-brand text-white'
        }`}
      >
        <MessageSquare className="w-6 h-6" />
      </button>

      {/* Chat Window */}
      <div
        className={`fixed bottom-6 right-6 w-80 sm:w-96 bg-white border border-health-border rounded-2xl shadow-2xl flex flex-col transition-all origin-bottom-right z-50 ${
          isOpen ? 'scale-100 opacity-100' : 'scale-0 opacity-0 pointer-events-none'
        }`}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-health-border bg-gradient-to-r from-health-brand to-health-accent text-white rounded-t-2xl">
          <div className="flex flex-col">
            <span className="font-bold">Mbështetja Live</span>
            <span className="text-xs text-green-100 flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-green-400 animate-pulse"></span> Online
            </span>
          </div>
          <button onClick={() => setIsOpen(false)} className="p-1 hover:bg-white/20 rounded-lg transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Messages */}
        <div className="h-80 overflow-y-auto p-4 flex flex-col gap-3 bg-gray-50/50">
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
                  <div className={`px-4 py-2 rounded-2xl max-w-[85%] text-sm ${
                    isMe 
                      ? 'bg-health-brand text-white rounded-br-none' 
                      : 'bg-white border border-health-border text-health-primary rounded-bl-none shadow-sm'
                  }`}>
                    {msg.message}
                  </div>
                </div>
              )
            })
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Input */}
        <form onSubmit={sendMessage} className="p-3 bg-white border-t border-health-border rounded-b-2xl">
          <div className="relative flex items-center">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Shkruaj mesazhin..."
              className="w-full pl-4 pr-12 py-2.5 bg-gray-50 border border-health-border rounded-full text-sm focus:outline-none focus:border-health-brand transition-colors"
            />
            <button
              type="submit"
              disabled={!input.trim()}
              className="absolute right-1.5 p-1.5 text-white bg-health-brand rounded-full hover:bg-health-brand/90 disabled:opacity-50 disabled:hidden transition-all"
            >
              <Send className="w-4 h-4 ml-0.5" />
            </button>
          </div>
        </form>
      </div>
    </>
  )
}
