import axios from 'axios'
import type { ExecuteBatchRequest, ExecuteBatchResponse, ExperimentBuilderOptionsResponse } from '../types/experimentBuilder'

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

const api = axios.create({ baseURL: API_BASE })

export async function getExperimentBuilderOptions(): Promise<ExperimentBuilderOptionsResponse> {
  const { data } = await api.get<ExperimentBuilderOptionsResponse>('/api/experiment-builder/options')
  return data
}

export async function executeExperimentBatch(payload: ExecuteBatchRequest): Promise<ExecuteBatchResponse> {
  const { data } = await api.post<ExecuteBatchResponse>('/api/experiments/execute-batch', payload)
  return data
}
