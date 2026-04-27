import { useState, useEffect, useCallback } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { ArrowRight, BookOpen, Link2 } from "lucide-react";
import RecipeCard from "@/components/RecipeCard";
import { getRecipes, getRecipeTags } from "@/api/recipes";
import type { RecipeSummary } from "@/types/recipe";
import { apiScopes } from "@/auth/msalConfig";

function SkeletonCard() {
  return (
    <div className="rounded-xl bg-[#1E1E2E] overflow-hidden border border-white/5 animate-pulse">
      <div className="aspect-video bg-[#2A2A3E]" />
      <div className="p-4 flex flex-col gap-3">
        <div className="h-4 bg-[#2A2A3E] rounded w-3/4" />
        <div className="h-3 bg-[#2A2A3E] rounded w-1/4" />
        <div className="flex gap-2">
          <div className="h-5 bg-[#2A2A3E] rounded-full w-12" />
          <div className="h-5 bg-[#2A2A3E] rounded-full w-16" />
        </div>
      </div>
    </div>
  );
}

export default function HomePage() {
  const { instance, accounts } = useMsal();
  const navigate = useNavigate();
  const isAuthenticated = accounts.length > 0;

  const [urlInput, setUrlInput] = useState("");
  const [recipes, setRecipes] = useState<RecipeSummary[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const [recipesLoading, setRecipesLoading] = useState(false);
  const [recipesError, setRecipesError] = useState<string | null>(null);

  const getToken = useCallback(async () => {
    const result = await instance.acquireTokenSilent({
      scopes: apiScopes,
      account: accounts[0],
    });
    return result.accessToken;
  }, [instance, accounts]);

  useEffect(() => {
    if (!isAuthenticated) return;
    getToken()
      .then((token) => getRecipeTags(token))
      .then(setTags)
      .catch(() => {});
  }, [isAuthenticated, getToken]);

  useEffect(() => {
    if (!isAuthenticated) return;
    setRecipesLoading(true);
    setRecipesError(null);
    getToken()
      .then((token) => getRecipes(token, { pageSize: 6 }))
      .then(setRecipes)
      .catch(() => setRecipesError("Failed to load recent recipes."))
      .finally(() => setRecipesLoading(false));
  }, [isAuthenticated, getToken]);

  const handleImport = () => {
    const trimmed = urlInput.trim();
    if (!trimmed) return;
    navigate(`/import?url=${encodeURIComponent(trimmed)}`);
  };

  return (
    <div className="min-h-screen bg-[#0F0F1A] text-[#F8F8FC]">
      {/* Hero section */}
      <section className="px-6 pt-20 pb-16 lg:pt-28 lg:pb-20 flex flex-col items-center text-center">
        <h1
          className="text-4xl lg:text-6xl font-extrabold leading-tight mb-6 max-w-3xl"
          style={{
            background: "linear-gradient(135deg, #7C3AED, #2563EB)",
            WebkitBackgroundClip: "text",
            WebkitTextFillColor: "transparent",
            backgroundClip: "text",
          }}
        >
          Your personal recipe library, powered by AI
        </h1>

        {/* URL import bar */}
        <div className="flex w-full max-w-xl gap-2 mb-6">
          <div className="relative flex-1">
            <Link2
              className="absolute left-3 top-1/2 -translate-y-1/2 text-[#A0A0C0]"
              size={16}
            />
            <input
              type="url"
              placeholder="Paste a recipe URL to import…"
              value={urlInput}
              onChange={(e) => setUrlInput(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleImport()}
              className="w-full pl-10 pr-4 py-3 bg-[#1E1E2E] border border-white/10 rounded-lg text-[#F8F8FC] placeholder:text-[#A0A0C0] focus:outline-none focus:border-[#7C3AED]/60 transition-colors"
            />
          </div>
          <button
            onClick={handleImport}
            className="flex items-center gap-2 px-5 py-3 rounded-lg font-semibold text-white transition-all hover:opacity-90 active:scale-95"
            style={{ background: "linear-gradient(135deg, #7C3AED, #2563EB)" }}
          >
            Import Recipe
            <ArrowRight size={16} />
          </button>
        </div>

        <Link
          to="/recipes"
          className="flex items-center gap-2 px-5 py-2.5 rounded-lg border border-white/15 text-[#A0A0C0] hover:border-[#7C3AED]/50 hover:text-[#F8F8FC] transition-colors font-medium"
        >
          <BookOpen size={16} />
          Browse Library
        </Link>
      </section>

      <div className="max-w-7xl mx-auto px-6 pb-16 lg:px-8 flex flex-col gap-14">
        {/* Recent recipes */}
        <section>
          <h2 className="text-xl font-bold mb-6 text-[#F8F8FC]">Recent Recipes</h2>

          {!isAuthenticated && (
            <div className="flex items-center justify-center py-16 rounded-xl bg-[#1E1E2E] border border-white/5">
              <p className="text-[#A0A0C0]">Sign in to see your recent recipes</p>
            </div>
          )}

          {isAuthenticated && recipesLoading && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <SkeletonCard key={i} />
              ))}
            </div>
          )}

          {isAuthenticated && !recipesLoading && recipesError && (
            <p className="text-[#A0A0C0] text-sm">{recipesError}</p>
          )}

          {isAuthenticated && !recipesLoading && !recipesError && recipes.length === 0 && (
            <div className="flex flex-col items-center gap-4 py-16 text-center rounded-xl bg-[#1E1E2E] border border-white/5">
              <span className="text-5xl">🍽️</span>
              <p className="text-[#A0A0C0]">No recipes yet — import your first one!</p>
            </div>
          )}

          {isAuthenticated && !recipesLoading && !recipesError && recipes.length > 0 && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {recipes.map((recipe) => (
                <RecipeCard key={recipe.id} recipe={recipe} />
              ))}
            </div>
          )}
        </section>

        {/* Tag cloud */}
        {tags.length > 0 && (
          <section>
            <h2 className="text-xl font-bold mb-4 text-[#F8F8FC]">Browse by Tag</h2>
            <div className="flex flex-wrap gap-2">
              {tags.map((tag) => (
                <Link
                  key={tag}
                  to={`/recipes?tags=${encodeURIComponent(tag)}`}
                  className="px-3 py-1 rounded-full text-sm font-medium border border-white/15 text-[#A0A0C0] hover:border-[#7C3AED]/50 hover:text-[#F8F8FC] transition-all"
                >
                  {tag}
                </Link>
              ))}
            </div>
          </section>
        )}
      </div>
    </div>
  );
}
