using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Zegeger.Decal.VVS;

namespace Zegeger.Decal.Plugins.AgentU
{
    internal enum ComponentState
    {
        Init,
        Startingup,
        Running,
        ShuttingDown,
        ShutDown,
        Unknown
    }

    internal delegate void ComponentStateChangeEvent(object sender, ComponentStateChangeEventArgs e);

    internal class ComponentStateChangeEventArgs
    {
        private ComponentState previousState;
        internal ComponentState PreviousState
        {
            get { return previousState; }
            private set { previousState = value; }
        }

        private ComponentState newState;
        internal ComponentState NewState
        {
            get { return newState; }
            private set { newState = value; }
        }

        internal ComponentStateChangeEventArgs(ComponentState previousState, ComponentState newState)
        {
            this.previousState = previousState;
            this.newState = newState;
        }
    }

    internal abstract class Component
    {
        internal event ComponentStateChangeEvent ComponentStateChange;

        private string componentName;
        public string ComponentName
        {
            get { return componentName; }
            protected set { componentName = value; }
        }

        private ComponentState state;
        public ComponentState State
        {
            get { return state; }
            protected set
            {
                var args = new ComponentStateChangeEventArgs(state, value);
                state = value;
                if (ComponentStateChange != null)
                    ComponentStateChange(this, args);
            }
        }

        private CoreManager core;
        protected CoreManager Core
        {
            get { return core; }
        }

        private static IView view;
        protected static IView View
        {
            get { return Component.view; }
        }

        internal Component(CoreManager core, IView view)
        {
            State = ComponentState.Init;
            this.core = core;
            Component.view = view;
        }

        private bool critical;
        public bool Critical
        {
            get { return critical; }
            protected set { critical = value; }
        }

        internal abstract void PostPluginInit();
        internal abstract void PostLogin();
        internal abstract void Shutdown();
    }
}
