using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VideoCaptureApplication
{
    public class CAM_Client
    {
        internal const string server_url = "http://demo.macroscop.com:8080/";
        internal string? Id { get; set; }
        internal string? Name { get; set; }
        internal string? Description { get; set; }
        internal string? DeviceInfo { get; set; }
        internal string? AttachedToServer { get; set; }
        internal bool IsDisabled { get; set; }
        internal bool IsSoundOn { get; set; }
        internal bool IsArchivingEnabled { get; set; }
        internal bool IsSoundArchivingEnabled { get; set; }
        internal bool AllowedRealtime { get; set; }
        internal bool IsPtzOn { get; set; }
        internal bool IsTransmitSoundOn { get; set; }
        internal string? ArchiveMode { get; set; }
        internal string? ArchiveStreamType { get; set; }
        internal string? ArchiveVideoFormat { get; set; }
        internal string? ArchiveRotationMode { get; set; }
        internal bool IsFaceAnalystEnabled { get; set; }
        internal bool IsPeopleCountingOn { get; set; }
        internal string? TimeZoneOffset { get; set; }

        public CAM_Client(XElement channelInfoElement)
        {
            this.Id = channelInfoElement.Attribute("Id")?.Value;
            this.Name = channelInfoElement.Attribute("Name")?.Value;
            this.Description = channelInfoElement.Attribute("Description")?.Value;
            this.DeviceInfo = channelInfoElement.Attribute("DeviceInfo")?.Value;
            this.AttachedToServer = channelInfoElement.Attribute("AttachedToServer")?.Value;
            this.IsDisabled = Convert.ToBoolean(channelInfoElement.Attribute("IsDisabled")?.Value);
            this.IsSoundOn = Convert.ToBoolean(channelInfoElement.Attribute("IsSoundOn")?.Value);
            this.IsArchivingEnabled = Convert.ToBoolean(channelInfoElement.Attribute("IsArchivingEnabled")?.Value);
            this.IsSoundArchivingEnabled = Convert.ToBoolean(channelInfoElement.Attribute("IsSoundArchivingEnabled")?.Value);
            this.AllowedRealtime = Convert.ToBoolean(channelInfoElement.Attribute("AllowedRealtime")?.Value);
            this.IsPtzOn = Convert.ToBoolean(channelInfoElement.Attribute("IsPtzOn")?.Value);
            this.IsTransmitSoundOn = Convert.ToBoolean(channelInfoElement.Attribute("IsTransmitSoundOn")?.Value);
            this.ArchiveMode = channelInfoElement.Attribute("ArchiveMode")?.Value;
            this.ArchiveVideoFormat = channelInfoElement.Attribute("ArchiveVideoForma")?.Value;
            this.ArchiveRotationMode = channelInfoElement.Attribute("ArchiveRotationMode")?.Value;
            this.IsFaceAnalystEnabled = Convert.ToBoolean(channelInfoElement.Attribute("IsFaceAnalystEnabled")?.Value);
            this.IsPeopleCountingOn = Convert.ToBoolean(channelInfoElement.Attribute("IsPeopleCountingOn")?.Value);
            this.TimeZoneOffset = channelInfoElement.Attribute("TimeZoneOffset")?.Value;
        }

        private static string GetConfig(string login = "root")
        {
            return $"{server_url}configex?login={login}";
        }

        public static async Task<XDocument> GetCameras()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string xmlUrl = GetConfig();
                    var response = await client.GetAsync(xmlUrl);
                    var xmlContent = await response.Content.ReadAsStringAsync();
                    XDocument xmlDoc = XDocument.Parse(xmlContent);
                    return xmlDoc;
                }
                catch (WebException e)
                {
                    CAM_Show.ErrorMessageRequest(e.Message);
                    return null;
                }
            }
        }

        private static List<CAM_Show> ParseXMLConfig(XDocument xmlDoc)
        {
            List<CAM_Show> channelInfos = new List<CAM_Show>();
            var channelInfoElements = xmlDoc.Descendants("ChannelInfo");
            foreach (var channelInfoElement in channelInfoElements)
            {
                var element = new CAM_Show(channelInfoElement);
                channelInfos.Add(element);
            }
            return channelInfos;
        }

        public static async Task<List<CAM_Show>> GetChannelsInfo()
        {
            XDocument xmlDoc = await GetCameras();
            return ParseXMLConfig(xmlDoc);
        }
    }
}
