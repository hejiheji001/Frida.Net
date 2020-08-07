using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample
{
	[Serializable]
	[Table("daily_sales", Schema = "moviepro")]
	public class SaleInfo
	{
		[Column("id")] public int Id { get; set; }

		[Column("date")] public string Date { get; set; }

		[Column("sales")] public double Sales { get; set; }

		[Column("city")] public string City { get; set; }

		[Column("type")] public string Type { get; set; }
	}
}