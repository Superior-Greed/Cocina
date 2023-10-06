using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Data
{
    [Table("Ingredients")]
    public class Ingredient
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [ForeignKey(typeof(Recipe))]
        [Column("recipeId")]
        public int RecipeId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public Recipe Recipe { get; set; }
        [Column("ingredient")]
        public string Name { get; set; }
    }
}
