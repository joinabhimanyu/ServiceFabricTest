using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NetCore.Stateful.Data;
using Newtonsoft.Json;

namespace NetCore.Stateful.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShoppingCartController : ControllerBase
	{
		private readonly IReliableStateManager _mStateManager = null;
		private readonly string ShoppingCart = "shoppingCart";

		public ShoppingCartController(IReliableStateManager stateManager)
		{
			this._mStateManager = stateManager;
		}
		private string getUserIdentity() {
			if (HttpContext.User!=null && HttpContext.User.Identity!=null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
			{
				return HttpContext.User.Identity.Name;
			}
			return "anonymous";
		}
		[HttpGet]
		public async Task<string> Get() {
			var myDictionary = await this._mStateManager.GetOrAddAsync<IReliableDictionary<string,ShoppingCart>>(ShoppingCart);
			using (var tx=_mStateManager.CreateTransaction())
			{
				var result = await myDictionary.TryGetValueAsync(tx, getUserIdentity());
				if (result.HasValue)
				{
					return JsonConvert.SerializeObject(new { Total = result.Value.Total, Items = result.Value.GetItems() }, 
						Formatting.None, 
						new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
				}
				else
				{
					return JsonConvert.SerializeObject(new ShoppingCart(),
						Formatting.None,
						new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
				}
			}
		}
		[HttpPost]
		public async void Post([FromBody]ShoppingCartItem item)
		{
			var myDictionary = await this._mStateManager.GetOrAddAsync<IReliableDictionary<string,ShoppingCart>>(ShoppingCart);
			using (var tx=_mStateManager.CreateTransaction())
			{
				await myDictionary.AddOrUpdateAsync(tx, getUserIdentity(), new ShoppingCart(item), (k, v) => v.AddItem(item));
				await tx.CommitAsync();
			}
		}
		[HttpDelete("{name}")]
		public async Task<IActionResult> Delete(string name) {
			var myDictionary = await _mStateManager.GetOrAddAsync<IReliableDictionary<string,ShoppingCart>>(ShoppingCart);
			using (var tx=_mStateManager.CreateTransaction())
			{
				var result = await myDictionary.TryGetValueAsync(tx, getUserIdentity());
				if (result.HasValue)
				{
					await myDictionary.SetAsync(tx, name, result.Value.RemoveItem(name));
					await tx.CommitAsync();
					return new OkResult();
				}
				else
					return new NotFoundResult();
			}
		}
	}
}
