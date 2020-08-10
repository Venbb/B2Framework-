namespace shacoEditor
{
    public class ToolsGlobalDefine
    {
        public class MenuPriority
        {
            public enum Viewer
            {
                UI_MANAGER,
                EVENT_MANAGER,
                GUIDE_MANAGER,
                OBSERVER,
                OBJECT_POOL,
                BEHAVIOUR_TREE,
                UI_STATE_CHANGE,
                ASSETBUNDLE_CACHE,
                BEHAVIOUR_RUNTIME_TREE,
            }

            public static class ViewerShortcutKeys
            {
                public const string UI_MANAGER = "%#&1";
                public const string EVENT_MANAGER = "%#&2";
                public const string GUIDE_MANAGER = "%#&3";
                public const string OBSERVER = "%#&4";
                public const string OBJECT_POOL = "%#&5";
                public const string BEHAVIOUR_TREE = "%#&6";
                public const string UI_STATE_CHANGE = "%#&7";
                public const string ASSETBUNDLE_CACHE = "%#&8";
                public const string BEHAVIOUR_RUNTIME_TREE = "%#&9";
            }

            public enum Tools
            {
                FILE_TOOLS,
                BUILD_WINDOW,
                // TEXTURE_SIZE_ANALYSE,
                SPRITE_ATLAS_SETTINGS,
                EXCEL_HELPER_INSPECTOR,
                ASSETBUNDLE_VIEWER,
                LOCALIZATION_REPLACE,
                CHANGE_COMPONENT_DATA,
                SCRIPTS_COUNT_LINE,
                // LOG_LOCATION,

                RUN_GAME = 50,
                PAUSE_OR_RESUME_GAME,
                Stop_Game,
                UPDATE_FRAMEWORK,
                REVERSE_HIERARCHY_GAMEOBJECT_ACTIVE,
            }

            public enum Other
            {
                EXPORT_PACKAGE_1,
                EXPORT_PACKAGE_2,
                MUILTY_PREFABS_APPLY,
                TEST_EDITOR,
                EDITOR_STYLE_PREVIEW,
                INTERNAL_ICON_PREVIEW,
            }
            
            public enum Default
            {
                HOT_UPDATE_EXPORT = 80,
                GAMEH_HELPER,
            }
        }

        public enum ProjectMenuPriority
        {
            CREATE_UI,
            
            CREATE_EXCEL_SCRIPT = 20,
            CREATE_EXCEL_TEXT,
            CREATE_EXCEL_SERIALIZABLE_ASSET,

            CREATE_SCRIPTABLE_OBJECT = 40,
            CREATE_SCRIPTABLE_OBJECT_ASSET,

            CREATE_LUA_BEHAVIOUR_SCRIPT = 60,
            CREATE_RESOURCE_PACK_SETTING,
            CREATE_BEHAVIOUR_PROCESS_SCRIPT,

            // FIND_REFERENCE = 20,
            LUA_SCRIPT_PATH_FIX_HELPER = 21,
        }

        public enum HierachyMenuPriority
        {
            MUILTY_PREFABS_APPLY = -20,
            REFERENCE_BIND,
            RENAME_BY_SEQUEUE,
        }

        public enum InternalAssetMenuPriority
        {
            BUILD_VERION,
            LOCAL_SAVE_DATA,
            DATA_TYPES_SETTING,
            EXCEL_DATA_SETTING,
            SPRITE_ATLAS_SETTING,
        }
    }
}