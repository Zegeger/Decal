///////////////////////////////////////////////////////////////////////////////
//File: ViewSystemSelector.cs
//
//Description: Contains the Zegeger.Decal.VVS.ViewSystemSelector class,
//  which is used to determine whether the Virindi View Service is enabled.
//  As with all the VVS wrappers, the VVS_REFERENCED compilation symbol must be
//  defined for the VVS code to be compiled. Otherwise, only Decal views are used.
//
//References required:
//  VirindiViewService (if VVS_REFERENCED is defined)
//  Decal.Adapter
//  Decal.Interop.Core
//
//This file is Copyright (c) 2009 VirindiPlugins
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Decal.Adapter.Wrappers;

#if METAVIEW_PUBLIC_NS
namespace Zegeger.Decal.VVS
#else
namespace Zegeger.Decal.VVS
#endif
{
    public  static class ViewSystemSelector
    {
        public enum eViewSystem
        {
            DecalInject,
            VirindiViewService,
        }


        ///////////////////////////////System presence detection///////////////////////////////

        public static bool IsPresent(PluginHost pHost, eViewSystem VSystem)
        {
            switch (VSystem)
            {
                case eViewSystem.DecalInject:
                    return true;
                case eViewSystem.VirindiViewService:
                    return VirindiViewsPresent(pHost);
                default:
                    return false;
            }
        }
        static bool VirindiViewsPresent(PluginHost pHost)
        {
#if VVS_REFERENCED
            System.Reflection.Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            foreach (System.Reflection.Assembly a in asms)
            {
                AssemblyName nmm = a.GetName();
                if ((nmm.Name == "VirindiViewService") && (nmm.Version >= new System.Version("1.0.0.18")))
                {
                    try
                    {
                        return Curtain_VVS_Running();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
#else
            return false;
#endif
        }
        public static bool VirindiViewsPresent(PluginHost pHost, Version minver)
        {
#if VVS_REFERENCED
            System.Reflection.Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            foreach (System.Reflection.Assembly a in asms)
            {
                AssemblyName nm = a.GetName();
                if ((nm.Name == "VirindiViewService") && (nm.Version >= minver))
                {
                    try
                    {
                        return Curtain_VVS_Running();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
#else
            return false;
#endif
        }
		
#if VVS_REFERENCED
        static bool Curtain_VVS_Running()
        {
            return VirindiViewService.Service.Running;
        }
#endif

        ///////////////////////////////CreateViewResource///////////////////////////////

        internal static IView CreateViewResource(PluginHost pHost, string pXMLResource)
        {
#if VVS_REFERENCED
            if (IsPresent(pHost, eViewSystem.VirindiViewService))
                return CreateViewResource(pHost, pXMLResource, eViewSystem.VirindiViewService);
            else
#endif
                return CreateViewResource(pHost, pXMLResource, eViewSystem.DecalInject);
        }
        internal static IView CreateViewResource(PluginHost pHost, string pXML, eViewSystem VSystem)
        {
            if (!IsPresent(pHost, VSystem)) return null;
            switch (VSystem)
            {
                case eViewSystem.DecalInject:
                    return CreateDecalViewResource(pHost, pXML);
                case eViewSystem.VirindiViewService:
#if VVS_REFERENCED
                    return CreateMyHudViewResource(pHost, pXML);
#else
                    break;
#endif
            }
            return null;
        }
        static IView CreateDecalViewResource(PluginHost pHost, string pXML)
        {
            IView ret = new DecalControls.View();
            ret.InitializeRawXML(pHost, pXML);
            return ret;
        }

#if VVS_REFERENCED
        static IView CreateMyHudViewResource(PluginHost pHost, string pXML)
        {
            IView ret = new VirindiViewServiceHudControls.View();
            ret.InitializeRawXML(pHost, pXML);
            return ret;
        }
#endif


        ///////////////////////////////CreateViewXML///////////////////////////////

        internal static IView CreateViewXML(PluginHost pHost, string pXML)
        {
#if VVS_REFERENCED
            if (IsPresent(pHost, eViewSystem.VirindiViewService))
                return CreateViewXML(pHost, pXML, eViewSystem.VirindiViewService);
            else
#endif
                return CreateViewXML(pHost, pXML, eViewSystem.DecalInject);
        }

        internal static IView CreateViewXML(PluginHost pHost, string pXML, eViewSystem VSystem)
        {
            if (!IsPresent(pHost, VSystem)) return null;
            switch (VSystem)
            {
                case eViewSystem.DecalInject:
                    return CreateDecalViewXML(pHost, pXML);
                case eViewSystem.VirindiViewService:
#if VVS_REFERENCED
                    return CreateMyHudViewXML(pHost, pXML);
#else
                    break;
#endif
            }
            return null;
        }
        static IView CreateDecalViewXML(PluginHost pHost, string pXML)
        {
            IView ret = new DecalControls.View();
            ret.InitializeRawXML(pHost, pXML);
            return ret;
        }

#if VVS_REFERENCED
        static IView CreateMyHudViewXML(PluginHost pHost, string pXML)
        {
            IView ret = new VirindiViewServiceHudControls.View();
            ret.InitializeRawXML(pHost, pXML);
            return ret;
        }
#endif


        ///////////////////////////////HasChatOpen///////////////////////////////

        public static bool AnySystemHasChatOpen(PluginHost pHost)
        {
            if (IsPresent(pHost, eViewSystem.VirindiViewService))
                if (HasChatOpen_VirindiViews()) return true;
            if (pHost.Actions.ChatState) return true;
            return false;
        }
        
        static bool HasChatOpen_VirindiViews()
        {
#if VVS_REFERENCED
            if (VirindiViewService.HudView.FocusControl != null)
            {
                if (VirindiViewService.HudView.FocusControl.GetType() == typeof(VirindiViewService.Controls.HudTextBox))
                    return true;
            }
            return false;
#else
            return false;
#endif
        }
        
    }
}
