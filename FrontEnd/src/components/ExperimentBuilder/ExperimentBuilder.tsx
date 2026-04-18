import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { executeExperimentBatch, getExperimentBuilderOptions } from '../../api/experimentBuilderApi'
import { useExperimentBuilderStore } from '../../store/experimentBuilderStore'
import type {
  BuilderOption,
  ExecuteBatchRequest,
  ExperimentBuilderOptionsResponse,
} from '../../types/experimentBuilder'
import type { DocumentSummary } from '../../types/documents'
import styles from './ExperimentBuilder.module.css'

const caseSchema = z.object({
  parser: z.string().min(1),
  chunkingStrategy: z.string().min(1),
  retrievalTechnique: z.string().min(1),
  embeddingModel: z.string().min(1),
  reranker: z.string().min(1),
  promptTemplate: z.string().min(1),
  evaluationFramework: z.string().min(1),
  databaseTarget: z.string().min(1),
  databaseName: z
    .string()
    .trim()
    .min(3, 'Database name must be at least 3 characters.')
    .regex(/^[a-z0-9_]+$/i, 'Database name can only contain letters, numbers, and underscores.'),
})

type ExperimentCaseFormValues = z.infer<typeof caseSchema>

const batchSchema = z.object({
  queryText: z.string().trim().min(8, 'Enter a realistic evaluation question before running the batch.'),
  sharedDocumentIds: z.array(z.string()).min(1, 'Upload at least one document before running the arena.'),
  cases: z
    .array(
      z.object({
        caseLabel: z.string().min(1),
        parser: z.string().min(1),
        chunkingStrategy: z.string().min(1),
        retrievalTechnique: z.string().min(1),
        embeddingModel: z.string().min(1),
        reranker: z.string().min(1),
        promptTemplate: z.string().min(1),
        evaluationFramework: z.string().min(1),
        databaseTarget: z.string().min(1),
        databaseName: z.string().trim().min(3),
      })
    )
    .min(1, 'At least one comparison case is required.'),
})

interface ExperimentBuilderProps {
  documents: DocumentSummary[]
}

interface ExperimentCaseCardProps {
  caseLabel: string
  canRemove: boolean
  initialValues: ExperimentCaseFormValues
  options: ExperimentBuilderOptionsResponse
  onRemove: () => void
  onPatch: (patch: ExperimentCaseFormValues) => void
}

function OptionHint({ option }: { option?: BuilderOption }) {
  if (!option) {
    return null
  }

  return (
    <p className={styles.optionHint}>
      {option.description}
      {!option.isAvailable ? <span className={styles.optionPending}>Phase 4+</span> : null}
    </p>
  )
}

function OptionSelect({
  label,
  value,
  values,
  onChange,
}: {
  label: string
  value: string
  values: BuilderOption[]
  onChange: (value: string) => void
}) {
  const selectedOption = values.find((item) => item.value === value)

  return (
    <div className={styles.field}>
      <label className={styles.label}>{label}</label>
      <select className={styles.select} value={value} onChange={(event) => onChange(event.target.value)}>
        {values.map((option) => (
          <option key={option.value} value={option.value} disabled={!option.isAvailable}>
            {option.label}
            {!option.isAvailable ? ' (Future)' : ''}
          </option>
        ))}
      </select>
      <OptionHint option={selectedOption} />
    </div>
  )
}

