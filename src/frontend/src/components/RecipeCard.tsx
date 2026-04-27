import { Link } from "react-router-dom";
import { Star, Clock, Users } from "lucide-react";
import type { RecipeSummary } from "@/types/recipe";
import { cn } from "@/lib/utils";

interface RecipeCardProps {
  recipe: RecipeSummary;
}

function StarRating({ rating }: { rating: number }) {
  return (
    <div className="flex gap-0.5">
      {Array.from({ length: 5 }).map((_, i) => (
        <Star
          key={i}
          size={14}
          className={cn(
            i < Math.round(rating)
              ? "fill-amber-400 text-amber-400"
              : "fill-none text-[#A0A0C0]"
          )}
        />
      ))}
    </div>
  );
}

export default function RecipeCard({ recipe }: RecipeCardProps) {
  const totalMinutes = (recipe.prepTimeMinutes ?? 0) + (recipe.cookTimeMinutes ?? 0);
  const imageUrl = recipe.imageUrls?.[0];

  return (
    <Link
      to={`/recipes/${recipe.id}`}
      className="group block rounded-xl bg-[#1E1E2E] overflow-hidden border border-white/5 hover:border-[#7C3AED]/50 transition-all duration-200 hover:shadow-lg hover:shadow-[#7C3AED]/10"
    >
      {/* Image area */}
      <div className="relative aspect-video bg-[#0F0F1A] overflow-hidden">
        {imageUrl ? (
          <img
            src={imageUrl}
            alt={recipe.title}
            className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <span className="text-4xl opacity-20">🍽️</span>
          </div>
        )}
        {/* Purple/blue gradient overlay on hover */}
        <div className="absolute inset-0 bg-gradient-to-br from-[#7C3AED]/40 to-[#2563EB]/40 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
      </div>

      {/* Card content */}
      <div className="p-4 flex flex-col gap-2">
        <h3 className="text-[#F8F8FC] font-semibold text-base leading-snug line-clamp-2">
          {recipe.title}
        </h3>

        {recipe.rating && <StarRating rating={recipe.rating.rating} />}

        <div className="flex items-center gap-3 text-[#A0A0C0] text-xs">
          {totalMinutes > 0 && (
            <span className="flex items-center gap-1">
              <Clock size={12} />
              {totalMinutes} min
            </span>
          )}
          {recipe.servings && (
            <span className="flex items-center gap-1">
              <Users size={12} />
              {recipe.servings} servings
            </span>
          )}
        </div>

        {recipe.tags.length > 0 && (
          <div className="flex flex-wrap gap-1.5 mt-1">
            {recipe.tags.slice(0, 4).map((tag) => (
              <span
                key={tag}
                className="px-2 py-0.5 rounded-full text-xs font-medium bg-[#7C3AED]/20 text-[#A78BFA] border border-[#7C3AED]/30"
              >
                {tag}
              </span>
            ))}
          </div>
        )}
      </div>
    </Link>
  );
}
