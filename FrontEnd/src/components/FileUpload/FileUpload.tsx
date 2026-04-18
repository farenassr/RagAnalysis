import { useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { uploadDocument } from '../../api/documentsApi'
import { useUploadStore } from '../../store/uploadStore'
import type { ChunkingStrategy } from '../../types/documents'
import styles from './FileUpload.module.css'

const schema = z.object({
  parser: z.literal('LlamaParse'),
  chunkingStrategy: z.enum(['Semantic', 'RecursiveCharacter', 'DocumentAware', 'StructureAware'] as const),
  embeddingModel: z.literal('TextEmbedding3Small'),
})

type FormValues = z.infer<typeof schema>

const CHUNKING_OPTIONS: { value: ChunkingStrategy; label: string }[] = [
  { value: 'Semantic', label: 'Semantic Chunking' },
  { value: 'RecursiveCharacter', label: 'Recursive Character' },
  { value: 'DocumentAware', label: 'Document-Aware' },
  { value: 'StructureAware', label: 'Structure-Aware' },
]

export function FileUpload() {
  const inputRef = useRef<HTMLInputElement>(null)
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [isDragActive, setIsDragActive] = useState(false)
  const [fileError, setFileError] = useState<string | null>(null)

  const queryClient = useQueryClient()
  const { setLastUploadResult, addUploadedFileName } = useUploadStore()

  const { register, handleSubmit } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      parser: 'LlamaParse',
      chunkingStrategy: 'Semantic',
      embeddingModel: 'TextEmbedding3Small',
    },
  })

  const mutation = useMutation({
    mutationFn: uploadDocument,
    onSuccess: (data) => {
      setLastUploadResult(data)
      addUploadedFileName(data.fileName)
      queryClient.invalidateQueries({ queryKey: ['documents'] })
    },
  })

  const handleFileSelect = (file: File) => {
    setFileError(null)
    const allowed = [
      'application/pdf',
      'text/plain',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    ]

    if (!allowed.includes(file.type)) {
      setFileError('Unsupported file type. Please upload a PDF, TXT, or Word document.')
      return
    }

    if (file.size > 50 * 1024 * 1024) {
      setFileError('File exceeds the 50 MB limit.')
      return
    }

    setSelectedFile(file)
  }

  const onDrop = (event: React.DragEvent) => {
    event.preventDefault()
    setIsDragActive(false)
    const file = event.dataTransfer.files[0]

    if (file) {
      handleFileSelect(file)
    }
  }

  const onSubmit = (values: FormValues) => {
    if (!selectedFile) {
      setFileError('Please select a file before processing.')
      return
    }

    mutation.mutate({ file: selectedFile, ...values })
  }

  const formatBytes = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`
    return `${(bytes / 1048576).toFixed(1)} MB`
  }

  const result = mutation.data

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div
        className={[
          styles.dropzone,
          isDragActive ? styles.dropzoneActive : '',
          fileError ? styles.dropzoneError : '',
        ].join(' ')}
        onClick={() => inputRef.current?.click()}
        onDragOver={(event) => {
          event.preventDefault()
          setIsDragActive(true)
        }}
        onDragLeave={() => setIsDragActive(false)}
        onDrop={onDrop}
        role="button"
        tabIndex={0}
        onKeyDown={(event) => event.key === 'Enter' && inputRef.current?.click()}
      >
        <div className={styles.dropzoneIcon}>FILE</div>
        {selectedFile ? (
          <p className={styles.dropzoneLabel}>
            <strong>{selectedFile.name}</strong>
            <span className={styles.fileInfo}>{formatBytes(selectedFile.size)}</span>
          </p>
        ) : (
          <p className={styles.dropzoneLabel}>
            <span>Browse</span> or drag and drop your document
          </p>
        )}
        <input
          ref={inputRef}
          className={styles.hiddenInput}
          type="file"
          accept=".pdf,.txt,.doc,.docx"
          onChange={(event) => {
            const file = event.target.files?.[0]
            if (file) {
              handleFileSelect(file)
            }
          }}
        />
      </div>

      {fileError && <p className={styles.errorText}>{fileError}</p>}

      <div className={styles.configGrid}>
        <div className={styles.field}>
          <label className={styles.label}>Parser</label>
          <input className={styles.readonlyInput} value="LlamaParse" readOnly />
        </div>

        <div className={styles.field}>
          <label className={styles.label}>Chunking Strategy</label>
          <select className={styles.select} {...register('chunkingStrategy')}>
            {CHUNKING_OPTIONS.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.field}>
          <label className={styles.label}>Embedding Model</label>
          <input className={styles.readonlyInput} value="text-embedding-3-small" readOnly />
        </div>
      </div>

      <button
        type="submit"
        className={styles.submitButton}
        disabled={mutation.isPending || !selectedFile}
      >
        {mutation.isPending ? (
          <>
            <span className={styles.spinner} />
            Processing...
          </>
        ) : (
          'Parse, Chunk & Embed'
        )}
      </button>

      {mutation.isError && (
        <p className={styles.errorText}>Error: {(mutation.error as Error).message}</p>
      )}

      {result && (
        <div className={styles.resultCard}>
          <p className={styles.resultTitle}>Document processed successfully</p>
          <div className={styles.resultGrid}>
            <div className={styles.resultStat}>
              <span className={styles.resultStatLabel}>Chunks</span>
              <span className={styles.resultStatValue}>{result.chunkCount}</span>
            </div>
            <div className={styles.resultStat}>
              <span className={styles.resultStatLabel}>Status</span>
              <span className={styles.resultStatValue}>{result.status}</span>
            </div>
            <div className={styles.resultStat}>
              <span className={styles.resultStatLabel}>Time</span>
              <span className={styles.resultStatValue}>
                {result.processingTimeMs.toLocaleString()} ms
              </span>
            </div>
            <div className={styles.resultStat}>
              <span className={styles.resultStatLabel}>Parser</span>
              <span className={styles.resultStatValue}>{result.parser}</span>
            </div>
          </div>
        </div>
      )}
    </form>
  )
}
