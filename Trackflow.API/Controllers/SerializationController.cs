using Microsoft.AspNetCore.Mvc;
using Trackflow.Application.Services;
using Trackflow.Shared.DTOs;

namespace Trackflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SerializationController : ControllerBase
{
    private readonly PrinterService _printerService;
    private readonly CameraService _cameraService;
    private readonly PLCService _plcService;

    public SerializationController(
        PrinterService printerService,
        CameraService cameraService,
        PLCService plcService)
    {
        _printerService = printerService;
        _cameraService = cameraService;
        _plcService = plcService;
    }

    /// <summary>
    /// İş emrine ait tüm üretilmiş serileri yazdır
    /// </summary>
    [HttpPost("{workOrderId:guid}/print")]
    public async Task<ActionResult<PrintBatchResult>> PrintAll(Guid workOrderId)
    {
        try
        {
            var result = await _printerService.PrintBatchAsync(workOrderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// İş emrine ait belirli sayıda seriyi yazdır
    /// </summary>
    [HttpPost("{workOrderId:guid}/print/{count:int}")]
    public async Task<ActionResult<PrintBatchResult>> PrintCount(Guid workOrderId, int count)
    {
        try
        {
            var result = await _printerService.PrintBatchAsync(workOrderId, count);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// İş emrine ait tüm yazdırılmış serileri doğrula
    /// </summary>
    [HttpPost("{workOrderId:guid}/verify")]
    public async Task<ActionResult<VerifyBatchResult>> VerifyAll(Guid workOrderId)
    {
        try
        {
            var result = await _cameraService.VerifyBatchAsync(workOrderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Tek seri numarasını doğrula
    /// </summary>
    [HttpPost("{serialId:guid}/verify-single")]
    public async Task<ActionResult<VerifyResult>> VerifySingle(Guid serialId)
    {
        var result = await _cameraService.VerifyAsync(serialId);
        return Ok(result);
    }

    /// <summary>
    /// Reddedilmiş seriyi tekrar doğrula (manuel kontrol)
    /// </summary>
    [HttpPost("{serialId:guid}/retry-verify")]
    public async Task<ActionResult<VerifyResult>> RetryVerify(Guid serialId)
    {
        var result = await _cameraService.RetryVerifyAsync(serialId);
        return Ok(result);
    }

    /// <summary>
    /// Üretim durumu istatistiklerini al
    /// </summary>
    [HttpGet("{workOrderId:guid}/status")]
    public async Task<ActionResult<ProductionStatus>> GetStatus(Guid workOrderId)
    {
        try
        {
            var result = await _plcService.GetProductionStatusAsync(workOrderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
