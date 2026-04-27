import { useState, useEffect, useCallback, useRef } from "react";
import { Link } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { Search, RefreshCw } from "lucide-react";
import RecipeCard from "@/components/RecipeCard";
import { getRecipes, getRecipeTags } from "@/api/recipes";
import { cn } from "@/lib/utils";
import type { RecipeSummary } from "@/types/recipe";

const API_SCOPES = ["api://recipeboss/Recipes.ReadWrite"];

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

export default function RecipeLibraryPage() {
  const { instance, accounts } = useMsal();

  const [recipes, setRecipes] = useState<RecipeSummary[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [searchInput, setSearchInput] = useState("");
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const getToken = useCallback(async () => {
    const result = await instance.acquireTokenSilent({
      scopes: API_SCOPES,
      account: accounts[0],
    });
    return result.accessToken;
  }, [instance, accounts]);

  // Debounce the search query
  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => setQuery(searchInput), 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [searchInput]);

  // Fetch available tags once on mount
  useEffect(() => {
    getToken()
      .then((token) => getRecipeTags(token))
      .then(setTags)
      .catch(() => {
        // Tags are optional UI enhancement; silently fail
      });
  }, [getToken]);

  const fetchRecipes = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const token = await getToken();
      const data = await getRecipes(token, {
        q: query || undefined,
        tags: selectedTags.length > 0 ? selectedTags : undefined,
      });
      setRecipes(data);
    } catch {
      setError("Failed to load recipes. Please try again.");
    } finally {
      setLoading(false);
    }
  }, [getToken, query, selectedTags]);

  useEffect(() => {
    fetchRecipes();
  }, [fetchRecipes]);

  const toggleTag = (tag: string) => {
    setSelectedTags((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag]
    );
  };

  return (
    <div className="min-h-screen bg-[#0F0F1A] text-[#F8F8FC] p-6 lg:p-8">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-2xl font-bold mb-6">Recipe Library</h1>

        {/* Search + tag filter bar */}
        <div className="flex flex-col gap-4 mb-8">
          <div className="relative">
            <Search
              className="absolute left-3 top-1/2 -translate-y-1/2 text-[#A0A0C0]"
              size={16}
            />
            <input
              type="text"
              placeholder="Search recipes..."
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 bg-[#1E1E2E] border border-white/10 rounded-lg text-[#F8F8FC] placeholder:text-[#A0A0C0] focus:outline-none focus:border-[#7C3AED]/60 transition-colors"
            />
          </div>

          {tags.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {tags.map((tag) => {
                const active = selectedTags.includes(tag);
                return (
                  <button
                    key={tag}
                    onClick={() => toggleTag(tag)}
                    className={cn(
                      "px-3 py-1 rounded-full text-sm font-medium border transition-all",
                      active
                        ? "bg-[#7C3AED] border-[#7C3AED] text-white"
                        : "bg-transparent border-white/15 text-[#A0A0C0] hover:border-[#7C3AED]/50 hover:text-[#F8F8FC]"
                    )}
                  >
                    {tag}
                  </button>
                );
              })}
            </div>
          )}
        </div>

        {/* Loading — skeleton grid */}
        {loading && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {Array.from({ length: 6 }).map((_, i) => (
              <SkeletonCard key={i} />
            ))}
          </div>
        )}

        {/* Error state */}
        {!loading && error && (
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <p className="text-[#A0A0C0]">{error}</p>
            <button
              onClick={fetchRecipes}
              className="flex items-center gap-2 px-4 py-2 bg-[#7C3AED] hover:bg-[#6D28D9] text-white rounded-lg transition-colors"
            >
              <RefreshCw size={14} />
              Try again
            </button>
          </div>
        )}

        {/* Empty state */}
        {!loading && !error && recipes.length === 0 && (
          <div className="flex flex-col items-center gap-4 py-16 text-center">
            <span className="text-6xl">🍽️</span>
            <p className="text-[#A0A0C0] text-lg">No recipes yet.</p>
            <Link
              to="/import"
              className="px-5 py-2.5 bg-[#7C3AED] hover:bg-[#6D28D9] text-white font-medium rounded-lg transition-colors"
            >
              Import your first recipe
            </Link>
          </div>
        )}

        {/* Recipe grid */}
        {!loading && !error && recipes.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {recipes.map((recipe) => (
              <RecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
