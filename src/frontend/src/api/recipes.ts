import axios from "axios";
import type { RecipeSummary } from "@/types/recipe";

const baseURL = import.meta.env.VITE_API_BASE_URL ?? "";

export async function getRecipes(
  accessToken: string,
  params: { q?: string; tags?: string[]; page?: number; pageSize?: number } = {}
): Promise<RecipeSummary[]> {
  const { q, tags, page, pageSize } = params;
  const response = await axios.get<RecipeSummary[]>(`${baseURL}/api/v1/recipes`, {
    headers: { Authorization: `Bearer ${accessToken}` },
    params: {
      ...(q ? { q } : {}),
      ...(tags && tags.length > 0 ? { tags: tags.join(",") } : {}),
      ...(page !== undefined ? { page } : {}),
      ...(pageSize !== undefined ? { pageSize } : {}),
    },
  });
  return response.data;
}

export async function getRecipeTags(accessToken: string): Promise<string[]> {
  const response = await axios.get<string[]>(`${baseURL}/api/v1/recipes/tags`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  return response.data;
}
