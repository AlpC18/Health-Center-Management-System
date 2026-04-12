export default function TableSkeleton({ rows = 5, cols = 5 }) {
  return (
    <div className="overflow-hidden">
      <div className="bg-gray-50 border-b border-gray-100 px-4 py-3 flex gap-4">
        {Array.from({ length: cols }).map((_, i) => (
          <div key={i} className="h-3 bg-gray-200 rounded animate-pulse flex-1" />
        ))}
        <div className="h-3 w-16 bg-gray-200 rounded animate-pulse" />
      </div>
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="px-4 py-3.5 border-b border-gray-50 flex gap-4 items-center">
          {Array.from({ length: cols }).map((_, j) => (
            <div
              key={j}
              className="h-3 bg-gray-100 rounded animate-pulse flex-1"
              style={{ animationDelay: `${(i * cols + j) * 50}ms` }}
            />
          ))}
          <div className="flex gap-2 w-16">
            <div className="h-7 w-7 bg-gray-100 rounded-lg animate-pulse" />
            <div className="h-7 w-7 bg-gray-100 rounded-lg animate-pulse" />
          </div>
        </div>
      ))}
    </div>
  )
}
