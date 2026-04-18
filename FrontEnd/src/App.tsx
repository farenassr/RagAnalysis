import './App.css'

function App() {
  return (
    <div className="app">
      <header className="app-header">
        <h1>RAG Evaluation Arena</h1>
        <p className="subtitle">Configure, execute, and compare RAG pipelines empirically</p>
      </header>

      <main className="app-main">
        <section className="workspace-placeholder">
          <div className="coming-soon-card">
            <h2>Experiment Workspace</h2>
            <p>Phase 1 foundation is ready. Upload, configure, and compare RAG pipelines — coming in Phase 2.</p>
            <div className="status-badges">
              <span className="badge badge-green">API: Online</span>
              <span className="badge badge-green">Database: Connected</span>
              <span className="badge badge-green">Telemetry: Active</span>
            </div>
          </div>
        </section>
      </main>
    </div>
  )
}

export default App
