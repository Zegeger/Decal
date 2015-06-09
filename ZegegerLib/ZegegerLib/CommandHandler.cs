using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Zegeger.Diagnostics;

namespace Zegeger.Decal.Chat
{
    public delegate void CommandIssuedCallback(CommandIssuedCallbackArgs e);
    public delegate void WriteToChatCallback(string text);

    public class CommandIssuedCallbackArgs
    {
        public string Command { get; private set; }
        public string Args { get; private set; }

        internal CommandIssuedCallbackArgs(string command, string args)
        {
            Command = command;
            Args = args;
        }
    }

    public enum CommandVisability
    {
        Public,
        Debug,
        Private
    }

    public static class CommandHandler
    {
        private static string _commandPrefix;
        private static WriteToChatCallback _writeToChat;
        private static CoreManager _core;
        private static List<CommandClass> _classes;
        private static List<string> _headers;
        internal static object _lock = new object();

        public static void StartUp(CoreManager core, string commandprefix, IEnumerable<string> headers, WriteToChatCallback chatWriteFunc)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                _core = core;
                _commandPrefix = commandprefix;
                _writeToChat = chatWriteFunc;
                _classes = new List<CommandClass>();
                _headers = new List<string>(headers);
                _core.CommandLineText += new EventHandler<ChatParserInterceptEventArgs>(_core_CommandLineText);
                CommandClass helpClass = CreateCommandClass("");
                CommandGroup helpGroup = helpClass.CreateCommandGroup("Displays this help text");
                helpGroup.AddCommand("help", helpCommandHandler);
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public static void Shutdown()
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Noise);
                lock (_lock)
                {
                    _core.CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(_core_CommandLineText);
                    _classes.Clear();
                    _headers.Clear();
                    _writeToChat = null;
                    _core = null;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Noise);
        }

        public static CommandClass CreateCommandClass(string Name)
        {
            CommandClass cc = new CommandClass(Name);
            AddCommandClass(cc);
            return cc;
        }

        private static void AddCommandClass(CommandClass cmdclass)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                lock (_lock)
                {
                    if (!_classes.Contains(cmdclass))
                    {
                        _classes.Add(cmdclass);
                        TraceLogger.Write("Command Class Added " + cmdclass.ClassName, TraceLevel.Verbose);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private static void WriteToChat(string text)
        {
            _writeToChat(text);
        }

        private static void helpCommandHandler(CommandIssuedCallbackArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                lock (_lock)
                {
                    foreach (string header in _headers)
                    {
                        WriteToChat(header);
                    }
                    foreach (CommandClass cc in _classes)
                    {
                        if (!String.IsNullOrEmpty(cc.ClassName))
                        {
                            WriteToChat(cc.ClassName + ":");
                        }
                        foreach (CommandGroup cg in cc.Groups)
                        {
                            string line = "";
                            foreach (Command c in cg.Commands)
                            {
                                line += c.CommandVerb;
                                foreach (string arg in c.Args)
                                {
                                    line += " <" + arg + ">";
                                }
                                line += ", ";
                            }
                            line = line.Trim().TrimEnd(',');
                            if (!String.IsNullOrEmpty(cg.GroupDescription))
                            {
                                line += " - " + cg.GroupDescription;
                            }
                            WriteToChat(line);
                        }
                        WriteToChat("");
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private static void _core_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                string text = e.Text.Trim();
                TraceLogger.Write("Command line " + text, TraceLevel.Info);
                if (text.StartsWith("/") || text.StartsWith("@"))
                {
                    text = text.Substring(1).Trim();
                    if (text.StartsWith(_commandPrefix))
                    {
                        text = text.Substring(_commandPrefix.Length).Trim();
                        TraceLogger.Write("Command text " + text, TraceLevel.Verbose);
                        lock (_lock)
                        {
                            foreach (CommandClass cc in _classes)
                            {
                                foreach (CommandGroup cg in cc.Groups)
                                {
                                    foreach (Command c in cg.Commands)
                                    {
                                        if (text.StartsWith(c.CommandVerb))
                                        {
                                            TraceLogger.Write("Command verb " + c.CommandVerb, TraceLevel.Info);
                                            string args = text.Substring(c.CommandVerb.Length).Trim();
                                            TraceLogger.Write("Command args " + args, TraceLevel.Verbose);
                                            c.Callback(new CommandIssuedCallbackArgs(c.CommandVerb, args));
                                            e.Eat = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    public class CommandClass
    {
        public string ClassName { get; private set; }
        internal List<CommandGroup> Groups;

        private CommandClass()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            ClassName = "";
            Groups = new List<CommandGroup>();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal CommandClass(string name)
            : this()
        {
            TraceLogger.Write("Enter name " + name, TraceLevel.Verbose);
            ClassName = name;
            TraceLogger.Write("Enter", TraceLevel.Verbose);
        }

        public CommandGroup CreateCommandGroup(string Description)
        {
            CommandGroup cg = new CommandGroup(Description);
            AddGroup(cg);
            return cg;
        }

        private void AddGroup(CommandGroup group)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                lock (CommandHandler._lock)
                {
                    if (!Groups.Contains(group))
                    {
                        Groups.Add(group);
                        TraceLogger.Write("Added Command Group " + group.GroupDescription, TraceLevel.Verbose);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    public class CommandGroup
    {
        public string GroupDescription { get; private set; }
        internal List<Command> Commands;

        private CommandGroup()
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            GroupDescription = "";
            Commands = new List<Command>();
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        internal CommandGroup(string description)
            : this()
        {
            TraceLogger.Write("Enter, description " + description, TraceLevel.Verbose);
            GroupDescription = description;
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void AddCommand(string verb, CommandIssuedCallback callback)
        {
            TraceLogger.Write("Enter verb " + verb, TraceLevel.Verbose);
            AddCommand(new Command(verb, callback));
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void AddCommand(string verb, List<string> args, CommandIssuedCallback callback)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            AddCommand(new Command(verb, args, callback));
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void AddCommand(string verb, CommandVisability visability, CommandIssuedCallback callback)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            AddCommand(new Command(verb, visability, callback));
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        public void AddCommand(string verb, List<string> args, CommandVisability visability, CommandIssuedCallback callback)
        {
            TraceLogger.Write("Enter", TraceLevel.Verbose);
            AddCommand(new Command(verb, args, visability, callback));
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }

        private void AddCommand(Command command)
        {
            try
            {
                TraceLogger.Write("Enter", TraceLevel.Verbose);
                lock (CommandHandler._lock)
                {
                    if (!Commands.Contains(command))
                    {
                        Commands.Add(command);
                        TraceLogger.Write("Added command " + command.CommandVerb, TraceLevel.Verbose);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Write(ex);
            }
            TraceLogger.Write("Exit", TraceLevel.Verbose);
        }
    }

    class Command
    {
        public string CommandVerb { get; private set; }
        internal CommandIssuedCallback Callback;
        internal CommandVisability Visability;
        internal List<string> Args;

        private Command()
        {
            Args = new List<string>();
            Visability = CommandVisability.Public;
        }

        public Command(string verb, CommandIssuedCallback callback)
            : this()
        {
            CommandVerb = verb;
            Callback = callback;
        }

        public Command(string verb, List<string> args, CommandIssuedCallback callback)
            : this(verb, callback)
        {
            Args = args;
        }

        public Command(string verb, CommandVisability visability, CommandIssuedCallback callback)
            : this(verb, callback)
        {
            Visability = visability;
        }

        public Command(string verb, List<string> args, CommandVisability visability, CommandIssuedCallback callback)
            : this(verb, callback)
        {
            Args = args;
            Visability = visability;
        }
    }
}
