using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PROJECT;
namespace COMMANDS
{
    public class CMD_DatabaseExtension_Project : CMD_DatabaseExtension
    {
        new public static void Extend(CommandDatabase database)
        {
            //variable assignment
            database.AddCommand("setPlayerName", new Action<string>(SetPlayerNameVariable));
        }



        private static void SetPlayerNameVariable(string data)
        {
            ProjectGameSave.activeFile.playerName = data;
        }
    }
}