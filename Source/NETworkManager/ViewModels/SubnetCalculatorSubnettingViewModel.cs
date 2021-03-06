﻿using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Windows.Input;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System.Linq;
using System.Net;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorSubnettingViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        private string _subnet;
        public string Subnet
        {
            get { return _subnet; }
            set
            {
                if (value == _subnet)
                    return;

                _subnet = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _subnetHistoryView;
        public ICollectionView SubnetHistoryView
        {
            get { return _subnetHistoryView; }
        }

        private string _newSubnetmaskOrCIDR;
        public string NewSubnetmaskOrCIDR
        {
            get { return _newSubnetmaskOrCIDR; }
            set
            {
                if (value == _newSubnetmaskOrCIDR)
                    return;

                _newSubnetmaskOrCIDR = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _newSubnetmaskOrCIDRHistoryView;
        public ICollectionView NewSubnetmaskOrCIDRHistoryView
        {
            get { return _newSubnetmaskOrCIDRHistoryView; }
        }

        private bool _isCalculationRunning;
        public bool IsCalculationRunning
        {
            get { return _isCalculationRunning; }
            set
            {
                if (value == _isCalculationRunning)
                    return;

                _isCalculationRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isResultVisible;
        public bool IsResultVisible
        {
            get { return _isResultVisible; }
            set
            {
                if (value == _isResultVisible)
                    return;


                _isResultVisible = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IPNetwork> _subnetsResult = new ObservableCollection<IPNetwork>();
        public ObservableCollection<IPNetwork> SubnetsResult
        {
            get { return _subnetsResult; }
            set
            {
                if (value == _subnetsResult)
                    return;

                _subnetsResult = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _subnetsResultView;
        public ICollectionView SubnetsResultView
        {
            get { return _subnetsResultView; }
        }

        private IPNetwork _selectedSubnetResult;
        public IPNetwork SelectedSubnetResult
        {
            get { return _selectedSubnetResult; }
            set
            {
                if (value == _selectedSubnetResult)
                    return;

                _selectedSubnetResult = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SubnetCalculatorSubnettingViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Set collection view
            _subnetHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory);
            _newSubnetmaskOrCIDRHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory);

            // Result view
            _subnetsResultView = CollectionViewSource.GetDefaultView(SubnetsResult);
        }
        #endregion

        #region ICommands & Actions
        public ICommand SubnettingCommand
        {
            get { return new RelayCommand(p => SubnettingAction()); }
        }

        private void SubnettingAction()
        {
            Subnetting();
        }

        public ICommand CopySelectedNetworkAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedNetworkAddressAction()); }
        }

        private void CopySelectedNetworkAddressAction()
        {
            Clipboard.SetText(SelectedSubnetResult.Network.ToString());
        }

        public ICommand CopySelectedBroadcastCommand
        {
            get { return new RelayCommand(p => CopySelectedBroadcastAction()); }
        }

        private void CopySelectedBroadcastAction()
        {
            Clipboard.SetText(SelectedSubnetResult.Broadcast.ToString());
        }

        public ICommand CopySelectedIPAddressesCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressesAction()); }
        }

        private void CopySelectedIPAddressesAction()
        {
            Clipboard.SetText(SelectedSubnetResult.Total.ToString());
        }

        public ICommand CopySelectedSubnetmaskCommand
        {
            get { return new RelayCommand(p => CopySelectedSubnetmaskAction()); }
        }

        private void CopySelectedSubnetmaskAction()
        {
            Clipboard.SetText(SelectedSubnetResult.Netmask.ToString());
        }

        public ICommand CopySelectedCIDRCommand
        {
            get { return new RelayCommand(p => CopySelectedCIDRAction()); }
        }

        private void CopySelectedCIDRAction()
        {
            Clipboard.SetText(SelectedSubnetResult.Cidr.ToString());
        }

        public ICommand CopySelectedFirstIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedFirstIPAddressAction()); }
        }

        private void CopySelectedFirstIPAddressAction()
        {
            Clipboard.SetText(SelectedSubnetResult.FirstUsable.ToString());
        }

        public ICommand CopySelectedLastIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedLastIPAddressAction()); }
        }

        private void CopySelectedLastIPAddressAction()
        {
            Clipboard.SetText(SelectedSubnetResult.LastUsable.ToString());
        }

        public ICommand CopySelectedHostCommand
        {
            get { return new RelayCommand(p => CopySelectedHostAction()); }
        }

        private void CopySelectedHostAction()
        {
            Clipboard.SetText(SelectedSubnetResult.Usable.ToString());
        }
        #endregion

        #region Methods
        private async void Subnetting()
        {
            DisplayStatusMessage = false;
            IsCalculationRunning = true;

            SubnetsResult.Clear();

            IPNetwork subnet = IPNetwork.Parse(Subnet);
            byte.TryParse(NewSubnetmaskOrCIDR.TrimStart('/'), out byte newCidr);

            // Ask the user if there is a large calculation...
            int baseCidr = subnet.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;

            if (65535 < (Math.Pow(2, (baseCidr - subnet.Cidr)) / Math.Pow(2, (baseCidr - newCidr))))
            {
                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_Continue");
                settings.NegativeButtonText = LocalizationManager.GetStringByKey("String_Button_Cancel");

                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                if (await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_AreYouSure"), LocalizationManager.GetStringByKey("String_TheProcessCanTakeUpSomeTimeAndResources"), MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                {
                    IsCalculationRunning = false;

                    return;
                }
            }

            // This still slows the application / freezes the ui... there are to many updates to the ui thread...
            await Task.Run(() =>
            {
                foreach (IPNetwork network in subnet.Subnet(newCidr))
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
                    {
                        lock (SubnetsResult)
                            SubnetsResult.Add(network);
                    }));
                }
            });

            IsResultVisible = true;

            AddSubnetToHistory(Subnet);
            AddNewSubnetmaskOrCIDRToHistory(NewSubnetmaskOrCIDR);

            IsCalculationRunning = false;
        }

        private void AddSubnetToHistory(string subnet)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.Clear();
            OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.Add(x));
        }

        private void AddNewSubnetmaskOrCIDRToHistory(string newSubnetmaskOrCIDR)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.ToList(), newSubnetmaskOrCIDR, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.Clear();
            OnPropertyChanged(nameof(NewSubnetmaskOrCIDR)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.Add(x));
        }

        public void OnShutdown()
        {

        }
        #endregion
    }
}