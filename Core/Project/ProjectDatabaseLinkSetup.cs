using UnityEngine;
using static UnityEditor.UIElements.ToolbarMenu;

namespace PROJECT
{
    public class ProjectDatabaseLinkSetup : MonoBehaviour
    {
        public int var_int = 0;
        public float var_flt = 0;
        public bool var_bool = false;
        public string var_str = "";
        public void SetupExtrenalLinks()
        {
            VariableStore.CreateVariable("Project.player", "", () => ProjectGameSave.activeFile.playerName, value => ProjectGameSave.activeFile.playerName = value);
            VariableStore.CreateDatabase("DB_Links");
            VariableStore.CreateVariable("DB_Links.Lint", var_int, () => var_int, value => var_int = value);
            VariableStore.CreateVariable("DB_Links.L_flt", var_flt, () => var_flt, value => var_flt = value);
            VariableStore.CreateVariable("DB_Links.L_bool", var_bool, () => var_bool, value => var_bool = value);
            VariableStore.CreateVariable("DB_Links.L_str", var_str, () => var_str, value => var_str = value);
            
        }
    }
}