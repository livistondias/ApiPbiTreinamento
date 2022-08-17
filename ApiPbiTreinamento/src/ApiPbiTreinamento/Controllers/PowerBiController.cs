using ApiPbiTreinamento.Domain.Dto;
using ApiPbiTreinamento.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ApiPbiTreinamento.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PowerBiController : Controller
    {
        private IPowerBiService Service;
        public PowerBiController(IPowerBiService service)
        {
            Service = service;
        }
        [HttpGet("{workspaceid}/{reportid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<EmbedConfig>> GetAsync(Guid workspaceid , Guid reportid, string email)
        {
            return Ok(await Service.GetToken(workspaceid, reportid, email));
        }
    }
}