export interface BuilderOption {
  value: string
  label: string
  description: string
  isAvailable: boolean
  isRecommended: boolean
}

export interface ExperimentBuilderOptionsResponse {
  parsers: BuilderOption[]
  chunkingStrategies: BuilderOption[]
  retrievalTechniques: BuilderOption[]
  embeddingModels: BuilderOption[]
  rerankers: BuilderOption[]
  promptTemplates: BuilderOption[]
  evaluationFrameworks: BuilderOption[]
  databaseTargets: BuilderOption[]
}

export interface ExperimentCaseDraft {
  id: string
  caseLabel: string
  parser: string
  chunkingStrategy: string
  retrievalTechnique: string
  embeddingModel: string
  reranker: string
  promptTemplate: string
  evaluationFramework: string
  databaseTarget: string
  databaseName: string
}

export interface ExecuteBatchExperimentCase {
  caseLabel: string
  parser: string
  chunkingStrategy: string
  retrievalTechnique: string
  embeddingModel: string
  reranker: string
  promptTemplate: string
  evaluationFramework: string
  databaseTarget: string
  databaseName: string
}

export interface ExecuteBatchRequest {
  queryText: string
  sharedDocumentIds: string[]
  cases: ExecuteBatchExperimentCase[]
}

export interface RetrievedChunkResult {
  chunkId: string
  documentId: string
  documentFileName: string
  chunkIndex: number
  score: number
  retrievalSource: string
  contentPreview: string
}

export interface ExecuteBatchCaseResult {
  caseLabel: string
  retrievalTechnique: string
  answer: string
  model: string
  totalLatencyMs: number
  retrievalLatencyMs: number
  generationLatencyMs: number
  usedFallbackAnswer: boolean
  retrievedChunks: RetrievedChunkResult[]
  warnings: string[]
}

export interface ExecuteBatchResponse {
  batchId: string
  queryText: string
  caseCount: number
  results: ExecuteBatchCaseResult[]
}
