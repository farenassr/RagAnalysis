import { create } from 'zustand';
import type { UploadDocumentResponse } from '../types/documents';

interface UploadStore {
  lastUploadResult: UploadDocumentResponse | null;
  setLastUploadResult: (result: UploadDocumentResponse | null) => void;
  uploadedFileNames: string[];
  addUploadedFileName: (name: string) => void;
  clearUploadHistory: () => void;
}

export const useUploadStore = create<UploadStore>((set) => ({
  lastUploadResult: null,
  setLastUploadResult: (result) => set({ lastUploadResult: result }),
  uploadedFileNames: [],
  addUploadedFileName: (name) =>
    set((state) => ({
      uploadedFileNames: state.uploadedFileNames.includes(name)
        ? state.uploadedFileNames
        : [...state.uploadedFileNames, name],
    })),
  clearUploadHistory: () => set({ uploadedFileNames: [], lastUploadResult: null }),
}));
