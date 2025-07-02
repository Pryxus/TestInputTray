using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

namespace TestInputTray
{
    internal class Program
    {
        private static NotifyIcon _notifyIcon;
        private static ContextMenuStrip _cms;
        private static ApplicationContext _context;

        private const int TimerIntervall = 30000;
        private static readonly System.Timers.Timer Timer = new System.Timers.Timer();
        private static bool _runs = true;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(ExecutionState esFlags);

        [STAThread]
        private static void Main()
        {
            SystemEvents.SessionSwitch += HandleSessionSwitch;

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = Properties.Resources.runs;
            _notifyIcon.Text = @"TestInput";

            _cms = new ContextMenuStrip();

            _cms.Items.Add(new ToolStripMenuItem("Status ändern", null, ChangeState_Click));
            _cms.Items.Add(new ToolStripSeparator());
            _cms.Items.Add(new ToolStripMenuItem("Beenden", null, Quit_Click, "Quit"));

            SetThreadExecutionState(ExecutionState.EsContinuous | ExecutionState.EsDisplayRequired | ExecutionState.EsAwaymodeRequired);

            Timer.Interval = TimerIntervall;
            Timer.Elapsed += OnTimerElapsed;
            Timer.AutoReset = false;
            Timer.Start();

            _notifyIcon.ContextMenuStrip = _cms;
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += ChangeState_Click;

            _context = new ApplicationContext();
            Application.Run(_context);

            _notifyIcon.Visible = false;
        }

        private static void HandleSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.ConsoleDisconnect:
                    Timer.Stop();
                    break;
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.ConsoleConnect:
                    Timer.Start();
                    break;
            }
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var s = new InputBuilder();
            s.MoveMouseBy(1, 1);
            s.MoveMouseBy(-1, -1);

            Timer.Interval = TimerIntervall;
            Timer.Start();
        }

        private static void ChangeState_Click(object sender, EventArgs e)
        {
            if (_runs)
            {
                SetThreadExecutionState(ExecutionState.EsContinuous);
                _notifyIcon.Icon = Properties.Resources.stops;
                Timer.Stop();
            }
            else
            {
                SetThreadExecutionState(ExecutionState.EsContinuous | ExecutionState.EsDisplayRequired | ExecutionState.EsAwaymodeRequired);
                _notifyIcon.Icon = Properties.Resources.runs;
                Timer.Start();
            }
            _runs = !_runs;
        }

        private static void Quit_Click(object sender, EventArgs e)
        {
            SetThreadExecutionState(ExecutionState.EsContinuous);
            _context.ExitThread();
        }

    }

    [Flags]
    public enum ExecutionState : uint
    {
        EsAwaymodeRequired = 64,
        EsContinuous = 2147483648,
        EsDisplayRequired = 2,
    }
}
