using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Plugins.Core;

// ReSharper disable once InconsistentNaming
public class DBICommands
{
    public static readonly ILogger Log = DBI.Logging.Create<DBICommands>();

    internal DBICommands() { }

    Dictionary<KeyCode, List<Command>> Commands { get; } = new();

    public void Register(string name, KeyCode key, Action action) => Register(name, null, key, action);

    public void Register(string name, string description, KeyCode key, Il2CppSystem.Action action)
    {
        Command command = new(name, description, key, action);
        if (!Commands.TryGetValue(key, out List<Command> commands))
        {
            commands = [];
            Commands[key] = commands;
        }

        commands.Add(command);

        Log.LogInformation("Command {Command} has been registered.", command);
    }

    public IReadOnlyCollection<KeyCode> GetRegisteredKeys() => Commands.Keys;
    public IReadOnlyCollection<Command> GetCommands(KeyCode key) => Commands.GetValueOrDefault(key);

    public struct Command
    {
        public Command(string name, string description, KeyCode key, Il2CppSystem.Action action)
        {
            Name = name;
            Description = description;
            Key = key;
            Action = action;
        }

        public string Name { get; }
        public string Description { get; }
        public KeyCode Key { get; }
        public Il2CppSystem.Action Action { get; }

        public override string ToString() => $"{Name} ({Key.ToString()})";
    }
}
