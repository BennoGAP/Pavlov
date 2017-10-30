﻿using System;
using System.Drawing;
using System.Media;
using System.Security.Principal;
using System.Windows.Forms;

namespace Pavlov
{
    public partial class Pavlov : Form
    {
        private MemRead memRead;
        private DateTime dt;
        private int foodPrev = -1;
        private int changeRate = -1;
        private bool alarmPlayed = false;
        private bool repeatAlarm = true;
        private SoundPlayer player;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();


        [STAThread]
        static void Main()
        {
            try
            {
                if ((new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Application.Run(new Pavlov());
                }
                else
                {
                    MessageBox.Show("Pavlov must be ran as Administrator.", "Privilege Error");
                    Application.Exit();
                }
            }
            catch
            {
                Application.Exit();
            }
        }

        public Pavlov()
        {
            InitializeComponent();
            BackColor = Color.Pink;
            TransparencyKey = Color.Pink;

            try
            {
                player = new SoundPlayer(@"feedme.wav");
            }
            catch 
            {
                player = null;
            }

            memRead = new MemRead();
            memRead.GetProcess();

            var Timer = new Timer()
            {
                Interval = (500)
            };
            Timer.Tick += new EventHandler(UpdateWindow);
            Timer.Start();
        }

        private void UpdateWindow(object sender, EventArgs e)
        {
            var gi = memRead.GetValues();
            if (gi != null)
            {
                if (gi.PetId != uint.MaxValue)
                {
                    hungerLabel.Text = $"Hunger:{GetHungerString(gi.Food)} ({gi.Food}/100)";
                    intimacyLabel.Text = $"Intimacy:{GetIntimacyString(gi.Intimacy)} ({gi.Intimacy}/1000)";
                    petNameLabel.Text = gi.PetName;
                    bestFeedLabel.Text = GetEstimatedTimeTillHungry(gi.Food);
                }
                else
                {
                    hungerLabel.Text = string.Empty;
                    intimacyLabel.Text = string.Empty;
                    bestFeedLabel.Text = string.Empty;
                    changeRate = -1;
                    foodPrev = -1;
                    if (alarmPlayed) StopAlarm();
                    petNameLabel.Text = "No Pet Active.";
                }
            }
            else
            {
                hungerLabel.Text = string.Empty;
                intimacyLabel.Text = string.Empty;
                bestFeedLabel.Text = string.Empty;
                if (alarmPlayed) StopAlarm();
                petNameLabel.Text = "Unable to find data.";
            }
        }

        private string GetEstimatedTimeTillHungry(int food)
        {
            if (foodPrev == -1) foodPrev = food;
            var change = foodPrev - food;
            if (food > 0 && changeRate == -1 && foodPrev > food) changeRate = change;
            foodPrev = food;
            if (food <= 25)
            {
                if (!alarmPlayed) PlayAlarm();
                return "Feed me!";
            }
            if (alarmPlayed) StopAlarm();
            if (changeRate != -1)
            {
                if (change != 0)
                {
                    var x = (int)Math.Ceiling((food - 25m) / changeRate);
                    dt = DateTime.Now.AddMinutes(x);
                }
                var ts = dt - DateTime.Now;
                return $"Feed in about {ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";
            }
            return "Calculating...";
        }

        private void StopAlarm()
        {
            alarmPlayed = false;
            if (player != null && repeatAlarm)
            {
                player.Stop();
            }
        }

        private void PlayAlarm()
        {
            alarmPlayed = true;
            if (player != null) {
                if (repeatAlarm)
                {
                    player.PlayLooping();
                }
                else
                {
                    player.Play();
                }
            } 
        }

        private string GetHungerString(int food)
        {
            if (food >= 91) return "Stuffed";
            if (food >= 76) return "Satisfied";
            if (food >= 26) return "Neutral";
            if (food >= 11) return "Hungry";
            return "Very Hungry";
        }

        private string GetIntimacyString(int intimacy)
        {
            if (intimacy >= 910) return "Loyal";
            if (intimacy >= 750) return "Cordial";
            if (intimacy >= 250) return "Neutral";
            if (intimacy >= 100) return "Shy";
            return "Awkward";
        }

        #region form controls
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void NextClientBtn_Click(object sender, EventArgs e)
        {
            dt = DateTime.Now;
            changeRate = -1;
            foodPrev = -1;
            if (alarmPlayed) StopAlarm();
            memRead.GetProcess();
        }
        #endregion
    }
}
