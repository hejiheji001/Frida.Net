using System.Data.Entity;

namespace Sample
{
	public class SampleDbContext : DbContext
	{
		public DbSet<SaleInfo> SaleInfos { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}