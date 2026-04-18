import { create } from 'zustand'
import type { ExecuteBatchResponse, ExperimentCaseDraft } from '../types/experimentBuilder'

interface ExperimentBuilderStore {
  cases: ExperimentCaseDraft[]
  lastExecutionResult: ExecuteBatchResponse | null
  addCase: () => void
  updateCase: (caseId: string, patch: Partial<ExperimentCaseDraft>) => void
  removeCase: (caseId: string) => void
  setLastExecutionResult: (payload: ExecuteBatchResponse | null) => void
}

function toCaseLabel(index: number) {
  return `Case ${String.fromCharCode(65 + index)}`
}

function createDefaultCase(index: number): ExperimentCaseDraft {
  return {
    id: crypto.randomUUID(),
    caseLabel: toCaseLabel(index),
    parser: 'LlamaParse',
    chunkingStrategy: 'Semantic',
    retrievalTechnique: 'DenseVector',
    embeddingModel: 'TextEmbedding3Small',
    reranker: 'None',
    promptTemplate: 'GroundedAnswer',
    evaluationFramework: 'ArenaBaseline',
    databaseTarget: 'PostgreSqlPgVector',
    databaseName: `ragarena_case_${String.fromCharCode(97 + index)}`,
  }
}

function relabelCases(cases: ExperimentCaseDraft[]) {
  return cases.map((item, index) => ({
    ...item,
    caseLabel: toCaseLabel(index),
  }))
}

export const useExperimentBuilderStore = create<ExperimentBuilderStore>((set) => ({
  cases: [createDefaultCase(0)],
  lastExecutionResult: null,
  addCase: () =>
    set((state) => {
      const nextCases = [...state.cases, createDefaultCase(state.cases.length)]
      return { cases: relabelCases(nextCases) }
    }),
  updateCase: (caseId, patch) =>
    set((state) => ({
      cases: state.cases.map((item) => (item.id === caseId ? { ...item, ...patch } : item)),
    })),
  removeCase: (caseId) =>
    set((state) => {
      const nextCases = state.cases.filter((item) => item.id !== caseId)
      const ensuredCases = nextCases.length > 0 ? nextCases : [createDefaultCase(0)]
      return { cases: relabelCases(ensuredCases) }
    }),
  setLastExecutionResult: (payload) => set({ lastExecutionResult: payload }),
}))
