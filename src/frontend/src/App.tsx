import { Routes, Route, Link } from 'react-router-dom'
import { LoginButton } from './components/LoginButton'
import HomePage from './pages/HomePage'
import ImportPage from './pages/ImportPage'
import RecipeLibraryPage from './pages/RecipeLibraryPage'
import RecipeDetailPage from './pages/RecipeDetailPage'
import EditRecipePage from './pages/EditRecipePage'
import ProfilePage from './pages/ProfilePage'
import LoginPage from './pages/LoginPage'

export default function App() {
  return (
    <>
      <nav
        className="flex items-center justify-between px-6 py-3 border-b"
        style={{ backgroundColor: "var(--color-surface)", borderColor: "rgba(255,255,255,0.07)" }}
      >
        <Link
          to="/"
          className="text-lg font-semibold tracking-tight"
          style={{ color: "var(--color-text-primary)" }}
        >
          🍴 RecipeBoss
        </Link>
        <div className="flex items-center gap-4">
          <Link
            to="/recipes"
            className="text-sm transition-colors"
            style={{ color: "var(--color-text-secondary)" }}
          >
            Library
          </Link>
          <Link
            to="/import"
            className="text-sm transition-colors"
            style={{ color: "var(--color-text-secondary)" }}
          >
            Import
          </Link>
          <LoginButton />
        </div>
      </nav>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/import" element={<ImportPage />} />
        <Route path="/recipes" element={<RecipeLibraryPage />} />
        <Route path="/recipes/:id" element={<RecipeDetailPage />} />
        <Route path="/recipes/:id/edit" element={<EditRecipePage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/login" element={<LoginPage />} />
      </Routes>
    </>
  )
}
