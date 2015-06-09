using Decal.Adapter;
using Decal.Adapter.Wrappers;
//using Decal.Interop.Inject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Zegeger.Decal.Plugins.ZChatSystem.Diagnostics;


namespace Zegeger.Decal.Plugins.ZChatSystem
{
    internal delegate void dListChange(string list, string plugin, string dir);

    internal static class MainView
    {
        #region Auto-generated view code
        static MyClasses.MetaViewWrappers.IView View;
        static MyClasses.MetaViewWrappers.IList Message_List;
        static MyClasses.MetaViewWrappers.IList Color_List;
        static MyClasses.MetaViewWrappers.IList Window_List;
        static MyClasses.MetaViewWrappers.INotebook Notebook1;

        private const int ICON_MOVEUP = 0x60028FC;
        private const int ICON_MOVEDOWN = 0x60028FD;

        private const int LIST_CHECK = 0;
        private const int LIST_NAME = 1;
        private const int LIST_DOWN = 2;
        private const int LIST_UP = 3;
        private const int LIST_GUID = 4;

        internal static event dListChange ListChange;

        internal static void ViewInit()
        {
            //Create view here
            foreach (string s in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                TraceLogger.Write(s);
            }
            TraceLogger.Write("Enter");
            View = MyClasses.MetaViewWrappers.ViewSystemSelector.CreateViewResource(PluginCore.MyHost, "Zegeger.Decal.Plugins.ZChatSystem.ViewXML.mainView.xml");
            Message_List = (MyClasses.MetaViewWrappers.IList)View["Message_List"];
            Color_List = (MyClasses.MetaViewWrappers.IList)View["Color_List"];
            Window_List = (MyClasses.MetaViewWrappers.IList)View["Window_List"];
            Notebook1 = (MyClasses.MetaViewWrappers.INotebook)View["Notebook1"];
            InitContinued();
            TraceLogger.Write("Exit");
        }

        internal static void ViewDestroy()
        {
            TraceLogger.Write("Enter");
            Message_List = null;
            Color_List = null;
            Window_List = null;
            Notebook1 = null;
            View.Dispose();
            TraceLogger.Write("Exit");
        }
        #endregion Auto-generated view code

        private static void InitContinued()
        {
            TraceLogger.Write("Enter");
            Message_List.Click += new MyClasses.MetaViewWrappers.dClickedList(Message_List_Click);
            Color_List.Click += new MyClasses.MetaViewWrappers.dClickedList(Color_List_Click);
            Window_List.Click += new MyClasses.MetaViewWrappers.dClickedList(Window_List_Click);
            TraceLogger.Write("Exit");
        }

        static void Window_List_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter - row: " + row + ", col: " + col);
                switch (col)
                {
                    case LIST_DOWN:
                        TraceLogger.Write("Move Down");
                        ListChange("window", Window_List[row][LIST_NAME][0].ToString(), "down");
                        break;
                    case LIST_UP:
                        TraceLogger.Write("Move Up");
                        ListChange("window", Window_List[row][LIST_NAME][0].ToString(), "up");
                        break;
                    case LIST_CHECK:
                        if ((bool)Window_List[row][LIST_CHECK][0] == true)
                        {
                            TraceLogger.Write("Enable");
                            ListChange("window", Window_List[row][LIST_NAME][0].ToString(), "enable");
                        }
                        else
                        {
                            TraceLogger.Write("Disable");
                            ListChange("window", Window_List[row][LIST_NAME][0].ToString(), "disable");
                        }
                        break;
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        static void Color_List_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter - row: " + row + ", col: " + col);
                switch (col)
                {
                    case LIST_DOWN:
                        TraceLogger.Write("Move Down");
                        ListChange("color", Color_List[row][LIST_NAME][0].ToString(), "down");
                        break;
                    case LIST_UP:
                        TraceLogger.Write("Move Up");
                        ListChange("color", Color_List[row][LIST_NAME][0].ToString(), "up");
                        break;
                    case LIST_CHECK:
                        if ((bool)Color_List[row][LIST_CHECK][0] == true)
                        {
                            TraceLogger.Write("Enable");
                            ListChange("color", Color_List[row][LIST_NAME][0].ToString(), "enable");
                        }
                        else
                        {
                            TraceLogger.Write("Disable");
                            ListChange("color", Color_List[row][LIST_NAME][0].ToString(), "disable");
                        }
                        break;
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        static void Message_List_Click(object sender, int row, int col)
        {
            try
            {
                TraceLogger.Write("Enter - row: " + row + ", col: " + col);
                switch (col)
                {
                    case LIST_DOWN:
                        TraceLogger.Write("Move Down");
                        ListChange("message", Message_List[row][LIST_NAME][0].ToString(), "down");
                        break;
                    case LIST_UP:
                        TraceLogger.Write("Move Up");
                        ListChange("message", Message_List[row][LIST_NAME][0].ToString(), "up");
                        break;
                    case LIST_CHECK:
                        if ((bool)Message_List[row][LIST_CHECK][0] == true)
                        {
                            TraceLogger.Write("Enable");
                            ListChange("message", Message_List[row][LIST_NAME][0].ToString(), "enable");
                        }
                        else
                        {
                            TraceLogger.Write("Disable");
                            ListChange("message", Message_List[row][LIST_NAME][0].ToString(), "disable");
                        }
                        break;
                }
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void Message_List_Add(string PluginName, bool enabled)
        {
            try
            {
                TraceLogger.Write("Enter - Plugin Name: " + PluginName + ", Enabled: " + enabled);
                MyClasses.MetaViewWrappers.IListRow row = Message_List.AddRow();
                row[LIST_CHECK][0] = enabled;
                row[LIST_NAME][0] = PluginName;
                row[LIST_DOWN][1] = ICON_MOVEDOWN;
                row[LIST_UP][1] = ICON_MOVEUP;
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void Color_List_Add(string PluginName, bool enabled)
        {
            try
            {
                TraceLogger.Write("Enter - Plugin Name: " + PluginName + ", Enabled: " + enabled);
                MyClasses.MetaViewWrappers.IListRow row = Color_List.AddRow();
                row[LIST_CHECK][0] = enabled;
                row[LIST_NAME][0] = PluginName;
                row[LIST_DOWN][1] = ICON_MOVEDOWN;
                row[LIST_UP][1] = ICON_MOVEUP;
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void Window_List_Add(string PluginName, bool enabled)
        {
            try
            {
                TraceLogger.Write("Enter - Plugin Name: " + PluginName + ", Enabled: " + enabled);
                MyClasses.MetaViewWrappers.IListRow row = Window_List.AddRow();
                row[LIST_CHECK][0] = enabled;
                row[LIST_NAME][0] = PluginName;
                row[LIST_DOWN][1] = ICON_MOVEDOWN;
                row[LIST_UP][1] = ICON_MOVEUP;
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void Message_List_Clear()
        {
            try
            {
                TraceLogger.Write("Enter");
                Message_List.Clear();
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void Color_List_Clear()
        {
            try
            {
                TraceLogger.Write("Enter");
                Color_List.Clear();
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }

        internal static void Window_List_Clear()
        {
            try
            {
                TraceLogger.Write("Enter");
                Window_List.Clear();
                TraceLogger.Write("Exit");
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
        }
    }
}