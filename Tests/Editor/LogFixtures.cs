using System.Text;
using EnhancedConsole.Editor.LogManagement;
using UnityEngine;

namespace EnhancedConsole.Editor.Tests
{
    /// <summary>Realistic logs: a short message and a stack trace of about 2 KB, like a log from game code.</summary>
    internal static class LogFixtures
    {
        private const int STACK_FRAMES = 20;
        private static readonly string[] SYSTEMS =
        {
            "PlayerController", "InventoryModel", "ShopScreen", "AudioMixer", "NetworkClient"
        };

        public static CapturedLog Create(int index)
        {
            string system = SYSTEMS[index % SYSTEMS.Length];
            return new CapturedLog
            {
                message = $"[{system}] step {index} finished with value {index * 7 % 1000}",
                stackTrace = StackTrace(system, index),
                type = index % 10 == 0 ? LogType.Warning : LogType.Log,
                frameCount = index
            };
        }

        public static LogEntryManager CreateManager(out LogFilterManager filters, int count)
        {
            filters = new LogFilterManager(new SectionedLogOrganizer());
            LogEntryManager manager = new(filters);
            for (int i = 0; i < count; i++)
                manager.AddLog(Create(i));

            return manager;
        }

        private static string StackTrace(string system, int index)
        {
            StringBuilder builder = new();
            for (int frame = 0; frame < STACK_FRAMES; frame++)
            {
                builder.Append("Game.").Append(system).Append(".Method").Append(frame);
                builder.Append(" (System.Int32 value) (at Assets/Client/Game/").Append(system).Append(".cs:");
                builder.Append(index % 300 + frame).Append(")\n");
            }

            return builder.ToString();
        }
    }
}
