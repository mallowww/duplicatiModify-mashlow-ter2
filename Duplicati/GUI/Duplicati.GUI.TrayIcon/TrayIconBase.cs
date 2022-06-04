
using System;
using Duplicati.Server.Serialization;
using Duplicati.Server.Serialization.Interface;
using System.Collections.Generic;

namespace Duplicati.GUI.TrayIcon
{
    public enum MenuIcons
    {
        None,
        Status,
        Quit,
        Pause,
        Resume,
    }
    
    public enum TrayIcons
    {
        Idle,
        Paused,
        Running,
        IdleWarning,
        IdleError,
        PausedError,
        RunningError
    }

    public enum WindowIcons
    {
        Regular,
        LogWindow
    }

    public enum NotificationType
    {
        Information,
        Warning,
        Error
    }
    
    public interface IMenuItem
    {
        void SetDefault(bool isDefault);
        void SetIcon(MenuIcons icon);
        void SetText(string text);
    }
    
    public abstract class TrayIconBase : IDisposable
    {           
        protected IMenuItem m_pauseMenu;
        protected bool m_stateIsPaused;
        protected Action m_onSingleClick;
        protected Action m_onDoubleClick;
        protected Action m_onNotificationClick;
        
        public virtual void Init(string[] args)
        {
            SetMenu(BuildMenu());
            RegisterStatusUpdateCallback();
            RegisterNotificationCallback();
            OnStatusUpdated(Program.Connection.Status);
            m_onDoubleClick = ShowStatusWindow;
            m_onNotificationClick = ShowStatusWindow;
            Run(args);
        }
        
        protected abstract void Run(string[] args);

        public void InvokeExit()
        {
            UpdateUIState(() => { this.Exit(); });
        }
        
        protected virtual void UpdateUIState(Action action)
        {
            action();
        }
        
        protected abstract IMenuItem CreateMenuItem(string text, MenuIcons icon, Action callback, IList<IMenuItem> subitems);
        
        protected abstract void Exit();

        protected abstract void SetIcon(TrayIcons icon);
        
        protected abstract void SetMenu(IEnumerable<IMenuItem> items);

        protected abstract void NotifyUser(string title, string message, NotificationType type);

        protected virtual void RegisterStatusUpdateCallback()
        {
            Program.Connection.OnStatusUpdated += OnStatusUpdated;
        }

        protected virtual void RegisterNotificationCallback()
        {
            Program.Connection.OnNotification += OnNotification;
        }

        protected void OnNotification(INotification notification)
        {
            var type = NotificationType.Information;
            switch (notification.Type)
            {
                case Server.Serialization.NotificationType.Information:
                    type = NotificationType.Information;
                    break;
                case Server.Serialization.NotificationType.Warning:
                    type = NotificationType.Warning;
                    break;
                case Server.Serialization.NotificationType.Error:
                    type = NotificationType.Error;
                    break;
            }

            UpdateUIState(() => {
                NotifyUser(notification.Title, notification.Message, type);
            });
        }

        public virtual IBrowserWindow ShowUrlInWindow(string url)
        {
            //Fallback is to just show the window in a browser
            Duplicati.Library.Utility.UrlUtility.OpenURL(url, Program.BrowserCommand);

            return null;
        }

        protected IEnumerable<IMenuItem> BuildMenu() 
        {
            var tmp = CreateMenuItem("Open", MenuIcons.Status, OnStatusClicked, null);
            tmp.SetDefault(true);
            return new IMenuItem[] {
                tmp,
                m_pauseMenu = CreateMenuItem("Pause", MenuIcons.Pause, OnPauseClicked, null ),
                CreateMenuItem("Quit", MenuIcons.Quit, OnQuitClicked, null),
            };
        }
        
        protected void ShowStatusWindow()
        {
            var window = ShowUrlInWindow(Program.Connection.StatusWindowURL);
            if (window != null)
            {
                window.SetIcon(WindowIcons.Regular);
                //window.SetTitle("Duplicati status");
                window.SetTitle("Arak Protection");
            }
        }

        protected void OnStatusClicked()
        {
            ShowStatusWindow();
        }

        protected void OnQuitClicked()
        {
            Exit();
        }

        protected void OnPauseClicked()
        {
            if (m_stateIsPaused)
                Program.Connection.Resume();
            else
                Program.Connection.Pause();
        }
        
        protected void OnStatusUpdated(IServerStatus status)
        {
            this.UpdateUIState(() => {
                switch(status.SuggestedStatusIcon)
                {
                    case SuggestedStatusIcon.Active:
                        this.SetIcon(TrayIcons.Running);
                        break;
                    case SuggestedStatusIcon.ActivePaused:
                        this.SetIcon(TrayIcons.Paused);
                        break;
                    case SuggestedStatusIcon.ReadyError:
                        this.SetIcon(TrayIcons.IdleError);
                        break;
                    case SuggestedStatusIcon.ReadyWarning:
                        this.SetIcon(TrayIcons.IdleWarning);
                        break;
                    case SuggestedStatusIcon.Paused:
                        this.SetIcon(TrayIcons.Paused);
                        break;
                    case SuggestedStatusIcon.Ready:
                    default:    
                        this.SetIcon(TrayIcons.Idle);
                        break;
                    
                }
    
                if (status.ProgramState == LiveControlState.Running)
                {
                    m_pauseMenu.SetIcon(MenuIcons.Pause);
                    m_pauseMenu.SetText("Pause");
                    m_stateIsPaused = false;
                }
                else
                {
                    m_pauseMenu.SetIcon(MenuIcons.Resume);
                    m_pauseMenu.SetText("Resume");
                    m_stateIsPaused = true;
                }
            });
        }

        #region IDisposable implementation
        public abstract void Dispose();
        #endregion
    }
}

