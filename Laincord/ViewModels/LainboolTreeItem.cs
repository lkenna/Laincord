using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laincord.ViewModels
{
    public class LainboolTreeItemViewModel : ViewModelBase
    { 
        private string _name;
        private string _type;
        private object _value;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public ObservableCollection<LainboolTreeItemViewModel> Children { get; } = [];
    }
}
