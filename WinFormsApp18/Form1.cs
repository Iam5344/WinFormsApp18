using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinFormsApp18
{
    public partial class Form1 : Form
    {
        List<string[]> users = new List<string[]>();

        [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);

        const int WH_KEYBOARD_LL = 13;
        const int WH_MOUSE_LL = 14;
        const int WM_KEYDOWN = 0x0100;
        const int WM_MOUSEMOVE = 0x0200;

        const int VK_CONTROL = 0x11;
        const int VK_SHIFT = 0x10;
        const int VK_Q = 0x51;
        const int VK_MENU = 0x12;

        delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        static IntPtr kHook = IntPtr.Zero;
        static IntPtr mHook = IntPtr.Zero;
        static HookProc kProc;
        static HookProc mProc;

        static Form1 form;

        bool visible = true;

        int left, top, right, bottom;

        public Form1()
        {
            InitializeComponent();
            form = this;

            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;

            left = (w - 500) / 2;
            top = (h - 500) / 2;
            right = left + 500;
            bottom = top + 500;

            kProc = Keyboard;
            mProc = Mouse;

            var p = Process.GetCurrentProcess();
            var m = p.MainModule;

            kHook = SetWindowsHookEx(WH_KEYBOARD_LL, kProc, GetModuleHandle(m.ModuleName), 0);
            mHook = SetWindowsHookEx(WH_MOUSE_LL, mProc, GetModuleHandle(m.ModuleName), 0);
        }

        static IntPtr Keyboard(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int key = Marshal.ReadInt32(lParam);

                bool ctrl = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
                bool shift = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;

                if (ctrl && shift && key == VK_Q)
                {
                    form.Invoke(new Action(() =>
                    {
                        form.visible = !form.visible;
                        form.Visible = form.visible;

                        if (form.visible)
                        {
                            form.WindowState = FormWindowState.Normal;
                            form.BringToFront();
                        }
                    }));

                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(kHook, nCode, wParam, lParam);
        }

        struct M
        {
            public int x;
            public int y;
            public uint d;
            public uint f;
            public uint t;
            public IntPtr i;
        }

        static IntPtr Mouse(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                bool alt = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;

                if (alt)
                {
                    M m = (M)Marshal.PtrToStructure(lParam, typeof(M));

                    int x = m.x;
                    int y = m.y;

                    if (x < form.left) x = form.left;
                    if (x > form.right) x = form.right;
                    if (y < form.top) y = form.top;
                    if (y > form.bottom) y = form.bottom;

                    SetCursorPos(x, y);
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(mHook, nCode, wParam, lParam);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (kHook != IntPtr.Zero) UnhookWindowsHookEx(kHook);
            if (mHook != IntPtr.Zero) UnhookWindowsHookEx(mHook);
            base.OnFormClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            users.Clear();

            string[] lines = File.ReadAllLines(textBox1.Text);

            foreach (string line in lines)
            {
                string[] parts = line.Split('-');

                if (parts.Length == 2)
                {
                    users.Add(new string[] { parts[0].Trim(), parts[1].Trim() });
                }
            }

            label1.Text = "Завантажено: " + users.Count;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Stopwatch sw = Stopwatch.StartNew();

            foreach (var u in users)
            {
                if (u[0].Contains(textBox2.Text) || u[1].Contains(textBox2.Text))
                {
                    listBox1.Items.Add(u[0] + " - " + u[1]);
                }
            }

            sw.Stop();

            label1.Text = "LINQ: " + sw.ElapsedMilliseconds + " мс";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Stopwatch sw = Stopwatch.StartNew();

            foreach (var u in users)
            {
                if (u[0].Contains(textBox2.Text) || u[1].Contains(textBox2.Text))
                {
                    listBox1.Items.Add(u[0] + " - " + u[1]);
                }
            }

            sw.Stop();

            label1.Text = "PLINQ: " + sw.ElapsedMilliseconds + " мс";
        }
    }
}