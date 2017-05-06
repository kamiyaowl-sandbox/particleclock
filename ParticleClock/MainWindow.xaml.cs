using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ParticleClock {

    static class CanvasExtension {
        /// <summary>
        /// 場所を設定
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetPosition(this UIElement ui, double x, double y) {
            Canvas.SetLeft(ui, x);
            Canvas.SetTop(ui, y);
        }
        public static IEnumerable<int> RandomSequence() {
            var r = new Random();
            while (true) {
                yield return r.Next();
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src) =>
            src.Zip(RandomSequence(), (d, i) => new { Index = i, Data = d })
               .OrderBy(x => x.Index)
               .Select(x => x.Data);

    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private DispatcherTimer particleTimer;
        private Dictionary<char, bool[][]> particleFonts = new Dictionary<char, bool[][]>();
        public MainWindow() {
            InitializeComponent();
        }
        #region Particle Control
        private const int particleSize = 20;

        private double CanvasWidth => drawCanvas.ActualWidth;
        private double Canvasheight => drawCanvas.ActualHeight;
        private IEnumerable<Ellipse> allParticles => drawCanvas.Children.Cast<Ellipse>();
        private int particleCount => drawCanvas.Children.Count;

        private void generateParticles(int n, double size, Action<Ellipse> f = null) {
            var r = new Random();
            for (int i = 0; i < n; ++i) {
                var e = new Ellipse() {
                    StrokeThickness = 1,
                    Width = size,
                    Height = size,
                };
                e.StrokeThickness = 1;
                var fillColor = Color.FromRgb((byte)r.Next(0x0, 0x100), (byte)r.Next(0x0, 0x100), (byte)r.Next(0x0, 0x100));
                var strokeColor = Color.FromRgb((byte)(fillColor.R / 4), (byte)(fillColor.G / 4), (byte)(fillColor.B / 4));
                e.Fill = new SolidColorBrush(fillColor);
                e.Stroke = new SolidColorBrush(strokeColor);
                if (f != null) f(e);
                this.drawCanvas.Children.Add(e);
            }
        }
        private void clearPaticles() {
            drawCanvas.Children.Clear();
        }


        #endregion

        #region UI Event
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            initializeParticleFont();

            particleTimer = new DispatcherTimer() {
                Interval = TimeSpan.FromMilliseconds(50),
                IsEnabled = true,
            };
            particleTimer.Tick += ParticleTimer_Tick;
            particleTimer.Start();
        }


        private void addParticles_Click(object sender, RoutedEventArgs e) {
            var r = new Random();
            generateParticles(100, particleSize, x => {
                x.SetPosition(r.NextDouble() * CanvasWidth, r.NextDouble() * Canvasheight);
            });
        }
        private void clearParticles_Click(object sender, RoutedEventArgs e) {
            clearPaticles();
        }
        private void sortParticles_Click(object sender, RoutedEventArgs e) {
            const int widthN = 100;
            int heightN = particleCount / widthN;
            int lastN = particleCount % widthN;

            var diffX = CanvasWidth / (double)widthN;
            var diffY = Canvasheight / (double)(heightN + (lastN > 0 ? 1 : 0));

            for (int j = 0; j < heightN; ++j) {
                var targets = allParticles.Skip(j * widthN).Take(widthN).ToArray();
                for (int i = 0; i < widthN; ++i) {
                    targets[i].SetPosition(i * diffX, j * diffY);
                }
            }
            if (lastN > 0) {
                var lasts = allParticles.Skip(heightN * widthN).ToArray();
                for (int i = 0; i < widthN; ++i) {
                    lasts[i].SetPosition(i * diffX, (heightN + 1) * diffY);
                }
            }
        }

        private void randomMoveParticles_Click(object sender, RoutedEventArgs e) {
            var r = new Random();
            foreach (var p in allParticles) {
                p.SetPosition(r.NextDouble() * CanvasWidth, r.NextDouble() * Canvasheight);
            }
        }
        private void circleMoveParticles_Click(object sender, RoutedEventArgs e) {
            var centerX = CanvasWidth / 2;
            var centerY = Canvasheight / 2;
            var r = Math.Min(CanvasWidth, Canvasheight) / 2.0;

            var arr = allParticles.ToArray();
            for (int i = 0; i < particleCount; ++i) {
                var ratio = i / (double)particleCount;
                var x = r * Math.Cos(2 * Math.PI * ratio) + centerX;
                var y = r * Math.Sin(2 * Math.PI * ratio) + centerY;

                arr[i].SetPosition(x, y);
            }

        }

        #endregion

        #region Particle Clock
        private DateTime lastUpdateDate = DateTime.Now;

        private void initializeParticleFont() {
            //TODO:ベクタフォントからロードする
            particleFonts['0'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['1'] = new bool[][] {
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, true, true, false, false, false, false},
                new bool[]{ false, true, false, true, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, true, true, true, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['2'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, false, false},
                new bool[]{ false, true, false, false, false, false, false, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['3'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['4'] = new bool[][] {
                new bool[]{ false, false, false, false, false, true, false, false},
                new bool[]{ false, false, false, false, true, true, false, false},
                new bool[]{ false, false, false, true, true, true, false, false},
                new bool[]{ false, false, true, false, false, true, false, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, true, false, false},
                new bool[]{ false, false, false, false, false, true, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['5'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, false, false},
                new bool[]{ false, true, false, false, false, false, false, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['6'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, false, false},
                new bool[]{ false, true, false, false, false, false, false, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['7'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['8'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts['9'] = new bool[][] {
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, false, false, false, false, false, true, false},
                new bool[]{ false, true, true, true, true, true, true, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts[':'] = new bool[][] {
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, false, true, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
            particleFonts[' '] = new bool[][] {
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
                new bool[]{ false, false, false, false, false, false, false, false},
            };
        }

        private void ParticleTimer_Tick(object sender, EventArgs e) {
            var fifo = new Queue<Ellipse>(randomFifoEnable.IsChecked.Value ? allParticles.Shuffle() : allParticles);

            //パーティクルクロック
            if (DateTime.Now - lastUpdateDate > TimeSpan.FromSeconds(1)) {
                var str = DateTime.Now.ToLongTimeString();
                //8==フォントサイズ8x8
                const int FONT_WIDTH = 8;
                const int FONT_HEIGHT = 8;
                var sx = CanvasWidth / 2 - (((str.Length + 1) / 2) * FONT_WIDTH * particleSize);
                var sy = Canvasheight / 2 - (FONT_HEIGHT / 2 * particleSize);

                for (int index = 0; index < str.Length; ++index) {
                    if (particleFonts.ContainsKey(str[index])) {
                        var pattern = particleFonts[str[index]];
                        for (int j = 0; j < FONT_HEIGHT; ++j) {
                            for (int i = 0; i < FONT_WIDTH; ++i) {
                                if (!pattern[j][i]) continue;
                                var x = sx + ((index * FONT_WIDTH + i) * particleSize);
                                var y = sy + (j * particleSize);
                                if (fifo.Count > 0) {
                                    fifo.Dequeue().SetPosition(x, y);
                                }
                            }
                        }
                    }
                }
                //今回はパーティクルを使い切る
                var centerX = CanvasWidth / 2;
                var centerY = Canvasheight / 2;
                var r = str.Length / 2 * FONT_WIDTH * particleSize * 1.1;
                var counts = fifo.Count;

                for (int i = 0; i < counts; ++i) {
                    var ratio = i / (double)counts;
                    var x = r * Math.Cos(2 * Math.PI * ratio) + centerX;
                    var y = r * Math.Sin(2 * Math.PI * ratio) + centerY;

                    fifo.Dequeue().SetPosition(x, y);
                }

                lastUpdateDate = DateTime.Now;
            }
            //マウスとか
            const int mousecircleR = 100;
            const int mouseCircleN = 50;
            if (isMouseLeftButtondown && fifo.Count > mouseCircleN) {
                //汚いのでラッチ下げる
                isMouseLeftButtondown = false;

                for (int i = 0; i < mouseCircleN; ++i) {
                    var c = fifo.Dequeue();
                    var ratio = i / (double)mouseCircleN;
                    var x = mousecircleR * Math.Cos(2 * Math.PI * ratio) + mouseLastPosition.X;
                    var y = mousecircleR * Math.Sin(2 * Math.PI * ratio) + mouseLastPosition.Y;

                    c.SetPosition(x, y);
                }
            }
            if (isMouseRightButtondown && fifo.Count > 1) {
                var c = fifo.Dequeue();
                c.SetPosition(mouseLastPosition.X, mouseLastPosition.Y);
            }
        }


        #endregion


        #region Mouse Events
        Point mouseLastPosition = new Point();
        bool isMouseLeftButtondown = false;
        bool isMouseRightButtondown = false;

        private void drawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            isMouseLeftButtondown = true;
            mouseLastPosition = e.GetPosition(drawCanvas);
        }

        private void drawCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            isMouseLeftButtondown = false;
            mouseLastPosition = e.GetPosition(drawCanvas);
        }

        private void drawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            isMouseRightButtondown = true;
            mouseLastPosition = e.GetPosition(drawCanvas);
        }

        private void drawCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            isMouseRightButtondown = false;
            mouseLastPosition = e.GetPosition(drawCanvas);
        }

        private void drawCanvas_MouseEnter(object sender, MouseEventArgs e) {
            mouseLastPosition = e.GetPosition(drawCanvas);
        }

        private void drawCanvas_MouseMove(object sender, MouseEventArgs e) {
            mouseLastPosition = e.GetPosition(drawCanvas);
        }

        private void drawCanvas_MouseLeave(object sender, MouseEventArgs e) {
            mouseLastPosition = e.GetPosition(drawCanvas);
            isMouseLeftButtondown = false;
            isMouseRightButtondown = false;
        }
        #endregion

    }
}
