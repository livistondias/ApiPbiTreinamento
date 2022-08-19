using ApiPbiTreinamento.Domain.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPbiTreinamento.Services
{
    public class PowerBiService : IPowerBiService
    {
        public IConfiguration Configuration { get; }
        private readonly string Username = "";
        private readonly string Password = "";        
        private readonly string ResourceUrl = "";
        private readonly string ApplicationId = "";
        private readonly string TenantId = "";
        private readonly string ClientSecret = "";
        private readonly string ApiUrl = "";

        public PowerBiService(IConfiguration configuration)
        {
            Configuration = configuration;
            Username = Configuration.GetSection("PowerBi").GetSection("pbiUsername").Value;
            Password = Configuration.GetSection("PowerBi").GetSection("pbiPassword").Value;            
            ResourceUrl = Configuration.GetSection("PowerBi").GetSection("resourceUrl").Value;
            ApplicationId = Configuration.GetSection("PowerBi").GetSection("ApplicationId").Value;
            TenantId = Configuration.GetSection("PowerBi").GetSection("TenantId").Value;
            ApiUrl = Configuration.GetSection("PowerBi").GetSection("apiUrl").Value;
            ClientSecret = Configuration.GetSection("PowerBi").GetSection("ClientSecret").Value;

        }
        public async Task<EmbedConfig> GetToken(Guid workspaceid, Guid reportid, string email)
        {
            var result = new EmbedConfig();
            string username = email;
            string roles = "RLS Leaders";
            try
            {
                result = new EmbedConfig { Username = username, Roles = roles };
                
                
                var pbi = new PowerBiAuth.Authentication(ClientSecret);
                var accessToken = await pbi.AuthenticationClientContext(ResourceUrl, ApplicationId, TenantId);
                
                var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(ApiUrl), tokenCredentials))
                {
                    // Get a list of reports.
                    var reports = await client.Reports.GetReportsInGroupAsync(workspaceid);

                    // No reports retrieved for the given workspace.
                    if (reports.Value.Count() == 0)
                    {
                        result.ErrorMessage = "No reports were found in the workspace";
                        return result;
                    }

                    Report report;
                    if (string.IsNullOrWhiteSpace(reportid.ToString()))
                    {
                        // Get the first report in the workspace.
                        report = reports.Value.FirstOrDefault();
                    }
                    else
                    {
                        report = reports.Value.FirstOrDefault(r => r.Id == reportid);
                    }

                    if (report == null)
                    {
                        result.ErrorMessage = "No report with the given ID was found in the workspace. Make sure ReportId is valid.";
                        return result;
                    }

                    var datasets = await client.Datasets.GetDatasetInGroupAsync(workspaceid, report.DatasetId);
                    result.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
                    result.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;
                    GenerateTokenRequest generateTokenRequestParameters;
                    // This is how you create embed token with effective identities
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var rls = new EffectiveIdentity(username, new List<string> { report.DatasetId });
                        if (!string.IsNullOrWhiteSpace(roles))
                        {
                            var rolesList = new List<string>();
                            rolesList.AddRange(roles.Split(','));
                            rls.Roles = rolesList;
                        }
                        // Generate Embed Token with effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view", identities: new List<EffectiveIdentity> { rls });
                    }
                    else
                    {
                        // Generate Embed Token for reports without effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    }

                    var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(workspaceid, report.Id, generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        result.ErrorMessage = "Failed to generate embed token.";
                        return result;
                    }

                    // Generate Embed Configuration.
                    result.EmbedToken = tokenResponse;
                    result.EmbedUrl = report.EmbedUrl;
                    result.Id = report.Id.ToString();

                    return result;
                }
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }

            return result;
        }
    }
}
