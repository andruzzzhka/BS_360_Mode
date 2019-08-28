using System;
using BS_Utils.Utilities;

namespace _360Mode
{
    internal class Config
    {
        private static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("360_Mode");

        public static bool swingMode;
        public static float swingMaxAngle;
        public static float swingAngleIncPerSecond;
        public static bool endlessSwing;

        public static void LoadConfig()
        {
            if (config.HasKey("360_Mode", "swingMode") && config.HasKey("360_Mode", "swingMaxAngle") && config.HasKey("360_Mode", "swingSpeed") && config.HasKey("360_Mode", "endlessSwing"))
            {
                swingMode = config.GetBool("360_Mode", "swingMode", false, false);
                swingMaxAngle = config.GetFloat("360_Mode", "swingMaxAngle", 140f, false);
                swingAngleIncPerSecond = config.GetFloat("360_Mode", "swingSpeed", 2.5f, false);
                endlessSwing = config.GetBool("360_Mode", "endlessSwing", false, false);
            }
            else
            {
                swingMode = false;
                swingMaxAngle = 140f;
                swingAngleIncPerSecond = 2.5f;
                endlessSwing = false;
            }
            SaveConfig();
        }

        public static void SaveConfig()
        {
            config.SetBool("360_Mode", "swingMode", swingMode);
            config.SetFloat("360_Mode", "swingMaxAngle", swingMaxAngle);
            config.SetFloat("360_Mode", "swingSpeed", swingAngleIncPerSecond);
            config.SetBool("360_Mode", "endlessSwing", endlessSwing);
        }
    }
}
