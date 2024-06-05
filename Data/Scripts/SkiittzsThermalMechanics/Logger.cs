﻿//logger sometimes crashes and needs to be pulled out.  keeping here temporarily as a reference point, im going to have to write my own.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Sandbox.ModAPI;
//using VRage;
//using VRage.Game;
//using VRage.Game.ModAPI;
//using VRage.Utils;

//namespace SkiittzsThermalMechanics.Data.Scripts.SkiittzsThermalMechanics
//{
//    [VRage.Game.Components.MySessionComponentDescriptor(VRage.Game.Components.MyUpdateOrder.NoUpdate)]
//    class LoggerSession : VRage.Game.Components.MySessionComponentBase
//    {
//        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
//        {
//            base.Init(sessionComponent);
//            //Logger.Instance.Init();
//        }

//        protected override void UnloadData()
//        {
//            base.UnloadData();
//            //Logger.Instance.Close();
//        }
//    }

//    // This is a singleton class for logging
//    // This should be generic, so it can be included without modification in other mods.
//    public class Logger
//    {
//        System.IO.TextWriter m_logger = null;
//        static private Logger m_instance = null;
//        StringBuilder m_cache = new StringBuilder(60);
//        string m_filename = "debug";            // Default filename if not specified
//        int m_indent = 0;
//        bool m_init = false;
//        bool m_loggedSession = false;
//        string m_modName = typeof(Logger).ToString();

//        private Logger()
//        {
//            Active = false;
//            Enabled = false;
//            Debug = false;
//        }

//        public override string ToString()
//        {
//            return this.GetType().FullName;
//        }

//        static public Logger Instance
//        {
//            get
//            {
//                if (m_instance == null)
//                    m_instance = new Logger();
//                return m_instance;
//            }
//        }

//        /// <summary>
//        /// Toggles whether to log messages (exceptions are always logged).
//        /// </summary>
//        public bool Active { get; set; }

//        /// <summary>
//        /// Toggles whether to log exceptions even if not Active. This is useful during startup.
//        /// </summary>
//        public bool Enabled { get; set; }

//        /// <summary>
//        /// Enable debug logging (admin only)
//        /// </summary>
//        public bool Debug { get; set; }

//        public void Init(string filename = null, bool force = false)
//        {
//            if (!string.IsNullOrEmpty(filename))
//                m_modName = filename;

//            if (!m_init || !string.IsNullOrEmpty(filename) || force)
//            {
//                m_init = true;

//                if (!string.IsNullOrEmpty(filename))
//                    Filename = filename;

//                LogMessage("Starting new session: " + filename);
//                LogSession();
//            }
//        }

//        public int IndentLevel
//        {
//            get { return m_indent; }
//            set
//            {
//                if (value < 0)
//                    value = 0;
//                m_indent = value;
//            }
//        }

//        public string Filename
//        {
//            get { return m_filename; }
//            set { m_filename = value; }
//        }

//        public void LogSession()
//        {
//            if (!m_loggedSession && Sandbox.ModAPI.MyAPIGateway.Session != null)
//            {
//                LogMessage(string.Format("IsServer: {0}", Sandbox.ModAPI.MyAPIGateway.Session.IsServer));
//                LogMessage(string.Format("CreativeMode: {0}", Sandbox.ModAPI.MyAPIGateway.Session.CreativeMode));
//                LogMessage(string.Format("OnlineMode: {0}", Sandbox.ModAPI.MyAPIGateway.Session.OnlineMode));
//                LogMessage(string.Format("IsDedicated: {0}", Sandbox.ModAPI.MyAPIGateway.Utilities?.IsDedicated));
//                m_loggedSession = true;
//            }
//        }

//        #region Mulithreaded logging
//        public void LogDebugOnGameThread(string message)
//        {
//            if (Debug)
//                Sandbox.ModAPI.MyAPIGateway.Utilities.InvokeOnGameThread(delegate { Instance.LogDebug(message); });
//        }

