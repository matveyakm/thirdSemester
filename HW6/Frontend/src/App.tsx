import { useState, useEffect } from 'react';
import { Container, Typography, Divider } from '@mui/material';
import UploadSection from './components/UploadSection';
import HistoryTable from './components/HistoryTable';
import RunDetailModal from './components/RunDetailModal';
import axios from 'axios';
import type { RunSummaryDto } from './types/api';

function App() {
  const [selectedRunId, setSelectedRunId] = useState<string | null>(null);
  const [history, setHistory] = useState<RunSummaryDto[]>([]);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  useEffect(() => {
    axios
      .get<RunSummaryDto[]>('http://localhost:5231/api/runs')
      .then((res) => setHistory(res.data))
      .catch((err) => console.error('Не удалось загрузить историю', err));
  }, [refreshTrigger]);

  const handleRunCompleted = () => {
    setRefreshTrigger((prev) => prev + 1);
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h3" component="h1" gutterBottom align="center" sx={{ mb: 4 }}>
        MyNUnit Web Runner
      </Typography>

      <UploadSection onRunCompleted={handleRunCompleted} />

      <Divider sx={{ my: 5 }} />

      <HistoryTable
        runs={history}
        onSelectRun={(runId) => setSelectedRunId(runId)}
      />

      <RunDetailModal
        runId={selectedRunId}
        onClose={() => setSelectedRunId(null)}
      />
    </Container>
  );
}

export default App;
