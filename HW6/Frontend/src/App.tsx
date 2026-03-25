import { useState, useEffect } from 'react';
import { Container, Typography, Divider } from '@mui/material';
import UploadSection from './components/UploadSection';
import HistoryTable from './components/HistoryTable';
import RunDetailModal from './components/RunDetailModal';
import axios from 'axios';

function App() {
  const [selectedRunId, setSelectedRunId] = useState<string | null>(null);
  const [history, setHistory] = useState<any[]>([]);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // Загружаем историю при монтировании и после каждого нового прогона
  useEffect(() => {
    axios
      .get('http://localhost:5231/api/runs') // или '/api/runs' если proxy настроен
      .then((res) => setHistory(res.data))
      .catch((err) => console.error('Не удалось загрузить историю', err));
  }, [refreshTrigger]);

  const handleRunCompleted = () => {
    setRefreshTrigger((prev) => prev + 1); // триггер перезагрузки истории
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h3" component="h1" gutterBottom align="center" sx={{ mb: 4 }}>
        MyNUnit Web Runner
      </Typography>

      {/* Секция загрузки и запуска */}
      <UploadSection onRunCompleted={handleRunCompleted} />

      <Divider sx={{ my: 5 }} />

      {/* Таблица истории */}
      <HistoryTable
        runs={history}
        onSelectRun={(runId) => setSelectedRunId(runId)}
      />

      {/* Модальное окно с деталями выбранного прогона */}
      <RunDetailModal
        runId={selectedRunId}
        onClose={() => setSelectedRunId(null)}
      />
    </Container>
  );
}

export default App;