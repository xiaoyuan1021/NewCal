using System;
using System.Text;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace NewCal.ViewModels
{
    public class CalculatorViewModel : BindableBase,INavigationAware
    {
        // 状态
        private string _display = "";
        private string CurrentInput = "";    // 当前正在输入的数字（右操作数）
        private double CurrentValue = 0;     // 已保存的左操作数
        private string Operator = null;      // 当前操作符，null 表示还没选运算符
        private bool isNewValue = true;      // 是否开始输入新值（按了运算符后为 true）

        private readonly IDialogService _dialogService;
        private  IRegionNavigationJournal _journal;

        public DelegateCommand<string> AppendNumberCommand { get; }
        public DelegateCommand<string> SetOperatorCommand { get; }
        public DelegateCommand AppendDotCommand { get; }
        public DelegateCommand ClearCommand { get; }
        public DelegateCommand CalculateCommand { get; }
        public DelegateCommand GoBackCommand { get; } 

        public CalculatorViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            GoBackCommand = new DelegateCommand(GoBack);
             AppendNumberCommand = new DelegateCommand<string>(AppendNumber);
            SetOperatorCommand = new DelegateCommand<string>(SetOperator);
            AppendDotCommand = new DelegateCommand(AppendDot);
            ClearCommand = new DelegateCommand(Clear);
            CalculateCommand = new DelegateCommand(Calculate);

            // 初始显示
            Display = "";
        }

       

        public string Display
        {
            get => _display;
            set => SetProperty(ref _display, value);
        }

        private void Clear()
        {
            Display = "";
            CurrentInput = "";
            CurrentValue = 0;
            Operator = null;
            isNewValue = true;
        }

        private void AppendNumber(string number)
        {
            // 如果刚开始新值（按了运算符），把当前输入清空并开始新输入
            if (isNewValue)
            {
                CurrentInput = number;
                isNewValue = false;
            }
            else
            {
                CurrentInput += number;
            }

            // 显示：如果有运算符，显示为 "left + right"，否则只显示当前输入
            Display = Operator == null ? CurrentInput : $"{CurrentValue}{Operator}{CurrentInput}";
        }

        private void AppendDot()
        {
            // 在当前输入里添加小数点（防重复）
            if (isNewValue)
            {
                CurrentInput = "0.";
                isNewValue = false;
            }
            else if (!CurrentInput.Contains("."))
            {
                CurrentInput += ".";
            }

            Display = Operator == null ? CurrentInput : $"{CurrentValue}{Operator}{CurrentInput}";
        }

        private void SetOperator(string op)
        {
            if (string.IsNullOrWhiteSpace(op)) return;
            op = op.Trim();

            // 如果还没有选择运算符（第一次按运算符）
            if (Operator == null)
            {
                // 尝试把当前显示（或当前输入）当成左操作数
                if (!string.IsNullOrEmpty(CurrentInput) && double.TryParse(CurrentInput, out double val))
                {
                    CurrentValue = val;
                }
                else
                {
                    // 如果没输入任何数字，则尝试从 Display 解析（Display 初始为 "0"）
                    double.TryParse(Display, out CurrentValue);
                }

                Operator = op;
                isNewValue = true;
                CurrentInput = ""; // 开始输入右操作数
                Display = $"{CurrentValue}{Operator}";
                return;
            }

            // 已经有操作符了：
            if (isNewValue)
            {
                // 用户连续按运算符 -> 替换显示的运算符（例如 "5+" -> "5-"）
                Display = Display.Substring(0, Display.Length - 1) + op;
                Operator = op;
            }
            else
            {
                // 用户已经输入了右操作数，按另一个运算符：先计算当前结果，再把结果作为左操作数和新的运算符连接
                if (double.TryParse(CurrentInput, out double right))
                {
                    double left = CurrentValue;
                    double result = Compute(left, right, Operator);
                    CurrentValue = result;
                    Operator = op;
                    CurrentInput = "";
                    isNewValue = true;
                    // 显示结果后跟新的运算符
                    Display = $"{CurrentValue}{Operator}";
                }
                else
                {
                    // 右操作数解析失败，直接替换运算符
                    Display = Display.Substring(0, Display.Length - 1) + op;
                    Operator = op;
                    isNewValue = true;
                    CurrentInput = "";
                }
            }
        }

        private double Compute(double left, double right, string op)
        {
            return op switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => right == 0 ? double.NaN : left / right,
                _ => double.NaN
            };
        }

        private void Calculate()
        {
            // 如果没有操作符，直接返回（不报错），或者你可以选择把 Display 设为当前输入
            if (Operator == null)
            {
                // 没有操作就不做任何计算，保持当前显示
                return;
            }

            // 如果没有右操作数（用户按了运算符但没输入右值），我们也不执行计算（更友好）
            if (string.IsNullOrEmpty(CurrentInput))
            {
                // 选择行为：不做运算，仅将显示调整为左值（保留运算符也可以）
                Display = CurrentValue.ToString();
                Operator = null;
                isNewValue = true;
                return;
            }

            // 正常情况：有左 + op + 右
            if (double.TryParse(CurrentInput, out double right))
            {
                double left = CurrentValue;
                double result = Compute(left, right, Operator);

                if (double.IsNaN(result)||double.IsInfinity(result))
                {
                    var parameters=new DialogParameters 
                    { { "message", "计算错误：无效运算（可能是除以 0 或其他异常）" } };
                    _dialogService.ShowDialog("ErrorDialog", parameters, r =>
                    {
                    });
                    Clear();
                    return;
                }
                // 显示结果，准备下一次输入
                Display = result.ToString();
                CurrentValue = result;
                CurrentInput = "";
                Operator = null;
                isNewValue = true;
            }
            else
            {
                // 解析失败
                var parameters = new DialogParameters { { "message", "计算错误：无法解析输入为数字。" } };
                _dialogService.ShowDialog("ErrorDialog", parameters, r => { });


                CurrentInput = "";
                Operator = null;
                isNewValue = true;
            }
        }


        private void GoBack()
        {
            if(_journal!=null && _journal.CanGoBack)
            {

                _journal.GoBack();
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
           _journal= navigationContext.NavigationService.Journal;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)=>true;
       

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
    }
}
