using ApiPbiTreinamento.Domain.Dto;
using System;
using System.Threading.Tasks;

namespace ApiPbiTreinamento.Services
{
    public interface IPowerBiService
    {
        Task<EmbedConfig> GetToken(Guid workspaceid, Guid reportid, string email);
    }
}
