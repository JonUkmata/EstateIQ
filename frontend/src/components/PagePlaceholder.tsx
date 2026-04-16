type PagePlaceholderProps = {
  badge: string
  title: string
  description: string
  highlights: string[]
}

export default function PagePlaceholder({
  badge,
  title,
  description,
  highlights,
}: PagePlaceholderProps) {
  return (
    <section className="content-stack">
      <article className="hero-card">
        <div className="hero-copy">
          <span className="eyebrow">{badge}</span>
          <h1>{title}</h1>
          <p className="lead">{description}</p>
        </div>

        <aside className="hero-panel">
          <p className="panel-label">Highlights</p>
          <ul className="panel-list">
            {highlights.map((highlight) => (
              <li key={highlight}>{highlight}</li>
            ))}
          </ul>
        </aside>
      </article>
    </section>
  )
}
