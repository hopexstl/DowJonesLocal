using System.Data.Entity;

namespace DJLocal.Models
{
    public partial class DowJonesContext : DbContext
    {
        public DowJonesContext()
            : base("name=dowjones_TrademonkeyEntities")
        {
        }

        public virtual DbSet<dowjones_Search_Result_Details> dowjones_Search_Result_Details { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
