using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace NewCal.ViewModels
{
    public class AboutViewModel:INavigationAware
    {
        private IRegionNavigationJournal _journal;

        public DelegateCommand GoBackCommand {  get;  }


        public AboutViewModel()
        {
            GoBackCommand = new DelegateCommand(GoBack);
        }

        private void GoBack()
        {
            if(_journal != null && _journal.CanGoBack )
            {
                _journal.GoBack();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        

        

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _journal = navigationContext.NavigationService.Journal;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
    }
}
