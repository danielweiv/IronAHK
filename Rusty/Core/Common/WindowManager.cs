using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IronAHK.Rusty
{
    partial class Core
    {
        internal abstract class WindowManager
        {
            #region Properties

            int delay = 100;

            public int Delay
            {
                get { return delay; }
                set { delay = value; }
            }

            #endregion

            #region Find

            #region Criteria

            public class SearchCriteria
            {
                public string Title { get; set; }
                public string Text { get; set; }
                public string ExcludeTitle { get; set; }
                public string ExcludeText { get; set; }

                public string ClassName { get; set; }
                public IntPtr ID { get; set; }
                public IntPtr PID { get; set; }
                public string Group { get; set; }

                public static SearchCriteria FromString(string mixed)
                {
                    var criteria = new SearchCriteria();
                    var opts = ParseOptions(mixed);
                    var i = 0;

                    while (i < opts.Length)
                    {
                        var current = opts[i++].ToLowerInvariant();
                        var next = ++i < opts.Length ? opts[i] : string.Empty;

                        switch (current)
                        {
                            case Keyword_ahk_class: criteria.ClassName = next; break;
                            case Keyword_ahk_group: criteria.Group = next; break;

                            case Keyword_ahk_id:
                                {
                                    long x;
                                    if (long.TryParse(next, out x)) criteria.ID = new IntPtr(x);
                                }
                                break;

                            case Keyword_ahk_pid:
                                {
                                    long x;
                                    if (long.TryParse(next, out x)) criteria.PID = new IntPtr(x);
                                }
                                break;

                            default:
                                criteria.Title = next;
                                i--;
                                break;
                        }
                    }

                    return criteria;
                }

                public static SearchCriteria FromString(string title, string text, string excludeTitle, string excludeText)
                {
                    var criteria = FromString(title);
                    criteria.Text = text;
                    criteria.ExcludeTitle = excludeTitle;
                    criteria.ExcludeText = excludeText;
                    return criteria;
                }
            }

            #endregion

            public abstract IntPtr LastFound { get; set; }

            public abstract IntPtr[] AllWindows { get; }

            public abstract IntPtr[] ActiveWindows { get; }

            public abstract IntPtr[] LastActiveWindows { get; }

            public abstract IntPtr[] FindWindow(SearchCriteria criteria);

            public IntPtr[] FindWindow(params string[] criteria)
            {
                var parts = new string[4];

                for (var i = 0; i < parts.Length; i++)
                    parts[i] = i < criteria.Length ? criteria[i] : string.Empty;

                var search = SearchCriteria.FromString(parts[0], parts[1], parts[2], parts[3]);
                return FindWindow(search);
            }

            public WindowManager FindFirstWindow(string title, string text, string excludeTitle, string excludeText)
            {
                var ids = FindWindow(title, text, excludeTitle, excludeText);
                return CreateWindow(ids.Length > 0 ? ids[0] : IntPtr.Zero);
            }

            #endregion

            #region Window

            public IntPtr ID { get; set; }

            public abstract WindowManager CreateWindow(IntPtr id);

            #region Helpers

            public bool IsSpecified
            {
                get { return ID != IntPtr.Zero; }
            }

            public bool Equals(SearchCriteria criteria)
            {
                if (ID != criteria.ID)
                    return false;

                if (PID != criteria.PID)
                    return false;

                var comp = StringComparison.OrdinalIgnoreCase;

                if (!string.IsNullOrEmpty(criteria.ClassName))
                {
                    if (!ClassName.Equals(criteria.ClassName, comp))
                        return false;
                }

                if (!string.IsNullOrEmpty(criteria.Title))
                {
                    if (!TitleCompare(Title, criteria.Title))
                        return false;
                }

                if (!string.IsNullOrEmpty(criteria.Text))
                {
                    if (Text.IndexOf(criteria.Text, comp) == -1)
                        return false;
                }

                if (!string.IsNullOrEmpty(criteria.ExcludeTitle))
                {
                    if (Title.IndexOf(criteria.ExcludeTitle, comp) != -1)
                        return false;
                }

                if (!string.IsNullOrEmpty(criteria.ExcludeText))
                {
                    if (Text.IndexOf(criteria.ExcludeText, comp) != -1)
                        return false;
                }

                return false;
            }

            bool TitleCompare(string a, string b)
            {
                var comp = StringComparison.OrdinalIgnoreCase;

                switch (A_TitleMatchMode.ToLowerInvariant())
                {
                    case "1":
                        return a.StartsWith(b, comp);

                    case "2":
                        return a.IndexOf(b, comp) != -1;

                    case "3":
                        return a.Equals(b, comp);

                    case Keyword_RegEx:
                        return new Regex(b).IsMatch(a);
                }

                return false;
            }

            #endregion

            protected abstract IntPtr PID { get; }

            public abstract bool Active { get; set; }

            public abstract bool Close();

            public abstract bool Exists { get; }

            public abstract string ClassName { get; }

            public abstract Point Location { get; set; }

            public abstract Size Size { get; set; }

            public abstract bool SelectMenuItem(params string[] items);

            public abstract string Title { get; set; }

            public abstract string Text { get; set; }

            public abstract bool Hide();

            public abstract bool Kill();

            public abstract bool AlwaysOnTop { get; set; }

            public abstract bool Bottom { set; }

            public abstract bool Enabled { get; set; }

            public abstract bool Redraw();

            public abstract int Style { get; set; }

            public abstract int ExStyle { get; set; }

            public abstract byte Transparency { get; set; }

            public abstract Color TransparencyColor { get; set; }

            public abstract bool Show();

            public abstract FormWindowState WindowState { get; set; }

            #region Wait

            public bool Wait(int timeout = -1)
            {
                if (timeout != -1)
                    timeout += Environment.TickCount;

                while (!this.Exists)
                {
                    if (timeout != -1 && Environment.TickCount >= timeout)
                        return false;

                    System.Threading.Thread.Sleep(Delay);
                }

                return true;
            }

            public bool WaitActive(int timeout = -1)
            {
                if (timeout != -1)
                    timeout += Environment.TickCount;

                while (!this.Active)
                {
                    if (timeout != -1 && Environment.TickCount >= timeout)
                        return false;

                    System.Threading.Thread.Sleep(Delay);
                }

                return true;
            }

            public bool WaitClose(int timeout = -1)
            {
                if (timeout != -1)
                    timeout += Environment.TickCount;

                while (this.Exists)
                {
                    if (timeout != -1 && Environment.TickCount >= timeout)
                        return false;

                    System.Threading.Thread.Sleep(Delay);
                }

                return true;
            }

            public bool WaitNotActive(int timeout = -1)
            {
                if (timeout != -1)
                    timeout += Environment.TickCount;

                while (this.Active)
                {
                    if (timeout != -1 && Environment.TickCount >= timeout)
                        return false;

                    System.Threading.Thread.Sleep(Delay);
                }

                return true;
            }

            #endregion

            #endregion
        }
    }
}