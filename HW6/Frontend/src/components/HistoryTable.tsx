import { Typography, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';

interface Run {
  runId: string;
  timestamp: string;
  assemblyCount: number;
  totalTests: number;
  passed: number;
  failed: number;
  errored: number;
  ignored: number;
}

interface HistoryTableProps {
  runs: Run[];
  onSelectRun: (runId: string) => void;
}

export default function HistoryTable({ runs, onSelectRun }: HistoryTableProps) {
  if (runs.length === 0) {
    return (
      <Typography variant="body1" color="text.secondary" align="center" sx={{ py: 6 }}>
        Пока нет ни одного тестового прогона
      </Typography>
    );
  }

  return (
    <Paper elevation={3}>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Прогон</TableCell>
              <TableCell align="right">Дата / Время</TableCell>
              <TableCell align="right">Сборок</TableCell>
              <TableCell align="right">Всего тестов</TableCell>
              <TableCell align="right">Passed</TableCell>
              <TableCell align="right">Failed</TableCell>
              <TableCell align="right">Errored</TableCell>
              <TableCell align="right">Ignored</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {runs.map((run) => (
              <TableRow
                key={run.runId}
                hover
                sx={{ cursor: 'pointer' }}
                onClick={() => onSelectRun(run.runId)}
              >
                <TableCell component="th" scope="row">
                  {run.runId.slice(0, 8)}...
                </TableCell>
                <TableCell align="right">
                  {new Date(run.timestamp).toLocaleString('ru-RU')}
                </TableCell>
                <TableCell align="right">{run.assemblyCount}</TableCell>
                <TableCell align="right">{run.totalTests}</TableCell>
                <TableCell align="right" sx={{ color: 'success.main' }}>
                  {run.passed}
                </TableCell>
                <TableCell align="right" sx={{ color: 'error.main' }}>
                  {run.failed}
                </TableCell>
                <TableCell align="right" sx={{ color: 'warning.main' }}>
                  {run.errored}
                </TableCell>
                <TableCell align="right" sx={{ color: 'info.main' }}>
                  {run.ignored}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}