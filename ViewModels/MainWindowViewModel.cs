using Prism.Mvvm;

namespace NewCal.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "简易计算器页面-by小猿本尊";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
