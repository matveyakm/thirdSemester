import { useState } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Typography,
  Alert,
  Paper,
  List,
  ListItem,
  ListItemText,
  IconButton,
} from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import DeleteIcon from '@mui/icons-material/Delete';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import axios from 'axios';
import type { TestRunResultDto, UploadResponse, ApiError } from '../types/api';

interface UploadSectionProps {
  onRunCompleted: () => void;
}

export default function UploadSection({ onRunCompleted }: UploadSectionProps) {
  const [files, setFiles] = useState<File[]>([]);
  const [uploading, setUploading] = useState(false);
  const [runId, setRunId] = useState<string | null>(null);
  const [running, setRunning] = useState(false);
  const [result, setResult] = useState<TestRunResultDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setFiles((prev) => [...prev, ...Array.from(e.target.files!)]);
    }
  };

  const removeFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleUpload = async () => {
    if (files.length === 0) return;

    setUploading(true);
    setError(null);
    setRunId(null);
    setResult(null);

    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));

    try {
      const res = await axios.post<UploadResponse>('/api/runs/upload', formData);
      setRunId(res.data.runId);
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: ApiError } };
      setError(axiosError.response?.data?.error || 'Ошибка загрузки');
    } finally {
      setUploading(false);
    }
  };

  const handleRun = async () => {
    if (!runId) return;

    setRunning(true);
    setError(null);
    setResult(null);

    try {
      const res = await axios.post<TestRunResultDto>(`/api/runs/${runId}/execute`);
      setResult(res.data);
      onRunCompleted();
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: ApiError } };
      setError(axiosError.response?.data?.error || 'Ошибка выполнения тестов');
    } finally {
      setRunning(false);
    }
  };

  return (
    <Paper elevation={3} sx={{ p: 4, mb: 5 }}>
      <Typography variant="h5" gutterBottom>
        Загрузить сборки и запустить тесты
      </Typography>

      <Box
        sx={{
          border: '2px dashed #aaa',
          borderRadius: 3,
          p: 8,
          textAlign: 'center',
          mb: 4,
          bgcolor: 'grey.50',
          transition: 'all 0.2s',
          '&:hover': { bgcolor: 'grey.100', borderColor: 'primary.main' },
        }}
      >
        <input
          type="file"
          multiple
          accept=".dll"
          onChange={handleFileChange}
          style={{ display: 'none' }}
          id="dll-upload"
        />
        <label htmlFor="dll-upload">
          <Button variant="outlined" component="span" startIcon={<CloudUploadIcon />}>
            Выбрать .dll файлы
          </Button>
        </label>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          или перетащите файлы сюда
        </Typography>
      </Box>

      {files.length > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle1" gutterBottom>
            Выбрано файлов: {files.length}
          </Typography>
          <List dense sx={{ maxHeight: 200, overflowY: 'auto', bgcolor: 'grey.100', borderRadius: 1 }}>
            {files.map((file, idx) => (
              <ListItem
                key={idx}
                secondaryAction={
                  <IconButton edge="end" aria-label="delete" onClick={() => removeFile(idx)}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                }
              >
                <ListItemText primary={file.name} secondary={`${(file.size / 1024).toFixed(1)} KB`} />
              </ListItem>
            ))}
          </List>
        </Box>
      )}

      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        <Button
          variant="contained"
          color="primary"
          startIcon={uploading ? <CircularProgress size={20} /> : <CloudUploadIcon />}
          onClick={handleUpload}
          disabled={uploading || files.length === 0}
          fullWidth={files.length === 0}
        >
          {uploading ? 'Загружаем...' : 'Загрузить на сервер'}
        </Button>

        {runId && (
          <Button
            variant="contained"
            color="success"
            startIcon={running ? <CircularProgress size={20} /> : <PlayArrowIcon />}
            onClick={handleRun}
            disabled={running}
            fullWidth
          >
            {running ? 'Тестируем...' : 'Запустить тесты'}
          </Button>
        )}
      </Box>

      {error && <Alert severity="error" sx={{ mt: 3 }}>{error}</Alert>}

      {result && (
        <Box sx={{ mt: 5 }}>
          <Typography variant="h6" gutterBottom color="success.main">
            Тестирование завершено
          </Typography>
          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
            <Typography>Passed: <strong>{result.summary.passed}</strong></Typography>
            <Typography color="error">Failed: <strong>{result.summary.failed}</strong></Typography>
            <Typography color="warning.main">Errored: <strong>{result.summary.errored}</strong></Typography>
            <Typography color="info.main">Ignored: <strong>{result.summary.ignored}</strong></Typography>
          </Box>
        </Box>
      )}
    </Paper>
  );
}
