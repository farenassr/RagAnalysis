import { useQuery } from '@tanstack/react-query'
import { FileUpload } from './components/FileUpload/FileUpload'
import { ExperimentBuilder } from './components/ExperimentBuilder/ExperimentBuilder'
import { getAllDocuments } from './api/documentsApi'
import './App.css'

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1048576).toFixed(1)} MB`
}

function App() {
  const documentsQuery = useQuery({
    queryKey: ['documents'],
    queryFn: getAllDocuments,
  })

  const documents = documentsQuery.data?.documents ?? []
  const completedDocuments = documents.filter((document) => document.status === 'Completed').length
  const totalChunks = documents.reduce((sum, document) => sum + document.chunkCount, 0)

  return (
    <div className="app-shell">
      <header className="hero">
        <div className="hero-copy">
          <span className="hero-kicker">Phase 3 · Multi-Case Builder</span>
          <h1>Shared document ingestion for the RAG Evaluation Arena</h1>
          <p>
            Upload a shared document set once, then configure multiple comparison rows against that
            same indexed source set before execution is introduced in phase 4.
          </p>
        </div>

        <div className="hero-stats">
          <article className="stat-card">
            <span className="stat-label">Documents</span>
            <strong className="stat-value">{documents.length}</strong>
          </article>
          <article className="stat-card">
            <span className="stat-label">Completed</span>
            <strong className="stat-value">{completedDocuments}</strong>
          </article>
          <article className="stat-card">
            <span className="stat-label">Chunks Indexed</span>
            <strong className="stat-value">{totalChunks}</strong>
          </article>
        </div>
      </header>

      <main className="workspace-grid">
        <section className="panel panel-upload">
          <div className="panel-header">
            <div>
              <p className="eyebrow">Shared Upload</p>
              <h2>Ingest source files once</h2>
            </div>
            <span className="panel-badge">Shared Source</span>
          </div>
          <p className="panel-copy">
            Phase 2 ingestion remains active here so every experiment row can reuse the exact same
            indexed source set for fair comparison.
          </p>
          <FileUpload />
        </section>

        <section className="panel panel-library">
          <div className="panel-header">
            <div>
              <p className="eyebrow">Processed Library</p>
              <h2>Indexed documents</h2>
            </div>
            <span className="panel-badge panel-badge-secondary">
              {documentsQuery.isFetching ? 'Refreshing' : 'Live'}
            </span>
          </div>
          <p className="panel-copy">
            Review what has already been parsed, chunked, and embedded before moving into the
            multi-case experiment builder.
          </p>

          {documentsQuery.isLoading ? (
            <div className="empty-state">
              <div>
                <h3>Loading document history</h3>
                <p>The workspace is fetching the latest ingestion results.</p>
              </div>
            </div>
          ) : documentsQuery.isError ? (
            <div className="empty-state empty-state-error">
              <div>
                <h3>Document history unavailable</h3>
                <p>{(documentsQuery.error as Error).message}</p>
              </div>
            </div>
          ) : documents.length === 0 ? (
            <div className="empty-state">
              <div>
                <h3>No documents indexed yet</h3>
                <p>Upload a file to create the first ingestion record and verify chunk persistence.</p>
              </div>
            </div>
          ) : (
            <div className="document-list">
              {documents.map((document) => (
                <article key={document.id} className="document-card">
                  <div className="document-main">
                    <div>
                      <p className="document-name">{document.fileName}</p>
                      <p className="document-meta">
                        {formatBytes(document.fileSizeBytes)} · {document.chunkCount} chunks ·{' '}
                        {document.processingTimeMs.toLocaleString()} ms
                      </p>
                    </div>
                    <span className={`status-pill status-${document.status.toLowerCase()}`}>
                      {document.status}
                    </span>
                  </div>

                  <div className="stack-tags">
                    <span>{document.parser}</span>
                    <span>{document.chunkingStrategy}</span>
                    <span>{document.embeddingModel}</span>
                  </div>
                </article>
              ))}
            </div>
          )}
        </section>
      </main>

      <div className="builder-shell">
        <ExperimentBuilder documents={documents} />
      </div>
    </div>
  )
}

export default App
