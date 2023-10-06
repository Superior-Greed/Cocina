using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Data
{
    [Table("Recipes")]
    public class Recipe
    {
        [PrimaryKey,AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("time")]
        public string Time { get; set; }
        [Column("principal_ingredient")]
        public string PrincipalIngredient { get; set; }
        [Column("person")]
        public int Person { get; set; }
        [OneToMany(CascadeOperations =CascadeOperation.All)]
        public  List<Repair> Repairs { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Ingredient> Ingredients { get; set; }
    }
}
