using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ChannelMetadata = SecureSightSystems.Core.Models.ChannelMetadata;

namespace SecureSightSystems.Core.Services
{
    public interface IApiClient
    {
        /// <summary>
        /// Gets all avaiable camera channels
        /// </summary>
        /// <exception cref="SocketException"/>
        Task<IEnumerable<ChannelMetadata>> GetChannelsAsync();

        /// Gets video stream output with given camera's resolution
        /// </summary>
        /// <param name="channelId">Camera's GUID</param>
        /// <param name="resolutionX">Camera's widht in pixels</param>
        /// <param name="resolutionY">Camera's height in pixels</param>
        /// <exception cref="HttpRequestException"/>
        Task<Stream> GetVideoDataAsync(string channelId, int resolutionX, int resolutionY);
    }
}
