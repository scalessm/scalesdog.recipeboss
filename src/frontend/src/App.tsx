import { Routes, Route } from 'react-router-dom'
import HomePage from './pages/HomePage'
import ImportPage from './pages/ImportPage'
import RecipeLibraryPage from './pages/RecipeLibraryPage'
import RecipeDetailPage from './pages/RecipeDetailPage'
import EditRecipePage from './pages/EditRecipePage'
import ProfilePage from './pages/ProfilePage'
import LoginPage from './pages/LoginPage'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/import" element={<ImportPage />} />
      <Route path="/recipes" element={<RecipeLibraryPage />} />
      <Route path="/recipes/:id" element={<RecipeDetailPage />} />
      <Route path="/recipes/:id/edit" element={<EditRecipePage />} />
      <Route path="/profile" element={<ProfilePage />} />
      <Route path="/login" element={<LoginPage />} />
    </Routes>
  )
}
