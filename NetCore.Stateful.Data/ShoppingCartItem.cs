using System;

namespace NetCore.Stateful.Data
{
	public class ShoppingCartItem
	{
		public string ProductName { get; set; }
		public double UnitPrice { get; set; }
		public int Quantity { get; set; }
		public double LineTotal
		{
			get
			{
				return this.Quantity * this.UnitPrice;
			}
		}
	}
}
