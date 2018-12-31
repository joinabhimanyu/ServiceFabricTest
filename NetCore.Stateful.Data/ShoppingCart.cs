using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore.Stateful.Data
{
	public class ShoppingCart
	{
		private List<ShoppingCartItem> _mItems = null;
		public IEnumerable<ShoppingCartItem> GetItems()
		{
			return _mItems.AsEnumerable<ShoppingCartItem>();
		}
		public ShoppingCart()
		{
			this._mItems = new List<ShoppingCartItem>();
		}
		public ShoppingCart(ShoppingCartItem item) : this()
		{
			this.AddItem(item);
		}
		public double Total
		{
			get
			{
				return _mItems.Sum(x => x.LineTotal);
			}
		}
		public ShoppingCart AddItem(ShoppingCartItem item)
		{
			var eitem = _mItems.FirstOrDefault(x => x.ProductName == item.ProductName);
			if (eitem!=null)
			{
				eitem.Quantity += item.Quantity;
			}
			else
			{
				_mItems.Add(item);
			}
			return this;
		}
		public ShoppingCart RemoveItem(string productName)
		{
			var eitem = _mItems.FirstOrDefault(x => x.ProductName == productName);
			if (eitem!=null)
			{
				_mItems.Remove(eitem);
			}
			return this;
		}
	}
}
