using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace NewCal.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public DelegateCommand<string> NavigateCommand { get; }

        public MainViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);

        }

        private void Navigate(string viewname)
        {
            if (!string.IsNullOrEmpty(viewname))
            {
                _regionManager.RequestNavigate("ContentRegion", viewname);

            }
        }
    }
}
