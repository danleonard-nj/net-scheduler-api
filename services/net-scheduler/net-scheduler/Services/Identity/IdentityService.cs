namespace NetScheduler.Services.Identity;

using Microsoft.Identity.Web;
using NetScheduler.Configuration;
using NetScheduler.Services.Identity.Abstractions;
using System.Threading.Tasks;

public class IdentityService : IIdentityService
{
	private readonly ITokenAcquisition _tokenAcquisition;

	private readonly ILogger<IdentityService> _logger;

	public IdentityService(
		ITokenAcquisition tokenAcquisition,
		ILogger<IdentityService> logger)
	{
		ArgumentNullException.ThrowIfNull(tokenAcquisition, nameof(tokenAcquisition));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_tokenAcquisition = tokenAcquisition;
		_logger = logger;
	}

	public async Task<string> GetAccessTokenAsync(
		string clientId,
		CancellationToken token = default)
	{
		if (string.IsNullOrWhiteSpace(clientId))
		{
			throw new ArgumentNullException(nameof(clientId));
		}

		_logger.LogInformation(
			"{@Method}: {@ClientId}: Fetching token",
			Caller.GetName(),
			clientId);

		var authToken = await _tokenAcquisition.GetAccessTokenForAppAsync(
			clientId);

		return authToken;
	}

	public async Task<object> GetAuthorizationHeadersAsync(
		string clientId,
		CancellationToken cancellationToken = default)
	{
		var token = await this.GetAccessTokenAsync(
			clientId,
			cancellationToken);

		return new
		{
			Authorization = $"Bearer {token}"
		};
	}
}
