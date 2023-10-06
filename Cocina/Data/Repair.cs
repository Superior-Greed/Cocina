using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocina.Data
{
    [Table("Repairs")]
    public class Repair
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("passed")]
        public  string Passed { get; set; }
        [Column("decription")]
        public  string Description { get; set; }
        [ForeignKey(typeof(Recipe))]
        [Column("recipeId")]
        public int RecipeId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public Recipe Recipe { get; set; }
    }
}
