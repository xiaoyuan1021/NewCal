using System.Windows;
using NewCal.Views;
using Prism.Ioc;
using Prism.Regions;

namespace NewCal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainView>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<CalculatorView>();
            
            containerRegistry.RegisterDialog<ErrorDialog>();
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            // 自动导航到 MainView（目标区域为 ContentRegion）
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", "MainView");
        }


    }
}
