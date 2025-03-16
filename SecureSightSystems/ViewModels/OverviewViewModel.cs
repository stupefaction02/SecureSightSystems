using SecureSightSystems.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SecureSightSystems.Services;
using System.Net.Http;
using System.Collections.Generic;
using System;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using SecureSightSystems.Core.Extensions;
using SecureSightSystems.Core.Services;
using SecureSightSystems.Core;

namespace SecureSightSystems.ViewModels
{
    public class OverviewViewModel : BaseViewModel
    {
        #region Commands

        public ICommand PageLoaded { get; set; }

        public ICommand SelectChannel { get; set; }

        #endregion

        private readonly IApiClient apiClient;

        private readonly FrameController frameController;
        private readonly ApplicationInfoManager infoManager;
        private ChannelMetadata selectedChannel = ChannelMetadata.Empty;

        public ChannelMetadata SelectedChannel 
        {
            get => selectedChannel; 
            set
            {
                selectedChannel = value;
                OnPropertyChanged(nameof(SelectedChannel));
            }
        }

        private bool showNoSignal;
        public bool ShowNoSignal
        {
            get => showNoSignal;
            set
            {
                showNoSignal = value;
                OnPropertyChanged(nameof(ShowNoSignal));
            }
        }

        private bool noDataMessageShown;
        public bool NoDataMessageShown
        {
            get => noDataMessageShown;
            set
            {
                noDataMessageShown = value;
                OnPropertyChanged(nameof(NoDataMessageShown));
            }
        }

        public ObservableCollection<ChannelMetadata> Channels { get; set; }

        #region Representation

        private BitmapImage mainImage;

        public BitmapImage MainImage
        {
            get => mainImage;
            set 
            {
                mainImage = value; 
                OnPropertyChanged(nameof(MainImage)); 
            }
        }

        private readonly BitmapImage noDataImage = new BitmapImage(new System.Uri("pack://application:,,,/Images/no_data.png"));
        private readonly BitmapImage noSignalImage = new BitmapImage(new System.Uri("pack://application:,,,/Images/no_signal.png"));
        private readonly BitmapImage loadingImage = new BitmapImage(new System.Uri("pack://application:,,,/Images/loading.png"));

        #endregion

        private Dispatcher Dispatcher;
        private readonly ILogger defaultLogger;

        public OverviewViewModel(IApiClient apiClient, FrameController parser, ApplicationInfoManager infoManager, ILoggerFactory loggerFactory)
        {
            this.apiClient = apiClient;
            this.infoManager = infoManager;

            frameController = parser;
            Dispatcher = Application.Current.Dispatcher;

            defaultLogger = loggerFactory.CreateLogger(nameof(OverviewViewModel));

            frameController.FrameReceived += bitmapImage =>
            {
                Dispatcher.Invoke(() => 
                {
                    //MainImage.Freeze();
                    MainImage = bitmapImage.ConvertToImage();
                });
            };

            frameController.ReadNetworkStreamError += FrameController_ReadNetworkStreamError;
            frameController.HostIsUnreachableError += FrameController_HostIsUnreachableError;
            frameController.NoDataOccured += FrameController_NoDataOccured;

            PageLoaded = new RelayVoidCommand(HandlePageLoadedAsync);
            SelectChannel = new RelayCommand(async channelId => await SelectChannelAsync(channelId.ToString()));
        }

        private void FrameController_ReadNetworkStreamError(Exception ex)
        {
            infoManager.ShowInfoMessage(new InfoErrorMessage { Text = "Error: Breach of connection during transmission." });

            Dispatcher.Invoke(() =>
            {
                MainImage = noSignalImage;
            });
        }

        private void FrameController_HostIsUnreachableError(Exception ex)
        {
            infoManager.ShowInfoMessage(new InfoErrorMessage { Text = "Error: host is unreachable." });

            Dispatcher.Invoke(() =>
            {
                MainImage = noSignalImage;
            });
        }

        private void FrameController_NoDataOccured()
        {
            if (!NoDataMessageShown)
            {
                MainImage = noDataImage;

                NoDataMessageShown = true;
            }
        }

        private async Task SelectChannelAsync(string channelId)
        {
            ShowLoadingScreen();

            var oldChannel = SelectedChannel;
            bool notSame = oldChannel?.ChannelId != channelId;
            if (channelId != null && notSame)
            {
                var newChannel = _channelsStore[channelId];

                if (!newChannel.IsDisabled)
                {
                    await frameController.SetChannelAsync(newChannel);

                    SelectedChannel = newChannel;
                }

                defaultLogger.LogInformation($"Changing channel. Old = {{ {oldChannel.ToString()} }}, New = {{ {newChannel.ToString()} }}");
            }
        }

        private void ShowLoadingScreen()
        {
            MainImage = loadingImage;
        }

        private Dictionary<string, ChannelMetadata> _channelsStore = new Dictionary<string, ChannelMetadata>();

        private async void HandlePageLoadedAsync()
        {
            try
            {
                var channels = await apiClient.GetChannelsAsync();
                Channels = new ObservableCollection<ChannelMetadata>(channels);

                foreach (var ch in channels)
                {
                    _channelsStore.Add(ch.ChannelId, ch);
                }

                OnPropertyChanged(nameof(Channels));
            }
            catch (HttpRequestException)
            {
                infoManager.ShowInfoMessage(new InfoErrorMessage { Text = "Internet Connection Error!" });
                return;
            }

            infoManager.HideInfoMessage();

            if (Channels.Any())
            {
                await SelectChannelAsync(Channels[0].ChannelId);
            }
        }
    }
}

