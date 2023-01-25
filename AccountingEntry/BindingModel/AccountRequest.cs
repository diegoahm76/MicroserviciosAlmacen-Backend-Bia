
namespace AccountingEntry.API.BindingModel
{
	public class AccountRequest
	{
		public string CodCta { get; set; } //REQUERIDO! Debe tomarse de la lista
		//public int ConsecCta { get; set; } //REQUERDO Solo para el Update
		public string Detalle { get; set; } //REQUERIDO! Debe tomarse de la lista
		public decimal ValorBase { get; set; } //REQUERIDO! Debe tomarse de la lista
		public decimal ValorDebito { get; set; } //REQUERIDO! Debe tomarse de la lista
		public decimal ValorCredito { get; set; } //REQUERIDO! Debe tomarse de la lista
	}
}
