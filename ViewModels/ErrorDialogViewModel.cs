using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewCal.ViewModels
{
    public class ErrorDialogViewModel : BindableBase,IDialogAware
    {
        private readonly IDialogService _dialogService;
        private string _message;
        public string Message
        {
            get => _message;
            set=>SetProperty(ref _message,value);
        }
        public DelegateCommand CloseCommand { get; }



        //构造函数
        public ErrorDialogViewModel()
        {
            CloseCommand = new DelegateCommand(Onclose);
            //_dialogService = dialogService;
        }

       

        public string Title => "温馨提示";

        public event Action<IDialogResult> RequestClose;

        private void Onclose()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if(parameters!=null && parameters.ContainsKey("message"))
            {
                Message = parameters.GetValue<string>("message");
            }
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {

        }

    }
}
