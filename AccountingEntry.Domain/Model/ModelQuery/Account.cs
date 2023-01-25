
namespace AccountingEntry.Domain.Model.ModelQuery
{
	public class Account
	{
		public string CodCta { get; set; }
		public int ConsecCta { get; set; }
		public string Referencia { get; set; }
		public string Detalle { get; set; }
		public string CodTipoDocCruce { get; set; }
		public string NumeroDocCruce { get; set; }
		public decimal ValorBase { get; set; }
		public decimal ValorDebito { get; set; }
		public decimal ValorCredito { get; set; }
		public bool requiresCostCenter { get; set; }
		public bool requiresPerson { get; set; }
	}
}
