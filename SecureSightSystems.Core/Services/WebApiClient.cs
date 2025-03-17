using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ChannelMetadata = SecureSightSystems.Core.Models.ChannelMetadata;

namespace SecureSightSystems.Core.Services
{
    public class WebApiClient : IApiClient, IDisposable
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly HttpClient client;

        // TODO: Move out to config
        public static readonly string BaseUrl = "http://demo.macroscop.com:8080";

        public WebApiClient(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;

            client = clientFactory.CreateClient("videodemo");

            client.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<IEnumerable<ChannelMetadata>> GetChannelsAsync()
        {
            var raw = await client.GetStringAsync("/configex?login=root");

            return await Task.Run(() => getChannelsFromXmlString(raw));
        }

        /// Gets video stream output with given camera's resolution
        /// </summary>
        /// <param name="channelId">Camera's GUID</param>
        /// <param name="resolutionX">Camera's widht in pixels</param>
        /// <param name="resolutionY">Camera's height in pixels</param>
        /// <exception cref="HttpRequestException"/>
        public async Task<Stream> GetVideoDataAsync(string channelId, int resolutionX, int resolutionY)
        {
            string url = $"mobile?login=root&channelid={channelId}&resolutionX={resolutionX}&resolutionY={resolutionY}&fps=25";

            return await client.GetStreamAsync(url);
        }

        private IEnumerable<ChannelMetadata> getChannelsFromXmlString(string xmlString)
        {
            XmlDocument document = new XmlDocument();

            var channels = new List<ChannelMetadata>();
            
            document.LoadXml(xmlString);

            XmlElement root = document.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name == "Channels")
                {
                    foreach (XmlNode cnode in node.ChildNodes)
                    {
                        var attrs = cnode.Attributes;

                        ChannelMetadata channel = new ChannelMetadata
                        {
                            ChannelId = attrs.GetNamedItem("Id").Value,
                            Name = attrs.GetNamedItem("Name").Value,
                            ServerId = attrs.GetNamedItem("AttachedToServer").Value,
                            IsDisabled = Boolean.Parse(attrs.GetNamedItem("IsDisabled").Value),
                            IsSoundOn = Boolean.Parse(attrs.GetNamedItem("IsSoundOn").Value)
                        };

                        channels.Add(channel);
                    }
                }
            }

            return channels;
        }

        public void Dispose()
        {
            
        }
    }
}
