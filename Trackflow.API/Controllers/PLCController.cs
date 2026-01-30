using Microsoft.AspNetCore.Mvc;
using Trackflow.Application.Services;
using Trackflow.Shared.DTOs;

namespace Trackflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PLCController : ControllerBase
{
    private readonly PLCService _plcService;

    public PLCController(PLCService plcService)
    {
        _plcService = plcService;
    }

    /// <summary>
    /// Üretim hattını başlat
    /// </summary>
    [HttpPost("{workOrderId:guid}/start")]
    public async Task<ActionResult<PLCStatus>> Start(Guid workOrderId)
    {
        try
        {
            var result = await _plcService.StartProductionAsync(workOrderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Üretim hattını durdur
    /// </summary>
    [HttpPost("{workOrderId:guid}/stop")]
    public async Task<ActionResult<PLCStatus>> Stop(Guid workOrderId)
    {
        try
        {
            var result = await _plcService.StopProductionAsync(workOrderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// PLC durumunu al
    /// </summary>
    [HttpGet("{workOrderId:guid}/status")]
    public async Task<ActionResult<PLCStatus>> GetStatus(Guid workOrderId)
    {
        var result = await _plcService.GetStatusAsync(workOrderId);
        return Ok(result);
    }

    /// <summary>
    /// Palet tamamlandı sinyali gönder
    /// </summary>
    [HttpPost("pallet/{palletId:guid}/complete")]
    public async Task<ActionResult<bool>> PalletComplete(Guid palletId)
    {
        var result = await _plcService.PalletCompleteSignalAsync(palletId);
        return Ok(result);
    }
}
