import axios from 'axios';
import type {
  GetAllDocumentsResponse,
  UploadDocumentResponse,
  ParserType,
  ChunkingStrategy,
  EmbeddingModel,
} from '../types/documents';

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

const api = axios.create({ baseURL: API_BASE });

export interface UploadDocumentPayload {
  file: File;
  parser: ParserType;
  chunkingStrategy: ChunkingStrategy;
  embeddingModel: EmbeddingModel;
}

export async function uploadDocument(payload: UploadDocumentPayload): Promise<UploadDocumentResponse> {
  const formData = new FormData();
  formData.append('file', payload.file);
  formData.append('parser', payload.parser);
  formData.append('chunkingStrategy', payload.chunkingStrategy);
  formData.append('embeddingModel', payload.embeddingModel);

  const { data } = await api.post<UploadDocumentResponse>('/api/documents/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return data;
}

export async function getAllDocuments(): Promise<GetAllDocumentsResponse> {
  const { data } = await api.get<GetAllDocumentsResponse>('/api/documents');
  return data;
}
