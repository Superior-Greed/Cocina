using Cocina.Data;
using Cocina.Data.Dto;
using Cocina.Interfas;
using Microsoft.Maui.Graphics;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Service
{
    public class RecipeService : RecipeInterface
    {
        SQLiteAsyncConnection _database;
        string _dbPath;
        public RecipeService(string dbPath)
        {
            _dbPath = dbPath;
        }
        private async Task Init()
        {
            if (_database != null)
            {
                return;
            }
            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<Recipe>();
        }
        public async Task<bool> AddRecipe(Recipe recipe)
        {
            await Init();
            await _database.InsertAsync(recipe);
            List < Ingredient > ingredients = recipe.Ingredients.Select(ingredient => new Ingredient
            {
                Id = 0,
                Name = ingredient.Name,
                RecipeId = recipe.Id
            }).ToList();
            await _database.InsertAllAsync(ingredients);
            List<Repair> repairs = recipe.Repairs.Select(repair => new Repair
            {
                Id = 0,
                Passed = repair.Passed,
                Description = repair.Description,
                RecipeId = recipe.Id
            }).ToList();
            await _database.InsertAllAsync(repairs);
            //await _database.InsertWithChildrenAsync(recipe, recursive: true);
            return true;
        }

        public async Task<Recipe> GetRecipe(int recipeId)
        {
            await Init();
            return await _database.Table<Recipe>().Where(recipe => recipe.Id == recipeId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RecipeDto>> GetRecipe()
        {
            await Init();
            //para en un futuro porner por rangos en caso de ser millones de registro asta 10000 funciona eficientemente
            //var recipes = await _database.QueryAsync<Recipe>("SELECT * from Recipes r  limit 100");

            var recipes = await _database.Table<Recipe>().ToListAsync();
            var ingredients = await _database.Table<Ingredient>().ToListAsync();
            var repairs = await _database.Table<Repair>().ToListAsync();
            return recipes.GroupJoin(repairs, recipe => recipe.Id, repair => repair.RecipeId,
            (recipe, repair) =>
            {
                recipe.Repairs = repair.ToList();
                return recipe;
            }).Where(parent => parent.Repairs.Any()).GroupJoin(ingredients, recipe => recipe.Id, ingredient => ingredient.RecipeId,
            (recipe, ingredient) =>
            {
                recipe.Ingredients = ingredient.ToList();
                return recipe;
            }
            ).Where(parent => parent.Ingredients.Any())
            .Select(recipe => new RecipeDto
            {
                Id = recipe.Id,
                Ingredients = recipe.Ingredients,
                Repairs = recipe.Repairs,
                Time = recipe.Time,
                Title = recipe.Title,
                PrincipalIngredient = recipe.PrincipalIngredient,
                Person = recipe.Person,
                VisibleRepairs = false,
                VisileIngredients = false,
            }).ToList();
        }
        public async Task<(IEnumerable<RecipeDto>, int)> GetRecipe(string[] search ,int page)
        {
            await Init();
            Stopwatch timeMeasure = new Stopwatch();
            timeMeasure.Start();
            string query = "";
            string recipeQuery = "select DISTINCT i.recipeId as id from Ingredients i where ";
            for (int i = 0; i < search.Length; i++)
            {
                if (i == (search.Length -1))
                {
                    query = @$"{query} i.ingredient like '%{search[i].Trim()}%'";
                }
                else
                {
                    query = @$"{query} i.ingredient like '%{search[i].Trim()}%' or";
                }
            }

            string completeQuery = @$"{recipeQuery}  {query} 
            GROUP BY i.recipeId 
            HAVING COUNT(DISTINCT i.id  ) >= {search.Length} ";

            //var recipes = await _database.QueryAsync<Recipe>(@$"select * from Recipes where id in ({recipeQuery} {query}  limit 20 offset {page * 20}) ");
            //var repairs = await _database.QueryAsync<Repair>(@$"select * from Repairs where recipeId in ({recipeQuery} {query}  limit 20 offset {page * 20})");
            //var ingredients = await _database.QueryAsync<Ingredient>(@$" select *  from Ingredients i2   WHERE  i2.recipeId  in ({recipeQuery} {query}  limit 20 offset {page * 20}) ");

            //var ingredients2 = await _database.QueryAsync<Ingredient>(@$" select *  from Ingredients i2   WHERE  i2.recipeId  in ({recipeQuery} {query})");
            //            var list_number2 = await _database.QueryAsync<Ingredient>($@"{recipeQuery}  {query} 
            //GROUP BY i.recipeId 
            //HAVING COUNT(DISTINCT i.id  ) >= {search.Length}");
            //            var list_number = list_number2.Select(x => x.RecipeId);

            //            var perfect = ingredients2.OrderBy(x => x.RecipeId).ToList();

            //            int id = 0;
            //            int count = 0;
            //            List<int > list = new List<int>();
            //            var distinc = list.Except(list_number);

            //            foreach (var item in perfect)
            //            {
            //                if (id != item.RecipeId || id == 0)
            //                {
            //                    id = item.RecipeId;
            //                    count = 0;
            //                }
            //                if (search.Where(x => item.Name.ToUpper().Contains(x)).Count() > 0)
            //                {
            //                    count++;
            //                }
            //                if (count == search.Length)
            //                {
            //                    list.Add(item.RecipeId);
            //                    count = 0;
            //                }
            //            }

            //var recipes = await _database.QueryAsync<Recipe>(@$"select * from Recipes where id in ({string.Join(",", list)})   ");
            //var repairs = await _database.QueryAsync<Repair>(@$"select * from Repairs where recipeId in (select id from Recipes where id in ({string.Join(",", list)})  limit 20 offset {page * 20}) ");
            //var ingredients = await _database.QueryAsync<Ingredient>(@$" select *  from Ingredients i2   WHERE  i2.recipeId  in (select id from Recipes where id in ({string.Join(",", list)})  limit 20 offset {page * 20}) ");

            string queryRecipes = $@"select id from Recipes where id in ({completeQuery} ) limit 20 offset {page * 20}";
            var recipes = await _database.QueryAsync<Recipe>(@$"select * from Recipes where id in ({completeQuery} ) limit 20 offset {page * 20} ");
            var repairs = await _database.QueryAsync<Repair>(@$"select * from Repairs where recipeId in ({queryRecipes}) ");
            var ingredients = await _database.QueryAsync<Ingredient>(@$" select *  from Ingredients i2   WHERE  i2.recipeId  in ({queryRecipes}) ");


            int cuantity = await _database.ExecuteScalarAsync<int>(@$"select count(*) from Recipes where id in ({completeQuery})");
            var recipeList = recipes.GroupJoin(repairs, recipe => recipe.Id, repair => repair.RecipeId,
            (recipe, repair) =>
            {
                recipe.Repairs = repair.ToList();
                return recipe;
            }).Where(parent => parent.Repairs.Any()).GroupJoin(ingredients, recipe => recipe.Id, ingredient => ingredient.RecipeId,
            (recipe, ingredient) =>
            {
                recipe.Ingredients = ingredient.ToList();
                return recipe;
            }
            ).Where(parent => parent.Ingredients.Any())
            .Select(recipe => new RecipeDto
            {
                Id = recipe.Id,
                Ingredients = recipe.Ingredients,
                Repairs = recipe.Repairs,
                Time = recipe.Time,
                Title = recipe.Title,
                PrincipalIngredient = recipe.PrincipalIngredient,
                Person = recipe.Person,
                VisibleRepairs = false,
                VisileIngredients = false,
            }).ToList();
            timeMeasure.Stop();
            Console.WriteLine($"Tiempo: {timeMeasure.Elapsed.TotalMilliseconds} ms");
            return (recipeList, cuantity);
        }

        public async Task<(IEnumerable<RecipeDto>, int)> GetRecipeList(int page)
        {
            await Init();
            string query = @$"select id from Recipes  limit 20 offset {page*20}";
            int count = await _database.ExecuteScalarAsync<int>("select count(*) from recipes");
            var recipes = await _database.QueryAsync<Recipe>(@$"select * from Recipes where id in ({query})");
            var repairs = await _database.QueryAsync<Repair>(@$"select * from Repairs where recipeId in ({query})");
            var ingredients = await _database.QueryAsync<Ingredient>(@$"select * from Ingredients  where  recipeId in ({query})");
            var recipeList = recipes.GroupJoin(repairs, recipe => recipe.Id, repair => repair.RecipeId,
            (recipe, repair) =>
            {
                recipe.Repairs = repair.ToList();
                return recipe;
            }).Where(parent => parent.Repairs.Any()).GroupJoin(ingredients, recipe => recipe.Id, ingredient => ingredient.RecipeId,
            (recipe, ingredient) =>
            {
                recipe.Ingredients = ingredient.ToList();
                return recipe;
            }
            ).Where(parent => parent.Ingredients.Any())
            .Select(recipe => new RecipeDto
            {
                Id = recipe.Id,
                Ingredients = recipe.Ingredients,
                Repairs = recipe.Repairs,
                Time = recipe.Time,
                Title = recipe.Title,
                PrincipalIngredient = recipe.PrincipalIngredient,
                Person = recipe.Person,
                VisibleRepairs = false,
                VisileIngredients = false,
            }).ToList();
            return (recipeList, count);
        }
        public async Task<bool> RemoveRecipe(int recipeId)
        {
            await Init();
            var recipe = await _database.Table<Recipe>().Where(recipe => recipe.Id == recipeId).FirstOrDefaultAsync();
            var ingredients = await _database.Table<Ingredient>().Where(ingredient => ingredient.RecipeId == recipeId).ToListAsync();
            var repairs = await _database.Table<Repair>().Where(repair => repair.RecipeId == recipeId).ToListAsync();
            if(recipe != null)
            {
                await _database.DeleteAllAsync(ingredients);
                await _database.DeleteAllAsync(repairs);
                await _database.DeleteAsync(recipe);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateRecipe(Recipe recipe)
        {
            await Init();
            await _database.UpdateAsync(recipe);
            return true;
        }


    }
}
