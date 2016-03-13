using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    public class Logger
    {
        const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss.ffff";

        public enum MessageType
        {
            Info,
            Warning,
            Error
        }

        protected string MakeJSONKeyValueString(KeyValuePair<string, string> kv)
        {
            return '"' + kv.Key + '"' + ":" + '"' + kv.Value + '"';
        }


        protected void writeLogLine(string message, MessageType type, KeyValuePair<string, string>[] payload)
        {
            List<KeyValuePair<string, string>> logLineKVPieces = new List<KeyValuePair<string, string>>();

            logLineKVPieces.Add(new KeyValuePair<string, string>("ts", DateTime.UtcNow.ToString(Logger.DATETIME_FORMAT)));
            logLineKVPieces.Add(new KeyValuePair<string, string>("type", type.ToString().ToLower()));
            logLineKVPieces.Add(new KeyValuePair<string, string>("msg", message));
            logLineKVPieces.InsertRange(logLineKVPieces.Count, payload);

            List<string> longLineStrings = new List<string>();
            foreach (KeyValuePair<string, string> kv in logLineKVPieces)
            {
                longLineStrings.Add(MakeJSONKeyValueString(kv));
            }


            string logLine = string.Join(",", longLineStrings.ToArray());

            System.Console.WriteLine("{" + logLine + "}");
        }

        public void logInfo(string message)
        {
            logInfo(message, new KeyValuePair<string, string>[] { });
        }

        public void logInfo(string message, KeyValuePair<string, string>[] payload)
        {
            writeLogLine(message, MessageType.Info, payload);
        }

        public void logError(string message)
        {
            logError(message, new KeyValuePair<string, string>[] { });
        }

        public void logError(string message, KeyValuePair<string, string>[] payload)
        {
            writeLogLine(message, MessageType.Error, payload);
        }
    }
}