function ExperimentCaseCard({
  caseLabel,
  canRemove,
  initialValues,
  options,
  onRemove,
  onPatch,
}: ExperimentCaseCardProps) {
  const form = useForm<ExperimentCaseFormValues>({
    resolver: zodResolver(caseSchema),
    defaultValues: initialValues,
    mode: 'onChange',
  })

  useEffect(() => {
    const subscription = form.watch((value) => {
      onPatch({
        parser: value.parser ?? initialValues.parser,
        chunkingStrategy: value.chunkingStrategy ?? initialValues.chunkingStrategy,
        retrievalTechnique: value.retrievalTechnique ?? initialValues.retrievalTechnique,
        embeddingModel: value.embeddingModel ?? initialValues.embeddingModel,
        reranker: value.reranker ?? initialValues.reranker,
        promptTemplate: value.promptTemplate ?? initialValues.promptTemplate,
        evaluationFramework: value.evaluationFramework ?? initialValues.evaluationFramework,
        databaseTarget: value.databaseTarget ?? initialValues.databaseTarget,
        databaseName: value.databaseName ?? initialValues.databaseName,
      })
    })

    return () => subscription.unsubscribe()
  }, [form, initialValues, onPatch])

  const values = form.watch()

  return (
    <article className={styles.caseCard}>
      <div className={styles.caseHeader}>
        <div>
          <p className={styles.caseEyebrow}>Comparison Row</p>
          <h3>{caseLabel}</h3>
        </div>
        <button type="button" className={styles.caseAction} onClick={onRemove} disabled={!canRemove}>
          Remove
        </button>
      </div>

      <div className={styles.caseGrid}>
        <OptionSelect
          label="Parser"
          value={values.parser}
          values={options.parsers}
          onChange={(value) => form.setValue('parser', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Chunking"
          value={values.chunkingStrategy}
          values={options.chunkingStrategies}
          onChange={(value) => form.setValue('chunkingStrategy', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Retrieval"
          value={values.retrievalTechnique}
          values={options.retrievalTechniques}
          onChange={(value) => form.setValue('retrievalTechnique', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Embedding"
          value={values.embeddingModel}
          values={options.embeddingModels}
          onChange={(value) => form.setValue('embeddingModel', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Reranker"
          value={values.reranker}
          values={options.rerankers}
          onChange={(value) => form.setValue('reranker', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Prompt Template"
          value={values.promptTemplate}
          values={options.promptTemplates}
          onChange={(value) => form.setValue('promptTemplate', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Evaluation"
          value={values.evaluationFramework}
          values={options.evaluationFrameworks}
          onChange={(value) => form.setValue('evaluationFramework', value, { shouldValidate: true })}
        />
        <OptionSelect
          label="Database Target"
          value={values.databaseTarget}
          values={options.databaseTargets}
          onChange={(value) => form.setValue('databaseTarget', value, { shouldValidate: true })}
        />

        <div className={styles.field}>
          <label className={styles.label}>Database Name</label>
          <input className={styles.input} {...form.register('databaseName')} placeholder="ragarena_case_a" />
          {form.formState.errors.databaseName ? (
            <p className={styles.validationError}>{form.formState.errors.databaseName.message}</p>
          ) : (
            <p className={styles.optionHint}>Use a distinct name per case for clean comparisons.</p>
          )}
        </div>
      </div>
    </article>
  )
}

export function ExperimentBuilder({ documents }: ExperimentBuilderProps) {
  const [queryText, setQueryText] = useState('')

  const optionsQuery = useQuery({
    queryKey: ['experiment-builder-options'],
    queryFn: getExperimentBuilderOptions,
    staleTime: 5 * 60_000,
  })

  const cases = useExperimentBuilderStore((state) => state.cases)
  const addCase = useExperimentBuilderStore((state) => state.addCase)
  const updateCase = useExperimentBuilderStore((state) => state.updateCase)
  const removeCase = useExperimentBuilderStore((state) => state.removeCase)
  const lastExecutionResult = useExperimentBuilderStore((state) => state.lastExecutionResult)
  const setLastExecutionResult = useExperimentBuilderStore((state) => state.setLastExecutionResult)

  const completedDocuments = useMemo(
    () => documents.filter((document) => document.status === 'Completed'),
    [documents]
  )

  const executeMutation = useMutation({
    mutationFn: executeExperimentBatch,
    onSuccess: (result) => {
      setLastExecutionResult(result)
    },
  })

  const runBatch = () => {
    const payload: ExecuteBatchRequest = {
      queryText,
      sharedDocumentIds: completedDocuments.map((document) => document.id),
      cases: cases.map((item) => ({
        caseLabel: item.caseLabel,
        parser: item.parser,
        chunkingStrategy: item.chunkingStrategy,
        retrievalTechnique: item.retrievalTechnique,
        embeddingModel: item.embeddingModel,
        reranker: item.reranker,
        promptTemplate: item.promptTemplate,
        evaluationFramework: item.evaluationFramework,
        databaseTarget: item.databaseTarget,
        databaseName: item.databaseName.trim(),
      })),
    }

    const parsedPayload = batchSchema.safeParse(payload)
    if (!parsedPayload.success) {
      const firstIssue = parsedPayload.error.issues[0]?.message ?? 'Batch validation failed.'
      window.alert(firstIssue)
      return
    }

    executeMutation.mutate(parsedPayload.data)
  }

  const builderOptions = optionsQuery.data

  return (
    <section className="panel">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Phase 4 Workspace</p>
          <h2>Execution engine and orchestration</h2>
        </div>
        <span className="panel-badge">
          {cases.length} {cases.length === 1 ? 'Case' : 'Cases'}
        </span>
      </div>
      <p className="panel-copy">
        Configure multiple experiment rows against the same uploaded documents, then execute them
        side by side with dense retrieval or the current hybrid stub and compare the generated answers.
      </p>

      <div className={styles.toolbar}>
        <div className={styles.toolbarCard}>
          <span className={styles.toolbarLabel}>Shared Source Set</span>
          <strong className={styles.toolbarValue}>{completedDocuments.length} indexed documents</strong>
          <p className={styles.toolbarHint}>All cases in this batch use the same uploaded document set.</p>
        </div>
        <div className={styles.toolbarActions}>
          <button type="button" className={styles.secondaryButton} onClick={addCase}>
            Add Comparison
          </button>
          <button type="button" className={styles.primaryButton} onClick={runBatch} disabled={executeMutation.isPending}>
            {executeMutation.isPending ? 'Running...' : 'Run Batch'}
          </button>
        </div>
      </div>

      <div className={styles.queryPanel}>
        <label className={styles.label}>Evaluation Question</label>
        <textarea
          className={styles.textarea}
          value={queryText}
          onChange={(event) => setQueryText(event.target.value)}
          placeholder="Ask a grounded question that every case should answer from the uploaded documents."
          rows={4}
        />
        <p className={styles.optionHint}>
          This shared query is executed against every configured case so the outputs remain directly comparable.
        </p>
      </div>

      {executeMutation.isError ? (
        <div className={`${styles.builderState} ${styles.builderStateError}`}>
          {(executeMutation.error as Error).message}
        </div>
      ) : null}

      {optionsQuery.isLoading ? (
        <div className={styles.builderState}>Loading experiment builder options...</div>
      ) : optionsQuery.isError ? (
        <div className={`${styles.builderState} ${styles.builderStateError}`}>
          {(optionsQuery.error as Error).message}
        </div>
      ) : builderOptions ? (
        <div className={styles.caseList}>
          {cases.map((item) => (
            <ExperimentCaseCard
              key={item.id}
              caseLabel={item.caseLabel}
              canRemove={cases.length > 1}
              initialValues={{
                parser: item.parser,
                chunkingStrategy: item.chunkingStrategy,
                retrievalTechnique: item.retrievalTechnique,
                embeddingModel: item.embeddingModel,
                reranker: item.reranker,
                promptTemplate: item.promptTemplate,
                evaluationFramework: item.evaluationFramework,
                databaseTarget: item.databaseTarget,
                databaseName: item.databaseName,
              }}
              options={builderOptions}
              onRemove={() => removeCase(item.id)}
              onPatch={(patch) => updateCase(item.id, patch)}
            />
          ))}
        </div>
      ) : (
        <div className={styles.builderState}>No builder options were returned by the API.</div>
      )}

      {lastExecutionResult ? (
        <div className={styles.executionResults}>
          <div className={styles.payloadHeader}>
            <div>
              <p className={styles.caseEyebrow}>Execution Results</p>
              <h3>Batch {lastExecutionResult.batchId}</h3>
            </div>
            <span className={styles.payloadTag}>{lastExecutionResult.caseCount} cases</span>
          </div>

          <div className={styles.resultsGrid}>
            {lastExecutionResult.results.map((result) => (
              <article key={result.caseLabel} className={styles.resultCard}>
                <div className={styles.resultHeader}>
                  <div>
                    <p className={styles.caseEyebrow}>{result.caseLabel}</p>
                    <h4>{result.retrievalTechnique}</h4>
                  </div>
                  <span className={styles.resultMetric}>{result.totalLatencyMs} ms</span>
                </div>

                <p className={styles.resultAnswer}>{result.answer}</p>

                <div className={styles.resultStats}>
                  <span>Model: {result.model}</span>
                  <span>Retrieval: {result.retrievalLatencyMs} ms</span>
                  <span>Generation: {result.generationLatencyMs} ms</span>
                </div>

                {result.warnings.length > 0 ? (
                  <div className={styles.warningList}>
                    {result.warnings.map((warning) => (
                      <span key={warning} className={styles.warningTag}>{warning}</span>
                    ))}
                  </div>
                ) : null}

                <div className={styles.chunkList}>
                  {result.retrievedChunks.map((chunk) => (
                    <div key={chunk.chunkId} className={styles.chunkCard}>
                      <div className={styles.chunkMeta}>
                        <span>{chunk.documentFileName}</span>
                        <span>Chunk {chunk.chunkIndex}</span>
                        <span>{chunk.retrievalSource}</span>
                        <span>{chunk.score.toFixed(3)}</span>
                      </div>
                      <p>{chunk.contentPreview}</p>
                    </div>
                  ))}
                </div>
              </article>
            ))}
          </div>
        </div>
      ) : null}
    </section>
  )
}
