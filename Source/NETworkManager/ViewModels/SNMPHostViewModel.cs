﻿using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.ViewModels
{
    public class SNMPHostViewModel : ViewModelBase
    {
        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        public ObservableCollection<DragablzSNMPTabItem> TabItems { get; private set; }

        private const string tagIdentifier = "tag=";

        private bool _isLoading = true;

        private int _tabId = 0;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (value == _selectedTabIndex)
                    return;

                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public SNMPHostViewModel()
        {
            InterTabClient = new DragablzTracerouteInterTabClient();

            TabItems = new ObservableCollection<DragablzSNMPTabItem>()
            {
                new DragablzSNMPTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new SNMPView (_tabId), _tabId)
            };

            _isLoading = false;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddSNMPTabCommand
        {
            get { return new RelayCommand(p => AddSNMPTabAction()); }
        }

        private void AddSNMPTabAction()
        {
            AddSNMPTab();
        }

        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzSNMPTabItem).View as SNMPView).CloseTab();
        }
        #endregion

        #region Methods
        private void AddSNMPTab()
        {
            _tabId++;

            TabItems.Add(new DragablzSNMPTabItem(LocalizationManager.GetStringByKey("String_Header_NewTab"), new SNMPView(_tabId), _tabId));

            SelectedTabIndex = TabItems.Count - 1;
        }
        #endregion
    }
}