//        public void LogMessageOnGameThread(string message)
//        {
//            Sandbox.ModAPI.MyAPIGateway.Utilities.InvokeOnGameThread(delegate { Instance.LogMessage(message); });
//        }

//        public void LogExceptionOnGameThread(Exception ex, bool showNotice = true)
//        {
//            Sandbox.ModAPI.MyAPIGateway.Utilities.InvokeOnGameThread(delegate { Instance.LogException(ex, showNotice); });
//        }
//        #endregion

//        public void LogDebug(string message, IMyTerminalBlock block)
//        {
//            if (block == null)
//            {
//                LogDebug($"{message}-failed, block is null");
//                return;
//            }

//            if (block.CubeGrid == null)
//            {
//                LogDebug($"NullGrid.{block.Name}[{block.BlockDefinition.SubtypeId}]:{message}");
//            }
//            var blockName = string.IsNullOrEmpty(block.CustomName) ? block.Name : block.CustomName;
//            LogDebug($"{block.CubeGrid.CustomName}.{blockName}[{block.BlockDefinition.SubtypeId}]({block.EntityId}):{message}");
//        }
//        public void LogDebug(string message)
//        {
//            if (!(Active && Debug))
//                return;

//            if ((Sandbox.ModAPI.MyAPIGateway.Multiplayer != null && Sandbox.ModAPI.MyAPIGateway.Multiplayer.IsServer) ||
//                (Sandbox.ModAPI.MyAPIGateway.Session != null && Sandbox.ModAPI.MyAPIGateway.Session.Player != null && Sandbox.ModAPI.MyAPIGateway.Session.Player.PromoteLevel >= MyPromoteLevel.Admin))
//                LogMessage(message, severity: MyLogSeverity.Debug);
//        }

//        public void LogAssert(bool trueExpression, string message)
//        {
//            if (!Active)
//                return;

//            if (!trueExpression)
//            {
//                var assertmsg = new StringBuilder(message.Length + 30);
//                assertmsg.Append("ASSERT FAILURE: ");
//                assertmsg.Append(message);
//                LogMessage(assertmsg.ToString(), severity: MyLogSeverity.Warning);
//            }
//        }

//        public delegate void LoggerCallback(string param1, int time = 2000);

//        public void LogException(Exception ex, bool showNotice = true)
//        {
//            if (!Enabled)
//                return;

//            if (showNotice && Sandbox.ModAPI.MyAPIGateway.Utilities != null && Sandbox.ModAPI.MyAPIGateway.Session != null)
//            {
//                if (Sandbox.ModAPI.MyAPIGateway.Session.Player == null)
//                    Sandbox.ModAPI.MyAPIGateway.Utilities.SendMessage(Filename + ": AN ERROR OCCURED! Please report to server admin! Admin: look for " + Filename + ".log.");
//                else
//                    Sandbox.ModAPI.MyAPIGateway.Utilities.ShowMessage(Filename, "AN ERROR OCCURED! Please report and send " + Filename + ".log in the mod storage directory!");
//            }

//            // Make sure we ALWAYS log an exception
//            var previousActive = Active;
//            Active = true;
//            Instance.LogMessage(string.Format("Exception: {0}\r\n{1}", ex.Message, ex.StackTrace), severity: MyLogSeverity.Error);

//            if (ex.InnerException != null)
//            {
//                Instance.LogMessage("Inner Exception Information:", severity: MyLogSeverity.Error);
//                Instance.LogException(ex.InnerException, showNotice);
//            }
//            Active = previousActive;
//        }

//        public void LogMessage(string message, LoggerCallback callback = null, int time = 2000, MyLogSeverity severity = MyLogSeverity.Info)
//        {
//            if (!Active)
//                return;

//            m_cache.Append(DateTime.Now.ToString("[HH:mm:ss.fff] "));

