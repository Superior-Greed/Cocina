
using Cocina.Data;
using Cocina.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Interfas
{
    public interface RecipeInterface
    {
        Task<bool> AddRecipe(Recipe recipe);
        Task<bool> UpdateRecipe(Recipe recipe);
        Task<bool> RemoveRecipe(int recipeId);
        Task<Recipe> GetRecipe(int recipeId);
        Task<IEnumerable<RecipeDto>> GetRecipe();

        Task<(IEnumerable<RecipeDto>,int)> GetRecipe(string[] search,int pages);

        Task<(IEnumerable<RecipeDto>, int)> GetRecipeList(int page);
    }
}
