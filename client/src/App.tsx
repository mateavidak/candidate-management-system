import { Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from './layout/AppLayout'
import { CandidateDetailPage } from './pages/CandidateDetailPage'
import { CandidateNewPage } from './pages/CandidateNewPage'
import { CandidatesListPage } from './pages/CandidatesListPage'
import { SkillsPage } from './pages/SkillsPage'
import './App.css'

export default function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<Navigate to="/candidates" replace />} />
        <Route path="/candidates" element={<CandidatesListPage />} />
        <Route path="/candidates/new" element={<CandidateNewPage />} />
        <Route path="/candidates/:id" element={<CandidateDetailPage />} />
        <Route path="/skills" element={<SkillsPage />} />
      </Route>
    </Routes>
  )
}