//            for (int x = 0; x < IndentLevel; x++)
//                m_cache.Append("  ");

//            m_cache.AppendFormat("{0}{1}", message, (m_logger != null ? m_//Logger.NewLine : "\r\n"));

//            if (callback != null)
//                callback(message, time);          // Callback to pass message to another logger (ie. ShowNotification)

//            MyLog.Default.Log(severity, m_modName + ": " + message);

//            if (m_init)
//            {
//                if (m_logger == null)
//                {
//                    try
//                    {
//                        m_logger = Sandbox.ModAPI.MyAPIGateway.Utilities.WriteFileInWorldStorage(Filename + ".log", typeof(Logger));
//                    }
//                    catch { return; }
//                }

//                //Sandbox.ModAPI.MyAPIGateway.Utilities.ShowNotification("writing: " + message);
//                m_//Logger.Write(m_cache);
//                m_//Logger.Flush();
//                m_cache.Clear();
//            }
//        }

//        public void Close()
//        {
//            if (!m_init)
//                return;

//            if (m_logger == null)
//                return;

//            if (m_cache.Length > 0)
//                m_//Logger.Write(m_cache);

//            m_cache.Clear();
//            IndentLevel = 0;
//            LogMessage("Ending session");
//            m_//Logger.Flush();
//            m_//Logger.Close();
//            m_logger = null;
//            m_init = false;
//        }
//    }

//    public static class PlayerExtensions
//    {
//        // This should only be called on the client where it will have an effect, so we can cache it for now
//        // TODO: Refresh this cache on a promotion event
//        static Dictionary<ulong, bool> _cachedResult = new Dictionary<ulong, bool>();


//        /// <summary>
//        /// Determines if the player is an Administrator of the active game session.
//        /// </summary>
//        /// <param name="player"></param>
//        /// <returns>True if is specified player is an Administrator in the active game.</returns>
//        public static bool IsAdminOld(this IMyPlayer player)
//        {
//            // Offline mode. You are the only player.
//            if (Sandbox.ModAPI.MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE)
//            {
//                return true;
//            }

//            // Hosted game, and the player is hosting the server.
//            if (player.IsHost())
//            {
//                return true;
//            }

//            if (!_cachedResult.ContainsKey(player.SteamUserId))
//            {
//                // determine if client is admin of Dedicated server.
//                var clients = Sandbox.ModAPI.MyAPIGateway.Session.GetCheckpoint("null").Clients;
//                if (clients != null)
//                {
//                    var client = clients.FirstOrDefault(c => c.SteamId == player.SteamUserId && c.IsAdmin);
//                    _cachedResult[player.SteamUserId] = (client != null);
//                    return _cachedResult[player.SteamUserId];
//                    // If user is not in the list, automatically assume they are not an Admin.
//                }

//                // clients is null when it's not a dedicated server.
//                // Otherwise Treat everyone as Normal Player.
//                _cachedResult[player.SteamUserId] = false;
//                return false;
//            }
//            else
//            {
//                ////Logger.Instance.LogMessage("Using cached value");
//                if (//Logger.Instance.Debug)
//                    Sandbox.ModAPI.MyAPIGateway.Utilities.ShowNotification("Used cached admin check.", 100);
//                return _cachedResult[player.SteamUserId];
//            }
//        }

//        public static bool IsHost(this IMyPlayer player)
//        {
//            return Sandbox.ModAPI.MyAPIGateway.Multiplayer.IsServerPlayer(player.Client);
//        }
//    }

//    public static class Profiler
//    {
//        public static bool Enabled = true;
//        private static string prefix = //Logger.Instance.Filename + "_";

//        public static void Begin(string name)
//        {
//            if (Enabled)
//                MySimpleProfiler.Begin(prefix + name);
//        }

//        public static void End(string name)
//        {
//            if (Enabled)
//                MySimpleProfiler.End(prefix + name);
//        }
//    }
//}
