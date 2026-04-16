const pillars = [
  {
    title: 'React + Vite',
    description: 'Frontend-i eshte inicializuar me Vite dhe React per nje workflow te shpejte lokal.',
  },
  {
    title: 'Home Page',
    description: 'Faqja kryesore shfaqet menjehere dhe mund te perdoret si baze per feature-t e ardhshme.',
  },
  {
    title: 'Ready To Build',
    description: 'Struktura eshte gati per zhvillim, styling dhe integrim me backend-in e EstateIQ.',
  },
]

const steps = [
  'Hape dosjen frontend',
  'Ekzekuto npm install',
  'Nis projektin me npm run dev',
]

export default function App() {
  return (
    <main className="page-shell">
      <section className="hero-card">
        <div className="hero-copy">
          <span className="eyebrow">EstateIQ Frontend</span>
          <h1>Home page-i i pare eshte gati per zhvillim lokal.</h1>
          <p className="lead">
            Kjo eshte baze e thjeshte dhe e paster per frontend-in e projektit. Projekti
            run-on me React + Vite dhe mund te zgjerohet pa prekur setup-in fillestar.
          </p>

          <div className="status-row">
            <div className="status-pill">
              <strong>Status</strong>
              <span>Ready locally</span>
            </div>
            <div className="status-pill">
              <strong>Stack</strong>
              <span>React 19 + Vite 8</span>
            </div>
          </div>
        </div>

        <aside className="hero-panel">
          <p className="panel-label">Quick Start</p>
          <ol>
            {steps.map((step) => (
              <li key={step}>{step}</li>
            ))}
          </ol>
        </aside>
      </section>

      <section className="pillars-grid" aria-label="Frontend highlights">
        {pillars.map((pillar) => (
          <article className="pillar-card" key={pillar.title}>
            <h2>{pillar.title}</h2>
            <p>{pillar.description}</p>
          </article>
        ))}
      </section>
    </main>
  )
}
