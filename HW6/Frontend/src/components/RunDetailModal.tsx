// Frontend/src/components/RunDetailModal.tsx
import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import axios from 'axios';

interface TestResult {
  testName: string;
  status: string;
  executionTimeMs: number;
  message?: string;
  stackTrace?: string;
  ignoreReason?: string;
}

interface ClassResult {
  className: string;
  testResults: TestResult[];
}

interface RunDetailModalProps {
  runId: string | null;
  onClose: () => void;
}

export default function RunDetailModal({ runId, onClose }: RunDetailModalProps) {
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!runId) return;

    setLoading(true);
    axios
      .get(`/api/runs/${runId}`)
      .then((res) => setData(res.data))
      .catch((err) => console.error(err))
      .finally(() => setLoading(false));
  }, [runId]);

  if (!runId || !data) return null;

  return (
    <Dialog open={true} onClose={onClose} maxWidth="xl" fullWidth>
      <DialogTitle>
        Результаты прогона — {runId.slice(0, 8)}...
      </DialogTitle>

      <DialogContent dividers>
        {loading ? (
          <Typography>Загрузка...</Typography>
        ) : (
          <>
            {/* Общая статистика */}
            <Box sx={{ mb: 4, display: 'flex', gap: 3, flexWrap: 'wrap' }}>
              <Chip label={`Total: ${data.summary.totalTests}`} />
              <Chip label={`Passed: ${data.summary.passed}`} color="success" />
              <Chip label={`Failed: ${data.summary.failed}`} color="error" />
              <Chip label={`Errored: ${data.summary.errored}`} color="warning" />
              <Chip label={`Ignored: ${data.summary.ignored}`} color="info" />
            </Box>

            {/* Список классов */}
            {data.classResults.map((cls: ClassResult, index: number) => (
              <Accordion key={index} defaultExpanded={index === 0}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="h6">{cls.className}</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <TableContainer component={Paper} variant="outlined">
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Тест</TableCell>
                          <TableCell align="center">Статус</TableCell>
                          <TableCell align="right">Время (мс)</TableCell>
                          <TableCell>Причина</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {cls.testResults.map((test, i) => (
                          <TableRow key={i}>
                            <TableCell>{test.testName}</TableCell>
                            <TableCell align="center">
                              {test.status === 'Passed' && <Chip label="Passed" color="success" size="small" />}
                              {test.status === 'Failed' && <Chip label="Failed" color="error" size="small" />}
                              {test.status === 'Errored' && <Chip label="Errored" color="warning" size="small" />}
                              {test.status === 'Ignored' && <Chip label="Ignored" color="info" size="small" />}
                            </TableCell>
                            <TableCell align="right">
                              {test.executionTimeMs.toFixed(0)}
                            </TableCell>
                            <TableCell>
                              {test.ignoreReason && <span>Ignored: {test.ignoreReason}</span>}
                              {test.message && (
                                <Box component="span" sx={{ display: 'block', mt: 0.5 }}>
                                  <strong>Message:</strong> {test.message}
                                  {test.stackTrace && (
                                    <Box component="pre" sx={{ fontSize: '0.75rem', mt: 0.5, whiteSpace: 'pre-wrap', wordBreak: 'break-all', bgcolor: '#f5f5f5', p: 0.5, borderRadius: 1 }}>
                                      {test.stackTrace}
                                    </Box>
                                  )}
                                </Box>
                              )}
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </AccordionDetails>
              </Accordion>
            ))}
          </>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} variant="contained">
          Закрыть
        </Button>
      </DialogActions>
    </Dialog>
  );
}