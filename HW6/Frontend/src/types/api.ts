export interface TestResultDto {
  testName: string;
  status: string;
  executionTimeMs: number;
  message?: string;
  stackTrace?: string;
  ignoreReason?: string;
}

export interface TestClassResultDto {
  className: string;
  testResults: TestResultDto[];
}

export interface RunSummaryDto {
  runId: string;
  timestamp: string;
  assemblyCount: number;
  totalTests: number;
  passed: number;
  failed: number;
  errored: number;
  ignored: number;
}

export interface TestRunResultDto {
  summary: RunSummaryDto;
  classResults: TestClassResultDto[];
}

export interface UploadResponse {
  runId: string;
}

export interface ApiError {
  error: string;
}
