export type ParserType = 'LlamaParse' | 'AzureDocumentIntelligence';
export type ChunkingStrategy = 'Semantic' | 'RecursiveCharacter' | 'DocumentAware' | 'StructureAware';
export type EmbeddingModel = 'TextEmbedding3Small' | 'TextEmbedding3Large' | 'AzureOpenAi';
export type ProcessingStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface UploadDocumentResponse {
  documentId: string;
  fileName: string;
  fileSizeBytes: number;
  chunkCount: number;
  status: ProcessingStatus;
  processingTimeMs: number;
  parser: string;
  chunkingStrategy: string;
  embeddingModel: string;
}

export interface DocumentSummary {
  id: string;
  fileName: string;
  fileSizeBytes: number;
  chunkCount: number;
  status: ProcessingStatus;
  parser: string;
  chunkingStrategy: string;
  embeddingModel: string;
  processingTimeMs: number;
  createdAt: string;
  processedAt: string | null;
}

export interface GetAllDocumentsResponse {
  documents: DocumentSummary[];
  total: number;
}
