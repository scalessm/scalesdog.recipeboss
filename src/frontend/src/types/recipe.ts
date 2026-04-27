export interface Ingredient {
  quantity: string;
  item: string;
}

export interface RecipeRating {
  rating: number;
  ratedAt: string;
  notes?: string;
}

export interface RecipeSummary {
  id: string;
  title: string;
  description?: string;
  servings?: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  tags: string[];
  imageUrls: string[];
  rating?: RecipeRating;
  importedAt: string;
}

export interface Recipe extends RecipeSummary {
  sourceUrl: string;
  ingredients: Ingredient[];
  instructions: string[];
  updatedAt: string;
}
