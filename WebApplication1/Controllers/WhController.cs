using Core.Contracts;
using Core.Grains;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WhController(IClusterClient clusterClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        IWarehouseGrain grain = clusterClient.GetGrain<IWarehouseGrain>(id);
        return Ok(await grain.GetWarehouseAsync(new GetWarehouse(id)));
    }

    [HttpPost]
    public async Task<IActionResult> Post(int id, string location, string owner, long capacity)
    {
        IWarehouseGrain grain = clusterClient.GetGrain<IWarehouseGrain>(id);
        await grain.CreateWarehouseAsync(new CreateWarehouse(location, owner, capacity));
        return NoContent();
    }
}