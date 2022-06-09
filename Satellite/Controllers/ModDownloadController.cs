using Microsoft.AspNetCore.Mvc;
using Satellite.Core;

namespace Satellite.Controllers;

[ApiController]
[Route("mods/download/{id}")]
public class ModDownloadController : ControllerBase
{
    private readonly ModResolver _modResolver;
    private readonly FileResolver _fileResolver;

    public ModDownloadController(ModResolver modResolver, FileResolver fileResolver)
    {
        this._modResolver = modResolver;
        this._fileResolver = fileResolver;
    }

    [HttpGet]
    public async Task<ActionResult> Download(
        string id,
        [FromQuery(Name = "version")] string? version,
        [FromQuery(Name = "game_version")] string? gameVersion)
    {
        var modInfo = await this._modResolver.GetModInfo(id);
        if (modInfo == null)
        {
            return this.BadRequest();
        }

        var downloadStream = await this._modResolver.GetDownloadStream(id, modInfo, version, gameVersion);
        if (downloadStream == null)
        {
            return this.NotFound();
        }

        return new FileStreamResult(downloadStream, "application/octet-stream")
        {
            FileDownloadName = modInfo.DownloadName
        };
    }
}