
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rock.Apps.CheckScannerUtility.Models
{
    public class DisplayAccount : INotifyPropertyChanged
    {
        private bool _accountIsChecked;
        private string _accountDisplayName;
        private ObservableCollection<DisplayAccount> _children;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }

        public string AccountDisplayName
        {
            get { return _accountDisplayName; }
            set
            {
                _accountDisplayName = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "AccountDisplayName" ) );
            }
        }
        public bool IsAccountChecked
        {
            get { return _accountIsChecked; }
            set
            {
                _accountIsChecked = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "IsAccountChecked" ) );
            }
        }

        public ObservableCollection<DisplayAccount> Children
        {
            get { return _children; }
            set
            {
                _children = value; 
            }
        }

    

    }
}
