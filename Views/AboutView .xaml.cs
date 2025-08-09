using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NewCal.Views
{
    public partial class AboutView : UserControl
    {
        private readonly Random _rand = new Random();

        public AboutView()
        {
            InitializeComponent();
        }

        private void Celebration_Click(object sender, RoutedEventArgs e)
        {
            LaunchConfetti(40);
        }

        // 点击“返回上一页”：先放彩带，再调用 ViewModel 的 GoBackCommand（如果存在）
        private async void BackWithConfetti_Click(object sender, RoutedEventArgs e)
        {
            //LaunchConfetti(60);

            // 等待动画结束：约 2 秒（和下方动画时长对应）
            await Task.Delay(1);

            // 调用 ViewModel 的 GoBackCommand（保持 MVVM）
            if (DataContext is not null)
            {
                var prop = DataContext.GetType().GetProperty("GoBackCommand");
                if (prop != null)
                {
                    if (prop.GetValue(DataContext) is System.Windows.Input.ICommand cmd && cmd.CanExecute(null))
                    {
                        cmd.Execute(null);
                        return;
                    }
                }

                var method = DataContext.GetType().GetMethod("GoBack");
                method?.Invoke(DataContext, null);
            }
        }

        private void LaunchConfetti(int count)
        {
            if (ActualWidth <= 0 || ActualHeight <= 0)
            {
                // 如果视图尚未测量好，延迟一点重试
                Dispatcher.BeginInvoke(new Action(() => LaunchConfetti(count)), System.Windows.Threading.DispatcherPriority.Loaded);
                return;
            }

            double width = ActualWidth;
            double startY = (ActualHeight * 0.15); // 从视图上方一点位置发射（相对 Border 的位置）
            double areaLeft = (ActualWidth - ConfettiCanvas.ActualWidth) / 2; // not critical

            for (int i = 0; i < count; i++)
            {
                // 随机颜色与形状（矩形或椭圆）
                var color = Color.FromRgb((byte)_rand.Next(40, 240), (byte)_rand.Next(40, 240), (byte)_rand.Next(40, 240));
                Shape piece;
                if (_rand.NextDouble() > 0.6)
                {
                    piece = new Ellipse() { Width = _rand.Next(6, 14), Height = _rand.Next(8, 20) };
                }
                else
                {
                    piece = new Rectangle() { Width = _rand.Next(6, 18), Height = _rand.Next(8, 20), RadiusX = 2, RadiusY = 2 };
                }
                piece.Fill = new SolidColorBrush(color);
                piece.RenderTransformOrigin = new Point(0.5, 0.5);

                // 准备变换：平移 + 旋转
                var tg = new TransformGroup();
                var rotate = new RotateTransform(0);
                var translate = new TranslateTransform(0, 0);
                tg.Children.Add(rotate);
                tg.Children.Add(translate);
                piece.RenderTransform = tg;

                // 初始随机 X（在卡片中间发射范围）
                double startX = (ActualWidth / 2) + (_rand.NextDouble() * 300 - 150);
                Canvas.SetLeft(piece, startX);
                Canvas.SetTop(piece, startY);

                ConfettiCanvas.Children.Add(piece);

                // 动画参数
                double durationSec = 1.6 + _rand.NextDouble() * 0.8; // 约 1.6 - 2.4s
                double endY = startY + (ActualHeight * (0.6 + _rand.NextDouble() * 0.4)); // 落点 Y
                double endX = startX + (_rand.NextDouble() * (_rand.Next(0, 2) == 0 ? -1 : 1) * (80 + _rand.NextDouble() * 220));
                double rotateBy = (_rand.Next(0, 2) == 0 ? -1 : 1) * (180 + _rand.NextDouble() * 720);

                // 垂直动画（Canvas.Top）
                var topAnim = new DoubleAnimation
                {
                    From = startY,
                    To = endY,
                    Duration = TimeSpan.FromSeconds(durationSec),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                Storyboard.SetTarget(topAnim, piece);
                Storyboard.SetTargetProperty(topAnim, new PropertyPath("(Canvas.Top)"));

                // 水平动画（Canvas.Left）
                var leftAnim = new DoubleAnimation
                {
                    From = startX,
                    To = endX,
                    Duration = TimeSpan.FromSeconds(durationSec),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(leftAnim, piece);
                Storyboard.SetTargetProperty(leftAnim, new PropertyPath("(Canvas.Left)"));

                // 旋转动画
                var rotAnim = new DoubleAnimation
                {
                    From = 0,
                    To = rotateBy,
                    Duration = TimeSpan.FromSeconds(durationSec),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                Storyboard.SetTarget(rotAnim, piece);
                Storyboard.SetTargetProperty(rotAnim, new PropertyPath("RenderTransform.Children[0].Angle"));

                // 透明度淡出
                var fadeAnim = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.6),
                    BeginTime = TimeSpan.FromSeconds(durationSec - 0.6)
                };
                Storyboard.SetTarget(fadeAnim, piece);
                Storyboard.SetTargetProperty(fadeAnim, new PropertyPath("Opacity"));

                // 把动画绑到同一个 Storyboard
                var sb = new Storyboard();
                sb.Children.Add(topAnim);
                sb.Children.Add(leftAnim);
                sb.Children.Add(rotAnim);
                sb.Children.Add(fadeAnim);

                // 动画结束后移除元素，防内存泄漏
                sb.Completed += (s, e) =>
                {
                    ConfettiCanvas.Children.Remove(piece);
                };

                // 开始动画（稍微随机延迟，会更自然）
                sb.BeginTime = TimeSpan.FromMilliseconds(_rand.Next(0, 200));
                sb.Begin();
            }
        }
    }
}
