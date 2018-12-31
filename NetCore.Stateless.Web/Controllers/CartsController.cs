using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using NetCore.Stateful.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCore.Stateless.Web.Controllers
{
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class CartsController : ControllerBase
	{
		private static Regex ipRex =
			new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
		private HttpClient httpClient = null;
		public CartsController(HttpClient client)
		{
			this.httpClient = client;
		}
		[HttpGet]
		public async Task<object> Get()
		{
			HttpResponseMessage response = await this.httpClient.GetAsync(await ResolveAddress() + "/api/ShoppingCart");
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				return this.StatusCode((int)response.StatusCode);
			}
			var cart = JsonConvert.DeserializeObject<ShoppingCart>(await response.Content.ReadAsStringAsync());
			return JsonConvert.SerializeObject(cart,
						Formatting.None,
						new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		}
		[HttpPost]
		public async Task<IActionResult> Post([FromBody]ShoppingCartItem item)
		{
			StringContent postContent = new StringContent(JsonConvert
			.SerializeObject(item), Encoding.UTF8, "application/json");
			postContent.Headers.ContentType =
			new
			System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
			HttpResponseMessage response = await this.httpClient.PostAsync(
			await ResolveAddress() + "/api/ShoppingCart", postContent);
			return new OkResult();
		}
		[HttpDelete("{productName}")]
		public async Task<IActionResult> Delete(string productName)
		{
			HttpResponseMessage response = await this.httpClient.DeleteAsync(
			await ResolveAddress() + "/api/ShoppingCart/" + productName);
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
				return this.StatusCode((int)response.StatusCode);
			return new OkResult();
		}
		private async Task<string> ResolveAddress()
		{
			var partitionResolver = ServicePartitionResolver.GetDefault();
			var resolveResults = await partitionResolver.ResolveAsync(
				new Uri("fabric:/ServiceFabricTest/NetCore.Stateful.Web"),
				new ServicePartitionKey(1),
				new System.Threading.CancellationToken());
			var endpoint = resolveResults.GetEndpoint();
			var endpointObject =
			JsonConvert.DeserializeObject<JObject>(endpoint.Address);
			var addressString = ((JObject)endpointObject
			.Property("Endpoints").Value)[""].Value<string>();
			return addressString;
		}
	}
}