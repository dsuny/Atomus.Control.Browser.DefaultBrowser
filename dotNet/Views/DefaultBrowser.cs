using Atomus.Attribute;
using Atomus.Control.Browser.Controllers;
using Atomus.Control.Browser.Models;
using Atomus.Diagnostics;
using System;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Atomus.Control.Browser
{
    public partial class DefaultBrowser : Form, IAction
    {
        private IAction loginControl;
        private IAction joinControl;
        private IAction toolbarControl;
        private IAction homeControl;
        private TabControl tabControl;
        private UserControl browserViewer;
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;

#region Init
        public DefaultBrowser()
        {
            string skinName;
            Color color;

            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.IsMdiContainer = false;
            this.ShowIcon = false;
            this.ControlBox = false;
            this.ShowInTaskbar = false;

            skinName = this.GetAttribute("SkinName");

            if (skinName != null)
            {
                Config.Client.SetAttribute("SkinName", skinName);

                color = this.GetAttributeColor(skinName + ".BackColor");
                if (color != null)
                    this.BackColor = color;

                color = this.GetAttributeColor(skinName + ".ForeColor");
                if (color != null)
                    this.ForeColor = color;
            }

            //this.TransparencyKey = Color.Magenta;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;

            this.FormClosing += new FormClosingEventHandler(this.DefaultBrowser_FormClosing);

            this.GetActivationUri();
        }
#endregion

#region Dictionary
#endregion

#region Spread 
#endregion

#region IO 
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            try
            {
                this.beforeActionEventHandler?.Invoke(this, e);

                switch (e.Action)
                {
                    default:
                        throw new AtomusException(this.GetMessage("Common", "00047", "'{0}'은(는) 처리할 수 없는 {1} 입니다.").Message.Translate(e.Action, "Action"));
                        //throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            finally
            {
                this.afterActionEventHandler?.Invoke(this, e);
            }
        }

        private void LoginControl_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private async void LoginControl_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            UserControl userControl;

            try
            {
                switch (e.Action)
                {
                    case "Form.Size":
                        this.Size = (Size)e.Value;
                        break;

                    case "Login.Ok":
                        this.Opacity = 0;
                        this.Controls.Clear();
                        this.loginControl = null;

                        this.ControlBox = true;
                        this.ShowInTaskbar = true;
                        this.FormBorderStyle = FormBorderStyle.Sizable;
                        this.StartPosition = FormStartPosition.CenterScreen;
                        this.TopMost = false;

                        try
                        {
                            this.Icon = await this.GetAttributeWebIcon("Icon");
                            if (this.Icon != null)
                                this.ShowIcon = true;
                        }
                        catch (Exception _Exception)
                        {
                            DiagnosticsTool.MyTrace(_Exception);
                        }

                        this.WindowState = FormWindowState.Maximized;

                        this.SetBrowserViewer();
                        this.SetTabControl();
                        this.SetToolbar();

                        this.SetStatusStrip();

                        this.AddHomeControl();

                        this.Opacity = 1;

                        //this.FormClosing -= new FormClosingEventHandler(this.DefaultBrowser_FormClosing);
                        break;

                    case "Login.Fail":
                        break;

                    case "Login.Cancel":
                        this.Close();
                        break;

                    case "Login.JoinNew":
                        if (this.joinControl == null)
                        {
                            this.joinControl = (IAction)this.CreateInstance("JoinControl");

                            this.joinControl.BeforeActionEventHandler += JoinControl_BeforeActionEventHandler;
                            this.joinControl.AfterActionEventHandler += JoinControl_AfterActionEventHandler;

                            userControl = (UserControl)this.joinControl;
                            userControl.Dock = DockStyle.Fill;

                            this.Controls.Add((UserControl)this.joinControl);
                        }

                        userControl = (UserControl)this.joinControl;
                        userControl.BringToFront();
                        break;

                    default:
                        throw new AtomusException(this.GetMessage("Common", "00047", "'{0}'은(는) 처리할 수 없는 {1} 입니다.").Message.Translate(e.Action, "Action"));
                        //throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void JoinControl_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void JoinControl_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case "Form.Size":
                        this.Size = (Size)e.Value;
                        break;

                    case "Join.Start":
                        break;

                    case "Join.Ok":
                        break;

                    case "Join.Cancel":
                        this.SetLoginControl();
                        break;

                    case "PasswordChange.Start":
                        break;

                    case "PasswordChange.Ok":
                        break;

                    default:
                        throw new AtomusException(this.GetMessage("Common", "00047", "'{0}'은(는) 처리할 수 없는 {1} 입니다.").Message.Translate(e.Action, "Action"));
                        //throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void ToolbarControl_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void ToolbarControl_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            IAction action;
            TabPage tabPage;
            UserControl userControl;
            AtomusControlEventArgs atomusControlEventArgs;

            try
            {
                action = (IAction)this.tabControl.Tag;

                switch (e.Action)
                {
                    case "Close":
                        if (!action.Equals(this.homeControl))
                        {
                            this.toolbarControl.ControlAction(action, "UserToolbarButton.Remove", null);

                            tabPage = this.tabControl.SelectedTab;
                            this.tabControl.DeselectTab(tabPage);
                            userControl = (UserControl)action;

                            if (((IAction)userControl).GetAttribute("AllowCloseAction") != null && ((ICore)userControl).GetAttribute("AllowCloseAction") == "Y")
                                try
                                {
                                    if (!(bool)((IAction)userControl).ControlAction(this, new AtomusControlArgs(e.Action, null)))
                                    {
                                        e.Value = false;
                                        return;
                                    }
                                }
                                catch (Exception exception)
                                {
                                    this.MessageBoxShow(this, exception);
                                    e.Value = false;
                                    return;
                                }

                            this.tabControl.TabPages.Remove(tabPage);
                            this.browserViewer.Controls.Remove(userControl);
                            userControl.Dispose();

                            e.Value = true;
                        }
                        else
                            this.ApplicationExit();

                        break;

                    case "Close all":
                        while (true)
                        {
                            if (this.tabControl.TabPages.Count == 1)
                                break;

                            this.tabControl.SelectedTab = this.tabControl.TabPages[1];

                            atomusControlEventArgs = new AtomusControlEventArgs() { Action = "Close" };
                            this.ToolbarControl_AfterActionEventHandler((IAction)this.tabControl.Tag, atomusControlEventArgs);

                            if (atomusControlEventArgs.Value != null && atomusControlEventArgs.Value is bool && !(bool)atomusControlEventArgs.Value)
                            {
                                e.Value = false;
                                return;
                            }
                        }

                        e.Value = true;
                        break;

                    case "Close all items except this one":
                        TabPage currentTabPage;
                        int _Index;

                        currentTabPage = this.tabControl.SelectedTab;
                        _Index = 1;

                        while (true)
                        {
                            if (this.tabControl.TabPages.Count == _Index)
                                break;

                            this.tabControl.SelectedTab = this.tabControl.TabPages[_Index];

                            if (this.tabControl.SelectedTab.Equals(currentTabPage))
                                _Index += 1;
                            else
                                this.ToolbarControl_AfterActionEventHandler((IAction)this.tabControl.Tag
                                                                            , new AtomusControlEventArgs()
                                                                            {
                                                                                Action = "Close"
                                                                            });
                        }

                        this.tabControl.SelectedTab = currentTabPage;
                        break;

                    default:
                        if (!e.Action.StartsWith("Action."))
                        {
                            if (!sender.Equals(this.toolbarControl))//toolbarControl 아니면?(기본 툴바 버튼이면)
                                action = (IAction)sender;

                            if (sender.GetAttribute("ASSEMBLY_ID") != null)
                            {
                                atomusControlEventArgs = new AtomusControlEventArgs("UserControl.AssemblyVersionCheck", null);
                                this.UserControl_AfterActionEventHandler(action, atomusControlEventArgs);

                                if ((bool)atomusControlEventArgs.Value)
                                {
                                    action.ControlAction(sender, e.Action, e.Value);
                                    return;
                                }
                            }
                            else
                            {
                                action.ControlAction(sender, e.Action, e.Value);
                            }
                        }

                        //this.ControlActionHome(sender, e);
                        break;
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void HomeControl_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void HomeControl_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            object[] objects;
            ICore core;
            string MENU_ID;

            try
            {
                switch (e.Action)
                {
                    case "Menu.OpenControl":
                        objects = (object[])e.Value;//_MENU_ID, _ASSEMBLY_ID, _VisibleOne

                        if ((bool)objects[2])//_VisibleOne
                        {
                            foreach (TabPage _TabPage in this.tabControl.TabPages)
                            {
                                core = (ICore)_TabPage.Tag;

                                MENU_ID = core.GetAttribute("MENU_ID");

                                if (MENU_ID != null)
                                    if (MENU_ID.Equals(objects[0].ToString()))
                                    {
                                        this.tabControl.Tag = _TabPage.Tag;
                                        this.tabControl.SelectedTab = _TabPage;
                                        return;//기존 화면이 있으니 바로 빠져 나감
                                    }
                            }
                        }

                        e.Value = this.OpenControl((decimal)objects[0], (decimal)objects[1], sender, null, true);

                        break;

                    case "Menu.GetControl":
                        objects = (object[])e.Value;//_MENU_ID, _ASSEMBLY_ID, AtomusControlEventArgs, addTabControl

                        e.Value = this.OpenControl((decimal)objects[0], (decimal)objects[1], sender, (AtomusControlEventArgs)objects[2], (bool)objects[3]);

                        break;

                    //case "Menu.Atomus.Control.AtomusManagement.ComposeServiceAssemblies":
                    //    _Core = Factory.CreateInstance(System.IO.File.ReadAllBytes(@"C:\Work\Project\Atomus\개발\Control\AtomusManagement\ComposeServiceAssemblies\bin\Debug\Atomus.Control.AtomusManagement.ComposeServiceAssemblies.V1.0.0.0.dll"), "Atomus.Control.AtomusManagement.ComposeServiceAssemblies", false, false);
                    //    this.OpenControl("ComposeServiceAssemblies", "ComposeServiceAssemblies", (UserControl)_Core);
                    //    break;

                    case "ApplicationExit":
                        this.ApplicationExit();
                        break;

                    //default:
                    //    throw new AtomusException(this.GetMessage("Common", "00047", "'{0}'은(는) 처리할 수 없는 {1} 입니다.").Message.Translate(e.Action, "Action"));
                        //throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void UserControl_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void UserControl_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            object[] objects;
            Service.IResponse response;
            string tmp;

            try
            {
                switch (e.Action)
                {
                    case "UserControl.OpenControl":
                        objects = (object[])e.Value;//_MENU_ID, _ASSEMBLY_ID, sender, AtomusControlArgs

                        e.Value = this.OpenControl((decimal)objects[0], (decimal)objects[1], sender, (AtomusControlEventArgs)objects[2], true);

                        break;

                    case "UserControl.GetControl":
                        objects = (object[])e.Value;//_MENU_ID, _ASSEMBLY_ID, sender, AtomusControlArgs

                        e.Value = this.OpenControl((decimal)objects[0], (decimal)objects[1], sender, (AtomusControlEventArgs)objects[2], false);

                        break;

                    case "UserControl.AssemblyVersionCheck":
                        tmp = this.GetAttribute("ProcedureAssemblyVersionCheck");

                        if (tmp != null && tmp.Trim() != "")
                        {
                            response = this.AssemblyVersionCheck(sender);

                            if (response.Status != Service.Status.OK)
                            {
                                this.MessageBoxShow(this, response.Message);
                                e.Value = false;
                            }
                            else
                                e.Value = true;
                        }
                        else
                            e.Value = true;

                        break;

                    case "UserControl.Status":
                        objects = (object[])e.Value;//StatusBarInfomation1  Text

                        ((StatusStrip)this.Controls.Find("StatusStrip", true)[0]).Items[string.Format("StatusStrip_{0}", objects[0])].Text = (string)objects[1];

                        break;

                        //default:
                        //    throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private async Task<ICore> OpenControlAsync(decimal _MENU_ID, decimal _ASSEMBLY_ID, ICore sender, AtomusControlEventArgs _AtomusControlEventArgs, bool addTabControl)
        {
            Service.IResponse _Result;
            IAction _Core;

            try
            {
                _Result = await this.SearchOpenControlAsync(new DefaultBrowserSearchModel()
                {
                    MENU_ID = _MENU_ID,
                    ASSEMBLY_ID = _ASSEMBLY_ID
                });

                if (_Result.Status == Service.Status.OK)
                {
                    if (_Result.DataSet.Tables.Count == 2)
                        if (_Result.DataSet.Tables[0].Rows.Count == 1)
                        {
                            if (_Result.DataSet.Tables[0].Columns.Contains("FILE_TEXT") && _Result.DataSet.Tables[0].Rows[0]["FILE_TEXT"] != DBNull.Value)
                                _Core = (IAction)Factory.CreateInstance(Convert.FromBase64String((string)_Result.DataSet.Tables[0].Rows[0]["FILE_TEXT"]), _Result.DataSet.Tables[0].Rows[0]["NAMESPACE"].ToString(), false, false);
                            else
                                _Core = (IAction)Factory.CreateInstance((byte[])_Result.DataSet.Tables[0].Rows[0]["FILE"], _Result.DataSet.Tables[0].Rows[0]["NAMESPACE"].ToString(), false, false);

                            _Core.BeforeActionEventHandler += UserControl_BeforeActionEventHandler;
                            _Core.AfterActionEventHandler += UserControl_AfterActionEventHandler;

                            _Core.SetAttribute("MENU_ID", _MENU_ID.ToString());
                            _Core.SetAttribute("ASSEMBLY_ID", _ASSEMBLY_ID.ToString());

                            foreach (DataRow _DataRow in _Result.DataSet.Tables[1].Rows)
                            {
                                _Core.SetAttribute(_DataRow["ATTRIBUTE_NAME"].ToString(), _DataRow["ATTRIBUTE_VALUE"].ToString());
                            }

                            if (addTabControl)
                                this.OpenControl((_Result.DataSet.Tables[0].Rows[0]["NAME"] as string).Translate(), string.Format("{0} {1}", (_Result.DataSet.Tables[0].Rows[0]["DESCRIPTION"] as string).Translate(), _Core.GetType().Assembly.GetName().Version.ToString()), (UserControl)_Core);

                            if (_AtomusControlEventArgs != null)
                                _Core.ControlAction(sender, _AtomusControlEventArgs.Action, _AtomusControlEventArgs.Value);

                            return _Core;
                        }
                }
                else
                {
                    this.MessageBoxShow(this, _Result.Message);
                }

                return null;
            }
            catch (Exception _Exception)
            {
                this.MessageBoxShow(this, _Exception);
                return null;
            }
            finally
            {
            }
        }
        private ICore OpenControl(decimal _MENU_ID, decimal _ASSEMBLY_ID, ICore sender, AtomusControlEventArgs _AtomusControlEventArgs, bool addTabControl)
        {
            Service.IResponse _Result;
            IAction _Core;

            try
            {
                _Result = this.SearchOpenControl(new DefaultBrowserSearchModel()
                {
                    MENU_ID = _MENU_ID,
                    ASSEMBLY_ID = _ASSEMBLY_ID
                });

                if (_Result.Status == Service.Status.OK)
                {
                    if (_Result.DataSet.Tables.Count == 2)
                        if (_Result.DataSet.Tables[0].Rows.Count == 1)
                        {
                            if (_Result.DataSet.Tables[0].Columns.Contains("FILE_TEXT")  && _Result.DataSet.Tables[0].Rows[0]["FILE_TEXT"] != DBNull.Value)
                                _Core = (IAction)Factory.CreateInstance(Convert.FromBase64String((string)_Result.DataSet.Tables[0].Rows[0]["FILE_TEXT"]), _Result.DataSet.Tables[0].Rows[0]["NAMESPACE"].ToString(), false, false);
                            else
                                _Core = (IAction)Factory.CreateInstance((byte[])_Result.DataSet.Tables[0].Rows[0]["FILE"], _Result.DataSet.Tables[0].Rows[0]["NAMESPACE"].ToString(), false, false);

                            _Core.BeforeActionEventHandler += UserControl_BeforeActionEventHandler;
                            _Core.AfterActionEventHandler += UserControl_AfterActionEventHandler;

                            _Core.SetAttribute("MENU_ID", _MENU_ID.ToString());
                            _Core.SetAttribute("ASSEMBLY_ID", _ASSEMBLY_ID.ToString());

                            foreach (DataRow _DataRow in _Result.DataSet.Tables[1].Rows)
                            {
                                _Core.SetAttribute(_DataRow["ATTRIBUTE_NAME"].ToString(), _DataRow["ATTRIBUTE_VALUE"].ToString());
                            }

                            if (addTabControl)
                                this.OpenControl((_Result.DataSet.Tables[0].Rows[0]["NAME"] as string).Translate(), string.Format("{0} {1}", (_Result.DataSet.Tables[0].Rows[0]["DESCRIPTION"] as string).Translate(), _Core.GetType().Assembly.GetName().Version.ToString()), (UserControl)_Core);

                            if (_AtomusControlEventArgs != null)
                                _Core.ControlAction(sender, _AtomusControlEventArgs.Action, _AtomusControlEventArgs.Value);

                            return _Core;
                        }
                }
                else
                {
                    this.MessageBoxShow(this, _Result.Message);
                }

                return null;
            }
            catch (Exception _Exception)
            {
                this.MessageBoxShow(this, _Exception);
                return null;
            }
            finally
            {
            }
        }

        private void OpenControl(string name, string description, UserControl userControl)
        {
            TabPage tabPage;

            try
            {
                userControl.Dock = DockStyle.Fill;

                tabPage = new TabPage
                {
                    BackColor = Color.Transparent,
                    Text = name,
                    ToolTipText = description,
                    Tag = userControl
                };

                this.tabControl.TabPages.Add(tabPage);
                this.tabControl.Tag = userControl;
                this.tabControl.SelectedTab = tabPage;

                //this.TabControl_Selected(this._TabControl, null);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }
#endregion

#region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private void DefaultBrowser_Load(object sender, EventArgs e)
        {
            try
            {
#if DEBUG
                DiagnosticsTool.MyDebug(string.Format("DefaultBrowser_Load(object sender = {0}, EventArgs e = {1})", (sender != null) ? sender.ToString() : "null", (e != null) ? e.ToString() : "null"));
#endif
                this.Size = new Size(0, 0);
                this.Text = Factory.FactoryConfig.GetAttribute("Atomus", "ServiceName");
            }
            //catch (AtomusException _Exception)
            //{
            //    this.MessageBoxShow(this, _Exception);
            //    Application.Exit();
            //}
            //catch (TypeInitializationException _Exception)
            //{
            //    this.MessageBoxShow(this, _Exception);
            //    Application.Exit();
            //}
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
                Application.Exit();
            }

            try
            {
                this.SetLoginControl();
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
                Application.Exit();
            }
        }


        [HistoryComment("권대선", 2017, 10, 9, AuthorAttributeType.Modify, @"
        Tab 키를 입력할때 보이지 않는 TagPage의 컨트롤로 커서가 이동 못하도록 변경 : userControl1.Visible = false
        ")]
        private void TabControl_Selected(object sender, TabControlEventArgs e)
        {
            TabControl tabControl;
            UserControl userControl;
            UserControl userControl1;
            ICore core;

            try
            {
                tabControl = (TabControl)sender;

                this.toolbarControl.ControlAction((ICore)tabControl.Tag, "UserToolbarButton.Remove", null);

                tabControl.Tag = tabControl.SelectedTab.Tag;
                userControl = (UserControl)tabControl.Tag;

                if (!this.browserViewer.Controls.Contains(userControl))
                    this.browserViewer.Controls.Add(userControl);

                core = (ICore)userControl;

                object value;

                value = core.GetAttribute("Action.New");
                this.toolbarControl.ControlAction(core, "Action.New", value ?? "Y");
                value = core.GetAttribute("Action.Search");
                this.toolbarControl.ControlAction(core, "Action.Search", value ?? "Y");
                value = core.GetAttribute("Action.Save");
                this.toolbarControl.ControlAction(core, "Action.Save", value ?? "Y");
                value = core.GetAttribute("Action.Delete");
                this.toolbarControl.ControlAction(core, "Action.Delete", value ?? "Y");
                value = core.GetAttribute("Action.Print");
                this.toolbarControl.ControlAction(core, "Action.Print", value ?? "Y");

                this.toolbarControl.ControlAction(core, "UserToolbarButton.Add", null);//각 화면에서만 사용하는 버튼 활성화

                userControl.BringToFront();

                userControl.Visible = true;

                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    if (!tabPage.Equals(tabControl.SelectedTab))
                    {
                        userControl1 = (UserControl)tabPage.Tag;
                        userControl1.Visible = false;
                    }
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void DefaultBrowser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F4)
            {
                try
                {
                    this.ToolbarControl_AfterActionEventHandler(toolbarControl, new AtomusControlArgs("Close", null));
                    //((IAction)this).ControlAction(_ToolbarControl, ));
                }
                catch (Exception exception)
                {
                    this.MessageBoxShow(this, exception);
                }
            }

            if (e.Control && e.KeyCode == Keys.Tab && ActiveControl != this.tabControl)
            {
                if (this.tabControl.SelectedIndex + 1 == this.tabControl.TabCount)
                    this.tabControl.SelectedIndex = 0;
                else
                    this.tabControl.SelectedIndex += 1;
            }

#if DEBUG
            if (e.Control && e.Shift && e.KeyCode == Keys.D)
            {
                DiagnosticsTool.ShowForm();
            }
#endif

            if (e.Control && e.Shift && e.KeyCode == Keys.T)
            {
                DiagnosticsTool.ShowForm();
            }
        }

        private void DefaultBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.IsApplicationExitIn)
                if (!this.ApplicationExit())
                    e.Cancel = true;
        }

        private void ToolStripMenuItem_Click(Object _sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem;

            toolStripMenuItem = (ToolStripMenuItem)_sender;

            this.ToolbarControl_AfterActionEventHandler((IAction)this.tabControl.Tag
                                                        , new AtomusControlEventArgs()
                                                        {
                                                            Action = toolStripMenuItem.Text
                                                        });
        }
        /// <summary>
        /// 오른쪽 클릭시 탭 변경 하도록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_MouseClick(object sender, MouseEventArgs e)
        {
            TabControl tabControl;

            tabControl = (TabControl)sender;

            if (e.Button == MouseButtons.Right)
                for (int i = 0; i < tabControl.TabCount; ++i)
                    if (tabControl.GetTabRect(i).Contains(e.Location))
                        tabControl.SelectTab(i);

        }
        #endregion

#region ETC
        private void SetLoginControl()
        {
            UserControl userControl;

            try
            {
                if (this.loginControl == null)
                {
                    //this.loginControl = new Login.DefaultLogin();
                    this.loginControl = (IAction)this.CreateInstance("LoginControl", false);

                    this.loginControl.BeforeActionEventHandler += LoginControl_BeforeActionEventHandler;
                    this.loginControl.AfterActionEventHandler += LoginControl_AfterActionEventHandler;

                    userControl = (UserControl)this.loginControl;
                    userControl.Dock = DockStyle.Fill;

                    this.Controls.Add((UserControl)this.loginControl);
                }
                else
                    userControl = (UserControl)this.loginControl;

                userControl.Visible = true;
                userControl.BringToFront();
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void SetBrowserViewer()
        {
            try
            {
                this.browserViewer = new UserControl
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };
                this.Controls.Add(this.browserViewer);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void SetToolbar()
        {
            UserControl userControl;

            try
            {
                this.toolbarControl = (IAction)this.CreateInstance("Toolbar");
                //this.toolbarControl = new Toolbar.DefaultToolbar();
                this.toolbarControl.BeforeActionEventHandler += ToolbarControl_BeforeActionEventHandler;
                this.toolbarControl.AfterActionEventHandler += ToolbarControl_AfterActionEventHandler;

                userControl = (UserControl)this.toolbarControl;
                userControl.Dock = DockStyle.Top;

                this.Controls.Add((UserControl)this.toolbarControl);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void SetTabControl()
        {
            ContextMenuStrip contextMenuStrip;
            ToolStripMenuItem toolStripMenuItem;
            string[] tmps;

            try
            {
                this.tabControl = new TabControl();

                if (this.GetAttribute("TabControlVisibleResponsibilityID") != "")
                {
                    tmps = this.GetAttribute("TabControlVisibleResponsibilityID").Split(',');

                    this.tabControl.Visible = tmps.Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString());
                }

                this.tabControl.DoubleBuffered(true);
                this.tabControl.ShowToolTips = true;
                this.tabControl.BackColor = this.BackColor;
                this.tabControl.TabPages.Clear();
                this.tabControl.Dock = DockStyle.Top;
                this.tabControl.Height = 21;
                this.tabControl.HotTrack = true;
                this.tabControl.Selected += this.TabControl_Selected;
                this.tabControl.MouseDown += TabControl_MouseClick;

                contextMenuStrip = new ContextMenuStrip();

                toolStripMenuItem = new ToolStripMenuItem("Close", null, ToolStripMenuItem_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Close all", null, ToolStripMenuItem_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Close all items except this one", null, ToolStripMenuItem_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                this.tabControl.ContextMenuStrip = contextMenuStrip;

                this.Controls.Add(this.tabControl);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void AddHomeControl()
        {
            UserControl userControl;
            TabPage tabPage;

            try
            {
                //this.homeControl = new Atomus.Control.Home.DefaultHome();
                this.homeControl = (IAction)this.CreateInstance("HomeControl");
                this.homeControl.BeforeActionEventHandler += HomeControl_BeforeActionEventHandler;
                this.homeControl.AfterActionEventHandler += HomeControl_AfterActionEventHandler;

                userControl = (UserControl)this.homeControl;
                userControl.Dock = DockStyle.Fill;

                tabPage = new TabPage
                {
                    BackColor = Color.Transparent,
                    Text = "Home",
                    Tag = this.homeControl
                };

                this.tabControl.TabPages.Add(tabPage);
                this.tabControl.Tag = this.homeControl;
                this.tabControl.SelectedTab = tabPage;

                this.TabControl_Selected(this.tabControl, null);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private bool IsApplicationExitIn = false;
        private bool ApplicationExit()
        {
            AtomusControlEventArgs atomusControlEventArgs;

            try
            {
                this.IsApplicationExitIn = true;
                if (this.MessageBoxShow(this, "Common-00002 {0} 하시겠습니까?^종료", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        if (this.toolbarControl != null)
                        {
                            atomusControlEventArgs = new AtomusControlArgs("Close all", null);
                            this.ToolbarControl_AfterActionEventHandler(this.toolbarControl, atomusControlEventArgs);
                            //((IAction)this).ControlAction(_ToolbarControl, ));

                            if (atomusControlEventArgs.Value != null && atomusControlEventArgs.Value is bool && !(bool)atomusControlEventArgs.Value)
                                return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        this.MessageBoxShow(this, exception);
                    }

                    Application.Exit();
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                this.IsApplicationExitIn = false;
            }
        }

        private void DebugStart()
        {
            if (!DiagnosticsTool.IsStart)
            {
                DiagnosticsTool.Mode = Mode.DebugToTextBox | Mode.DebugToFile;
                DiagnosticsTool.TextBoxBase = new RichTextBox();
                DiagnosticsTool.Start();
            }
        }

        private void TraceStart()
        {
            if (!DiagnosticsTool.IsStart)
            {
                DiagnosticsTool.Mode = Mode.TraceToTextBox | Mode.TraceToFile;
                DiagnosticsTool.TextBoxBase = new RichTextBox();
                DiagnosticsTool.Start();
            }
        }

        private void SetStatusStrip()
        {
            StatusStrip statusStrip;
            string type;
            string kind;
            string[] temps;
            string[] temps1;
            ToolStripStatusLabel toolStripStatusLabel;

            try
            {
                temps = this.GetAttribute("StatusStrip").Split(',');

                if (temps == null || temps.Count() < 1)
                    return;

                statusStrip = new StatusStrip
                {
                    ImageScalingSize = new Size(20, 20),
                    Location = new Point(0, 523),
                    Name = "StatusStrip",
                    Size = new Size(779, 22),
                    SizingGrip = false,
                    TabIndex = 0,
                    BackColor = Color.Transparent,
                    RightToLeft = (RightToLeft)Enum.Parse(typeof(RightToLeft), this.GetAttribute("StatusStrip.RightToLeft"))
                };

                this.Controls.Add(statusStrip);

                if (this.GetAttribute("StatusStripVisibleResponsibilityID") != "")
                {
                    temps1 = this.GetAttribute("StatusStripVisibleResponsibilityID").Split(',');

                    statusStrip.Visible = temps1.Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString());
                }

                foreach (string name in temps)
                {
                    type = this.GetAttribute(string.Format("StatusStrip.{0}.Type", name));

                    switch (type)
                    {
                        case "ToolStripStatusLabel":
                            toolStripStatusLabel = new ToolStripStatusLabel
                            {
                                BackColor = Color.Transparent,
                                Spring = this.GetAttributeBool(string.Format("StatusStrip.{0}.Spring", name)),
                                Name = string.Format("StatusStrip_{0}", name)
                            };

                            statusStrip.Items.Add(toolStripStatusLabel);

                            kind = this.GetAttribute(string.Format("StatusStrip.{0}.Kind", name));

                            switch (kind)
                            {
                                case "Email":
                                    toolStripStatusLabel.Text = (string)Config.Client.GetAttribute("Account.EMAIL");
                                    break;
                                case "NickName":
                                    toolStripStatusLabel.Text = string.Format("{0} ({1})", Config.Client.GetAttribute("Account.NICKNAME"), Config.Client.GetAttribute("Account.USER_ID"));
                                    //toolStripStatusLabel.Text = (string)Config.Client.GetAttribute("Account.NICKNAME");
                                    break;
                                case "Responsibility":
                                    toolStripStatusLabel.Text = (string)Config.Client.GetAttribute("Account.RESPONSIBILITY_NAME");
                                    break;
                                case "Server":
                                    try
                                    {
                                        if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                                            //barStaticItem.Caption = Config.Client.GetAttribute("DeployUriHost").ToString();
                                            toolStripStatusLabel.Text = string.Format("{0}:{1}"
                                                                            , System.Deployment.Application.ApplicationDeployment.CurrentDeployment.ActivationUri.Host
                                                                            , System.Deployment.Application.ApplicationDeployment.CurrentDeployment.ActivationUri.Port);
                                        else
                                            toolStripStatusLabel.Text = "";
                                    }
                                    catch (Exception ex)
                                    {
                                        DiagnosticsTool.MyTrace(ex);
                                    }

                                    break;
                                case "Timer":
                                    DateTime dateTime;
                                    Timer timer;

                                    timer = new Timer() { Interval = 1000 };
                                    timer.Tick += (o, e) => {
                                        dateTime = ((DateTime)Config.Client.GetAttribute("Account.DATETIME"));

                                        if (Config.Client.GetAttribute("Account.DiffNowServer") == null)
                                        {
                                            Config.Client.SetAttribute("Account.DiffNowServer", dateTime - DateTime.Now);
                                        }

                                        dateTime = DateTime.Now.Add((TimeSpan)Config.Client.GetAttribute("Account.DiffNowServer"));

                                        Config.Client.SetAttribute("Account.DATETIME", dateTime);
                                        toolStripStatusLabel.Text = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                    };

                                    timer.Start();

                                    break;

                                default:
                                    toolStripStatusLabel.Text = "";
                                    break;
                            }
                            break;
                            //case "":
                            //    break;
                            //case "":
                            //    break;
                    }


                }
            }
            catch (Exception ex)
            {
                DiagnosticsTool.MyTrace(ex);
            }
        }


        private void GetActivationUri()
        {
            string tmp;
            string[] tmps;
            string[] tmps1;

            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    tmp = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;

                    if (!tmp.IsNullOrEmpty() && tmp.Contains("?"))
                    {
                        tmps = tmp.Substring(tmp.IndexOf('?') + 1).Split('&');

                        foreach (string value in tmps)
                            if (value.Contains("="))
                            {
                                tmps1 = value.Split('=');

                                if (tmps1.Length > 1)
                                    Config.Client.SetAttribute(string.Format("UriParameter.{0}", tmps1[0]), tmps1[1]);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsTool.MyTrace(ex);
            }
        }
        private void GetActivationUri_20200106()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    System.Collections.Specialized.NameValueCollection nameValueTable = new System.Collections.Specialized.NameValueCollection();

                    if (ApplicationDeployment.IsNetworkDeployed)
                        nameValueTable = HttpUtility.ParseQueryString(ApplicationDeployment.CurrentDeployment.ActivationUri.Query);

                    foreach (string key in nameValueTable.AllKeys)
                        Config.Client.SetAttribute(string.Format("UriParameter.{0}", key), nameValueTable[key]);
                }
            }
            catch (Exception ex)
            {
                DiagnosticsTool.MyTrace(ex);
            }
        }
        #endregion
    }
}