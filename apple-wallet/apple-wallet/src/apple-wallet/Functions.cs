using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Options;
using Passbook.Generator;
using Passbook.Generator.Fields;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace apple_wallet
{
    public class Functions
    {
        AppleWalletConfiguration _appleWalletConfiguration;
        public Functions(IOptionsSnapshot<AppleWalletConfiguration> appleWalletConfiguration)
        {
            _appleWalletConfiguration = appleWalletConfiguration.Value;
        }

        [LambdaFunction()]
        [HttpApi(LambdaHttpMethod.Get, "/apple-wallet/{orderId}/{eventId}")]
        public async Task<APIGatewayHttpApiV2ProxyResponse> DefaultAsync(string orderId, string eventId)
        {
            var request = await GetPassRequestAsync(orderId, eventId);
            var generator = new PassGenerator();
            var passBundle = generator.Generate(request);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = Convert.ToBase64String(passBundle),
                IsBase64Encoded = true,
                StatusCode = 200,
                Headers = new Dictionary<string, string>()
                {
                    { "Content-Type", "application/vnd.apple.pkpasses" },
                    { "Content-Disposition", "attachment; filename=tickets.pkpasses.zip; filename*=UTF-8''tickets.pkpasses.zip" }
                }
            };
        }

        private async Task<PassGeneratorRequest[]> GetPassRequestAsync(string orderId, string eventId)
        {
            var eventName = "YouTube Demo Event";
            var venueName = "YouTube Online";
            var tickeType = "Subscriber";
            var eventDate = DateTime.Now.AddDays(55);

            var icon = await _appleWalletConfiguration.GetIcon();
            var logo = await _appleWalletConfiguration.GetLogo();

            var ticketId = Guid.NewGuid();
            var request = new PassGeneratorRequest
            {
                Style = PassStyle.EventTicket,
                PassTypeIdentifier = _appleWalletConfiguration.PassTypeIdentifier,
                SerialNumber = ticketId.ToString(),
                GroupingIdentifier = eventId.ToString(),
                BackgroundColor = "#823EB7",
                LabelColor = "#000000",
                ForegroundColor = "#ffffff",
                Images =
            {
                {PassbookImage.Icon, icon},
                {PassbookImage.Icon2X, icon},
                {PassbookImage.Icon3X, icon},
                {PassbookImage.Logo, logo},
                {PassbookImage.Logo2X, logo},
                {PassbookImage.Logo3X, logo},
            },
                Description = eventName,
                OrganizationName = "Rahul",
                RelevantDate = eventDate,
                ExpirationDate = eventDate.AddDays(1),
                AppleWWDRCACertificate = _appleWalletConfiguration.AppleWWDRCACertificate(),
                PassbookCertificate = _appleWalletConfiguration.PassbookCertificate()
            };
            request.AddHeaderField(new StandardField("time", eventDate.ToShortTimeString(),
                eventDate.ToShortDateString()));
            request.AddPrimaryField(new StandardField("name", null, eventName));
            request.AddSecondaryField(new StandardField("venue", "Venue", venueName));
            request.AddAuxiliaryField(new StandardField("ticketType", "Ticket Type", tickeType));
            request.AddBackField(new StandardField("ticketHolderName-back", "Ticket holder", "Rahul Nath"));
            request.AddBackField(new StandardField("ticketType-back", "Ticket Type", tickeType));

            return new[] { request };
        }
    }
}
